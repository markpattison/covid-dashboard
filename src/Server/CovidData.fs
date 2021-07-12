module CovidData

open System
open System.Collections.Generic
open Cov19API

open Shared

type AreaData =
    {
        NewCasesBySpecimenDate: Map<DateTime, float>
    }

type AllData =
    {
        LowerTierLocalAuthorities: Map<ONSCode, AreaData>
    }

type ApiData () =
    member val Date : DateTime = DateTime.MinValue with get, set
    member val AreaCode : string = "" with get, set
    member val NewCases : float = 0.0 with get, set

let private apiForAreaType areatype =
    let filters = dict [ "areaType", areatype ] |> Dictionary
    let structure = dict [ "Date", "date"; "NewCases", "newCasesBySpecimenDate"; "AreaCode", "areaCode" ] |> Dictionary
    let props = UkCovid19Props(FiltersType = filters, StructureType = structure)
    Cov19Api(props)

let ltlaApi = apiForAreaType "ltla"
let utlaApi = apiForAreaType "utla"
let regionApi = apiForAreaType "region"

let private getApi areaType =
    match areaType with
    | Region -> regionApi
    | UpperTierLocalAuthority -> utlaApi
    | LowerTierLocalAuthority -> ltlaApi

let private getApiData (api : Cov19Api) =
    async {
        let! result = api.Get<ApiData>() |> Async.AwaitTask
        return result.Data
    }

let private toDateMap (apiData: ApiData seq) =
    apiData
    |> Seq.map (fun d -> d.Date, d.NewCases)
    |> Map.ofSeq

let private toAreaMap (apiData: ApiData seq) =
    apiData
    |> Seq.groupBy (fun d -> d.AreaCode)
    |> Seq.map (fun (code, d) -> ONSCode code, { NewCasesBySpecimenDate = toDateMap d })
    |> Map.ofSeq

let getData areaType =
    async {
        let! data = areaType |> getApi |> getApiData
        return toAreaMap data
    }

let getLastModified (api : Cov19Api) =
    async {
        let! result = api.LastUpdate() |> Async.AwaitTask
        return result
    }
