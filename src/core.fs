module Core

open Config
open Utils
open result
open File
open Path

let getResolveObj filename oldPath =
    buildResolveObj filename oldPath (relativeToAbsolute filename oldPath)

let getBrokenRefs doesExist isNpm filename =
    getRefsFromFile filename
    |> Seq.filter (not << isNpm)
    |> Seq.map (getResolveObj filename)
    |> Seq.filter (not << doesExist)

let refExists allFiles excludedExtensions refObj =
    doesFileExistWithExtnLookup
        allFiles
        excludedExtensions
        refObj.fullOldPath

let findPotentials allFiles config fileAndRef = 
    let potentials = findFilesWithMatchingNamesi allFiles config.missingExtensions (baseName fileAndRef.oldPath)
                    |> Seq.choose (getRelativePath fileAndRef.filename)

    let potentialsOpt =
        if Seq.isEmpty potentials then None else Some potentials 

    { fileAndRef with potentials = potentialsOpt }

let displayChange x = 
    sprintf "[%s]: \n\t    %s\n\t -> %s" x.filename x.oldPath (optToStr x.resultRef)

let displayError x =
    sprintf "[%s]: \n\t%s\n\t ~ %s" x.filename x.oldPath (optToStr x.message)

let display = ifElse (fun x -> Option.isSome x.message) displayError displayChange

let applyChange x = 
    printfn "applying change"
    replaceContent x
    display x

let applyOrDisplay = ifElse (fun x -> x.dryRun) (K display) (K applyChange)
