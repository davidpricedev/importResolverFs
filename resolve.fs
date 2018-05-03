module resolve

open System
open utils
open editDistance
open Result

let isValid fileAndRef =
    fileAndRef.potentials &&
    fileAndRef.potentials.isNonEmpty()

let resolveValidation fileAndRef =
    if not (isValid fileAndRef) then
        None
    else Some fileAndRef

let first (fileAndRef: Result) = { fileAndRef with first = (fileAndRef.potentials |> Seq.head) }

let _getFirst fileAndRef =
    fileAndRef.potentials |> Seq.head
        
let closestMap x fileandRef = { fileAndRef with refpath = x; pathDist = x }

//let setrefpath = set(lensProp("refpath"))

let calcPathDistance = split dirPath.sep >> prop "length"

let setPathDist = calcPathDistance >> set (lensProp "pathDist")

///<summary>
/// Shortest distance between the file and the refpath option.
/// Good for converting to better app-like organization structure
///  (where apart from common utils, files should be close to the files they reference).
///</summary>
let closest fileAndRef = { fileAndRef with closest = (_getClosest fileAndRef) }

let _getClosest fileAndRef =
    fileAndRef
    |> .potentials.map closestMap
    |> Seq.fold (minBy (prop "pathDist")) (objOf "pathDist" Infinity)
    |> fun x -> x.refPath

/**
 * Shortest edit distance between old and new paths.
 * Good for pluralization changes to folders
 */
let editDistance fileAndRef =
    objOf "minDistance" (_getEditDistance fileAndRef)

let _getEditDistance fileAndRef =
    fileAndRef
        .fold()
        .potentials.map (editDistMap (fileAndRef.fold().oldPath))
        .fold (minBy (prop "minDistance")) (objOf "minDistance" Infinity)
        .refpath

let editDistMap oldPath x =
    (setrefpath x) >> (setEditDist oldPath x)
let setEditDist oldPath =
    (calculateDistance oldPath) >> (set (lensProp "minDistance"))

/**
 * Not really good for anything...
 */
let random fileAndRef =
    objOf "random" (randomElem (fileAndRef.fold().potentials))

let randomElem list = list |> Seq.nth (randomRange 0, (list |> Seq.Length))

let randomRange lower upper = toRange lower upper Math.random()

let toRange lower upper seed =
    lower + Math.floor((upper - lower) * seed)

let algorithms () = [|first; random; closest; editDistance|]

let resolve fileAndRef =
    algorithms() 
    |> Array.map (T (resolveValidation fileAndRef))
    |> Array.fold merge 
