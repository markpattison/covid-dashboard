module Cache

open Microsoft.Extensions.Caching.Memory

open Shared

type CacheKey =
    | Populations
    | Boundaries
    | LtlaData
    | LtlaJoined

let populations (cache: IMemoryCache) =
    async {
        printfn "Asking for population data..."
        match cache.TryGetValue Populations with
        | true, ( :? Lazy<Map<ONSCode, float>> as lazyResult ) ->
            let result = lazyResult.Value
            printfn "Population data from cache"
            return result
        | _ ->
            let lazyResult = lazy (Populations.read "./data/population_estimates.csv")
            cache.Set(Populations, lazyResult) |> ignore
            let result = lazyResult.Value
            printfn "Population data"
            return result
    }

let boundaries (cache: IMemoryCache) =
    async {
        printfn "Asking for boundary data..."
        match cache.TryGetValue Boundaries with
        | true, ( :? Lazy<(ONSCode * AreaName * Boundary) array> as lazyResult ) ->
            let result = lazyResult.Value
            printfn "Boundary data from cache"
            return result
        | _ ->
            let lazyResult = lazy (Geography.readBoundaries "./data/Local_Authority_Districts__December_2019__Boundaries_UK_BUC.kml")
            cache.Set(Boundaries, lazyResult) |> ignore
            let result = lazyResult.Value
            printfn "Boundary data"
            return result
    }

let ltlaData (cache: IMemoryCache) =
    async {
        printfn "Asking for LTLA data..."
        match cache.TryGetValue LtlaData with
        | true, ( :? Lazy<Map<ONSCode, CovidData.AreaData>> as lazyResult ) ->
            let result = lazyResult.Value
            printfn "LTLA data from cache"
            return result
        | _ ->
            let lazyResult = lazy (CovidData.getData LowerTierLocalAuthority |> Async.RunSynchronously)
            cache.Set(LtlaData, lazyResult) |> ignore
            let result = lazyResult.Value
            printfn "LTLA data"
            return result
    }

let ltlaJoined (cache: IMemoryCache) =
    async {
        printfn "Asking for joined data..."
        match cache.TryGetValue LtlaJoined with
        | true, ( :? Lazy<Area []> as lazyResult ) ->
            let result = lazyResult.Value
            printfn "Joined data from cache"
            return result
        | _ ->
            let! pop = populations cache
            let! bounds = boundaries cache
            let! ltla = ltlaData cache

            let lazyResult = lazy (JoinData.join ltla pop bounds)
            cache.Set(LtlaJoined, lazyResult) |> ignore

            let result = lazyResult.Value
            printfn "Joined data"
            return result
    }
