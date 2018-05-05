///<summary>
/// Shortest distance between the file and the refpath option.
/// Good for converting to better app-like organization structure
///  (where apart from common utils, files should be close to the files they reference).
///</summary>
module Closest 

open Path
open result
open ResolveUtil

let closestCalc (x: string) = (x, x.Split(pathSep).Length)

let applyClosest fileAndRef =
    { fileAndRef with
        closest = getBest closestCalc minReducer fileAndRef }
