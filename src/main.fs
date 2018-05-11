module Main

open System
open Core
open Config
open Utils
open IO
open result
open File
open Resolve
open Path

///<remarks>
/// Technically I could use collect instead of map >> filter >> concat
///  but the performance of collect with all the empty collections is abysmal.
///</remarks>
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
    //|> Seq.collect (fun x -> Spy.profile (sprintf "getBrokenRefs for %s" x) (fun () -> getBrokenRefs myRefExists myIsNpmPath x))
    |> Array.map (getBrokenRefs myRefExists myIsNpmPath)
    |> Spy.aside "Ignoring files without broken references"
    |> Array.filter (not << Seq.isEmpty)
    |> Seq.concat
    |> Spy.inspect "Identifying potential solutions"
    |> Seq.map (findPotentials allFiles config) 
    |> Spy.inspect "Finding the best solution"
    |> Seq.map (resolveRef allFiles resolve) 
    |> Spy.inspect "best phase2"
    |> Seq.map (resultToRef config) 
    |> Spy.inspect "fourth pass" 
    // apply all the changes (or display what we would change if --dry-run was specified)
    |> Seq.fold (fun a i -> a + (applyOrDisplay config i)) ""
    |> printfn "%s"
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