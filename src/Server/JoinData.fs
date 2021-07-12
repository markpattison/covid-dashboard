module JoinData

open System

open CovidData
open Shared

let private totalCasesInWeekTo (covidData: AreaData) (date: DateTime) =
    let weekBefore = date.AddDays(-6.0)

    covidData.NewCasesBySpecimenDate
    |> Map.toSeq
    |> Seq.filter (fun (d, _) -> d >= weekBefore && d <= date)
    |> Seq.sumBy snd

let private extractRates (areaData: AreaData) population =

    let dates = areaData.NewCasesBySpecimenDate |> Map.toSeq |> Seq.map fst

    let weeklyRates =
        dates
        |> Seq.map (fun date -> date, (totalCasesInWeekTo areaData date) * 100000.0 / population)

    { WeeklyCasesPer100k = Map.ofSeq weeklyRates }

let join (covidData: Map<ONSCode, AreaData>) populations boundaries =

    let getArea (onsCode, name, boundary) =

        let population = Map.tryFind onsCode populations
        let areaData = Map.tryFind onsCode covidData

        let covidRates =

            match population, areaData with
            | Some pop, Some data when pop > 0.0 -> extractRates data pop |> Some
            | _ -> None

        {
            ONSCode = onsCode
            Name = name
            Boundary = boundary
            Data = covidRates
        }

    boundaries |> Array.map getArea
