module Main

open System
open Config
open Utils
open IO
open result
open File
open Resolve
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

    let potentialsOpt =
        if potentials |> Seq.isEmpty then None else Some potentials 

    { fileAndRef with 
        potentials = potentialsOpt }

let resolveRef allFiles resolver fileAndRef =
    (resolver fileAndRef)

let getResult config (resolveObj: Result) =
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
            

let displayChange x = 
    printfn "[%s]: \n\t%s\n\t -> %s" x.filename x.oldPath (optToStr x.resultRef)

let displayError x =
    printfn "[%s]: \n\t%s\n\t ~ %s" x.filename x.oldPath (optToStr x.message)

let display = ifElse (fun x -> Option.isSome x.message) displayError displayChange

let applyChange x = 
    replaceContent x
    display x

let applyOrDisplay = ifElse (fun x -> x.dryRun) (K display) (K applyChange)

let run argv = 
    let config = getConfig argv
    let allFiles = getProjectFiles config
    let allNpms = getAllNpms

    let myRefExists = refExists allFiles config.missingExtensions
    let myIsNpmPath = isNpmPath allNpms

    allFiles
    //|> log "Looking for broken references")
    |> Seq.map (getBrokenRefs myRefExists myIsNpmPath)
    // remove files that don't have any broken refs
    |> Seq.filter (Seq.isEmpty >> not)
    |> Seq.concat
    //|> log "Looking for potential solutions")
    |> Seq.map (
        // find all potential ref matches
        (findPotentials allFiles config) >>
        // use some algorithms to find the best ones
        (resolveRef allFiles resolve) >>
        // pick the algorithm solution specified in the config
        (resultToRef config) >>
        // apply all the changes (or display what we would change if --dry-run was specified)
        (applyOrDisplay config))
