module Resolve

open System
open Utils
open EditDistance
open result
open Closest
open First
open Random

let algorithms = [applyFirst; applyRandom; applyClosest; applyEditDistance]

let resolve fileAndRef =
    algorithms 
    |> List.fold (fun far algo -> algo far) fileAndRef
