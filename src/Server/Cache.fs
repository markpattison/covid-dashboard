module Cache

open System
open Microsoft.Extensions.Caching.Memory

open Shared

type CacheKey =
    | Populations
    | Boundaries
    | LtlaData
    | LtlaJoined
    | Dates
    member this.DisplayAs =
        match this with
        | Populations -> "population data"
        | Boundaries -> "boundary data"
        | LtlaData -> "LTLA covid rates"
        | LtlaJoined -> "joined LTLA data"
        | Dates -> "dates"

let getCached (cacheKey: CacheKey) (f: IMemoryCache -> 'a) (cache: IMemoryCache) =
    async {
        printfn "Asking for %s..." cacheKey.DisplayAs

        match cache.TryGetValue cacheKey with
        | true, ( :? Lazy<'a> as lazyResult ) ->
            let result = lazyResult.Value

            printfn "Found %s in cache" cacheKey.DisplayAs
            return result
        | _ ->
            let stopwatch = System.Diagnostics.Stopwatch()
            stopwatch.Start()

            let getLazy = lazy f cache
            cache.Set(cacheKey, getLazy) |> ignore
            let result = getLazy.Value

            printfn "Calculated %s in %ims" cacheKey.DisplayAs stopwatch.ElapsedMilliseconds

            return result
    }

let populations = getCached Populations (fun _ -> Populations.read "./data/population_estimates.csv")

let boundaries = getCached Boundaries (fun _ -> Geography.readBoundaries "./data/Local_Authority_Districts__December_2019__Boundaries_UK_BUC.kml")

let ltlaData = getCached LtlaData (fun _ -> CovidData.getData LowerTierLocalAuthority |> Async.RunSynchronously)

let ltlaJoined : IMemoryCache -> Async<Area []> = getCached LtlaJoined (fun cache ->
    async {
        let! ltla = ltlaData cache
        let! bounds = boundaries cache
        let! pop = populations cache

        let result = JoinData.join ltla pop bounds

        return result
    } |> Async.RunSynchronously)

let dates : IMemoryCache -> Async<DateTime []> = getCached Dates (fun cache ->
    async {
        let! ltla = ltlaData cache

        let allDates =
            ltla
            |> Map.toSeq
            |> Seq.collect (fun (_, areaData) ->
                areaData.NewCasesBySpecimenDate
                |> Map.toSeq
                |> Seq.map fst)
            |> Seq.distinct
            |> Seq.sort
            |> Seq.toArray
        
        return allDates
    } |> Async.RunSynchronously)
