module CovidData

open System
open System.Collections.Generic
open Cov19API

open Shared

type CovidData =
    {
        ONSCode: ONSCode
        Date: DateTime
        NewCasesBySpecimenDate: float
    }

type ApiData () =
    member val Date : DateTime = DateTime.MinValue with get, set
    member val AreaName : string = "" with get, set
    member val AreaCode : string = "" with get, set
    member val NewCases : int = 0 with get, set



let filters = dict [ "areaType", "ltla" ] |> Dictionary
let structure = dict [ "Date", "date"; "NewCases", "newCasesBySpecimenDate"; "AreaName", "areaName"; "AreaCode", "areaCode" ] |> Dictionary

let props = UkCovid19Props(FiltersType = filters, StructureType = structure)

let api = Cov19Api(props)

let getLastModified =
    async {
        let! result = api.LastUpdate() |> Async.AwaitTask
        return result
    }

let getData =
    async {
        let! result = api.Get<ApiData>() |> Async.AwaitTask
        return result.Data
    }
