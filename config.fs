module config

open System
open FSharp.Data
open io

let EXPECTED_CONFIG_NAME = "./importResolver.json"

(*
export type Config = {
    // file types to examine
    fileTypes: string[],


    // file extensions that are missing from references
    //  (import './config' has no extension but probably means js or jsx)
    missingExtensions: string[],

    // folders to exclude 
    // TODO: replace with proper glob handling
    exclude: string[],

    // future: support for mandating no other changes
    requireGitClean: boolean,

    // What algoritthm to use for solving imports that are broken.
    // Needed to handle cases where the same file name appears in multiple locations throughout the tree.
    //  * first - picks the first filepath with the filename that matches
    //  * random - random
    //  * closest - picks the minimum path where each '../' and folder counts as 1
    //  * minDistance - uses https://en.wikipedia.org/wiki/Edit_distance algos to compute distance
    // TODO: Find a smart way to combine several of these (or maybe a few smart ways)
    resolveAlgo: string
};
*)

type configJsonType = JsonProvider<"""
{
    "fileTypes": [".js", ".jsx", ".mjs", ".ts", ".tsx", ".json", ".png"],
    "missingExtensions": [".js", ".jsx", ".mjs", ".ts", ".tsx", ".json"],
    "exclude": [".git", "node_modules", "coverage"],
    "requireGitClean": false,
    "resolveAlgo": "closest"
}
""">

type configType = {
    fileTypes: string seq
    missingExtensions: string seq
    exclude: string seq
    requireGitClean: bool
    resolveAlgo: string
    dryRun: bool
}

let defaultConfig = configJsonType.GetSample()

let hasDryRun args = args |> Seq.exists ((=) "--dry-run")

let toConfigType args jsonConfig = {
    fileTypes = jsonConfig.fileTypes
    missingExtensions = jsonConfig.missingExtensions
    exclude = jsonConfig.exclude
    requireGitClean = jsonConfig.requireGitClean
    resolveAlgo = jsonConfig.resolveAlgo
    dryRun = hasDryRun args }
    
let getAndParse x = (prefixCwd >> readWholeFile) x |> configJsonType.Parse
    
let findBestConfig x =
    match x with 
    | Some x when (doesFileExist x) -> getAndParse x
    | _ when (doesFileExist EXPECTED_CONFIG_NAME) -> getAndParse EXPECTED_CONFIG_NAME
    | _ -> defaultConfig

let getConfig args =
    args
    |> Array.tryLast
    |> findBestConfig
    |> (fun x -> toConfigType args x)

