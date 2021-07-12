module Server

open System
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared

let allData = 

let covidMapApi =
    { getDates = fun () -> async { return [||] }
      getData = fun () -> async { return [||] }
    }

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue covidMapApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

// let ltlaData = Async.RunSynchronously (CovidData.getData LowerTierLocalAuthority)
// let populations = Populations.read "./data/population_estimates.csv"
// let boundaries = Geography.readBoundaries "./data/Local_Authority_Districts__December_2019__Boundaries_UK_BUC.kml"

// let joined = JoinData.join ltlaData populations boundaries

// printfn "LTLAs: %i" joined.Length

run app
