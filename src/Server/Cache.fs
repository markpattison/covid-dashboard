module Cache

open Microsoft.Extensions.Caching.Memory

open Shared

type CacheKey =
    | Populations
    | Boundaries
    | LtlaData
    | LtlaJoined
    member this.DisplayAs =
        match this with
        | Populations -> "population"
        | Boundaries -> "boundary"
        | LtlaData -> "LTLA covid rates"
        | LtlaJoined -> "joined LTLA"

let getCached (cacheKey: CacheKey) (f: IMemoryCache -> 'a) (cache: IMemoryCache) =
    async {
        printfn "Asking for %s data..." cacheKey.DisplayAs

        match cache.TryGetValue cacheKey with
        | true, ( :? Lazy<'a> as lazyResult ) ->
            let result = lazyResult.Value

            printfn "Found %s data in cache" cacheKey.DisplayAs
            return result
        | _ ->
            let stopwatch = System.Diagnostics.Stopwatch()
            stopwatch.Start()

            let getLazy = lazy f cache
            cache.Set(cacheKey, getLazy) |> ignore
            let result = getLazy.Value

            printfn "Calculated %s data in %ims" cacheKey.DisplayAs stopwatch.ElapsedMilliseconds

            return result
    }

let populations = getCached Populations (fun _ -> Populations.read "./data/population_estimates.csv")

let boundaries = getCached Boundaries (fun _ -> Geography.readBoundaries "./data/Local_Authority_Districts__December_2019__Boundaries_UK_BUC.kml")

let ltlaData = getCached LtlaData (fun _ -> CovidData.getData LowerTierLocalAuthority |> Async.RunSynchronously)

let ltlaJoined : IMemoryCache -> Async<Area []> = getCached LtlaJoined (fun cache ->
    async {
        let! pop = populations cache
        let! bounds = boundaries cache
        let! ltla = ltlaData cache

        let result = JoinData.join ltla pop bounds

        return result
    } |> Async.RunSynchronously)
