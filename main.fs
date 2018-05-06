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

// sort of doesn't work.
let getBrokenRefs doesExist isNpm filename =
    getRefsFromFile filename
    |> Seq.filter (not << isNpm)
    |> Seq.map (getResolveObj filename)
    |> Seq.filter (not << doesExist)

// works but doesn't include the files that are really broken
let getBrokenRefs2 doesExist isNpm filename =
    getRefsFromFile filename
    |> Seq.filter (not << isNpm)
    |> Seq.filter doesFileExist
    |> Seq.map (getResolveObj filename)

let refExists allFiles excludedExtensions refObj =
    doesFileExistWithExtnLookup
        allFiles
        excludedExtensions
        refObj.fullOldPath

let findPotentials allFiles config fileAndRef = 
    let potentials = findFilesWithMatchingNamesi allFiles config.missingExtensions (baseName fileAndRef.oldPath)

    let potentialsOpt =
        if Seq.isEmpty potentials then None else Some potentials 

    { fileAndRef with potentials = potentialsOpt }

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
    printfn "Gathing Project Files"

    let config = getConfig argv
    let allFiles = getProjectFiles config
    let allNpms = getAllNpms

    printfn "using config: %A" config
    //printfn "files: %A, %A" (Seq.length allFiles) (Seq.toList allFiles)
    let listToString = (Seq.map (sprintf "%A")) >> (String.concat ", ") >> (sprintf "[%s]")
    printfn "files: %A" (Seq.length allFiles)
    printfn "npms: %A" (Seq.length allNpms)
    printfn "cwd: %A" cwd

    let myRefExists = refExists allFiles config.missingExtensions
    let myIsNpmPath = isNpmPath allNpms

    allFiles
    |> Spy.aside "Looking for broken references"
    |> Seq.collect (getBrokenRefs myRefExists myIsNpmPath)
    //|> (fun x -> (Spy.inspect "1st length" (Seq.length x)); x)
    |> Spy.inspect "first pass"
    // find all potential ref matches
    |> Seq.map (findPotentials allFiles config) 
    |> Spy.inspect "second pass" 
    // use some algorithms to find the best ones
    |> Seq.map (resolveRef allFiles resolve) 
    |> Spy.inspect "third pass" 
    // pick the algorithm solution specified in the config
    |> Seq.map (resultToRef config) 
    |> Spy.inspect "fourth pass" 
    // apply all the changes (or display what we would change if --dry-run was specified)
    |> Seq.map (applyOrDisplay config)
    (*
    |> Seq.map (
        // find all potential ref matches
        (findPotentials allFiles config) >>
        (Spy.inspect "second pass") >>
        // use some algorithms to find the best ones
        (resolveRef allFiles resolve) >>
        (Spy.inspect "third pass") >>
        // pick the algorithm solution specified in the config
        (resultToRef config) >>
        (Spy.inspect "fourth pass") >>
        // apply all the changes (or display what we would change if --dry-run was specified)
        (applyOrDisplay config))
*)