///<summary>
/// Shortest edit distance between old and new paths.
/// Good for pluralization changes to folders
///</summary>
module EditDistance

open result
open EditDistanceAlgo
open ResolveUtil

let editDistanceCalc fileAndRef x =
    (x, calculateDistance fileAndRef.oldPath x)

let applyEditDistance fileAndRef =
    { fileAndRef with
        editDistance = getBest (editDistanceCalc fileAndRef) minReducer fileAndRef }
