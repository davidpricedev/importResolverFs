module Resolve

open EditDistance
open result
open Closest
open First
open Random
open File

let algorithms = [applyFirst; applyRandom; applyClosest; applyEditDistance]

let resolve fileAndRef =
    algorithms 
    |> List.fold (fun far algo -> algo far) fileAndRef

let resolveRef allFiles resolver fileAndRef =
    (resolver fileAndRef)

let getResult (config: Config.ConfigType) (resolveObj: Result) =
    match config.resolveAlgo with
    | "first" -> resolveObj.first 
    | "random" -> resolveObj.random 
    | "closest" -> resolveObj.closest 
    | "editDistance" -> resolveObj.editDistance 
    | _ -> resolveObj.first

let resultToRef config resolveObj = 
    let iResultToRef =
        getResult config >>
        Option.bind (absoluteToRef resolveObj.filename)

    { resolveObj with 
        resultRef = iResultToRef resolveObj }
            