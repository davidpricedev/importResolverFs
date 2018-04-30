module resolve

open System
open utils
open editDistance

let resolveValidation = fileAndRef =
    if not (isValid fileAndRef) then
        let defaultObj = { potentials: List.of(), oldPath: "" }
        return Maybe.None(merge(defaultObj, fileAndRef))
    }

    return Maybe.Some(fileAndRef)
}

let isValid = fileAndRef =
    fileAndRef.potentials &&
    List.isList(fileAndRef.potentials) &&
    fileAndRef.potentials.isNonEmpty() &&
    fileAndRef.oldPath

/**
 * Not really any better than random...
 */
let first fileAndRef = objOf "first" (_getFirst fileAndRef)

let _getFirst fileAndRef =
    fileAndRef
        .fold()
        .potentials.maybeHead()
        .fold()
        
let closestMap x = setrefpath x >> setPathDist x
let setrefpath = set(lensProp("refpath"))
let calcPathDistance = split dirPath.sep >> prop "length"
let setPathDist = calcPathDistance >> set (lensProp "pathDist")

/**
 * Shortest distance between the file and the refpath option.
 * Good for converting to better app-like organization structure
 *  (where apart from common utils, files should be close to the files they reference).
 */
let closest fileAndRef = objOf "closest" (_getClosest fileAndRef)

let _getClosest fileAndRef =
    fileAndRef
        .fold()
        .potentials.map closestMap
        .fold (minBy (prop "pathDist")) (objOf "pathDist" Infinity)
        .refPath

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
