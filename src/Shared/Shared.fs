namespace Shared

open System

type Todo = { Id: Guid; Description: string }

module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) =
        { Id = Guid.NewGuid()
          Description = description }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type ITodosApi =
    { getTodos: unit -> Async<Todo list>
      addTodo: Todo -> Async<Todo> }

type Loop =
    {
        LatLongs: (float * float) []
    }

type Shape =
    {
        OuterBoundary: Loop
        Holes: Loop []
    }

type Boundary =
    {
        Shapes: Shape []
    }

type CovidRates =
    {
        WeeklyCasesPer100k: Map<DateTime, float>
    }

type ONSCode = | ONSCode of string

type Area =
    {
        ONSCode: ONSCode
        Name: string
        Boundary: Boundary
        Data: CovidRates option
    }

// fsharplint:disable RecordFieldNames

type ICovidMapApi =
    { getDates : unit -> Async<DateTime []>
      getData : unit -> Async<Area []>
    }
