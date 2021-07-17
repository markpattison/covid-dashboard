module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Caching.Memory
open Microsoft.Extensions.DependencyInjection
open Saturn

open Shared

let withCacheFromContext f (context: HttpContext) =
    let cache = context.GetService<IMemoryCache>()
    f cache

let covidMapApi cache =
    { getDates = fun () -> Cache.dates cache
      getData = fun () -> Cache.ltlaJoined cache
    }

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromContext (withCacheFromContext covidMapApi)
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
        service_config (fun s -> s.AddMemoryCache())
    }

run app
