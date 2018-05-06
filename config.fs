module Config

open System
open Newtonsoft.Json
open IO

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

let defaultConfig = """
{
    "fileTypes": [".js", ".jsx", ".mjs", ".ts", ".tsx", ".json", ".png"],
    "missingExtensions": [".js", ".jsx", ".mjs", ".ts", ".tsx", ".json"],
    "exclude": [".git", "node_modules", "coverage", "bin"],
    "requireGitClean": false,
    "resolveAlgo": "closest"
}
"""

type ConfigJsonType = {
    fileTypes: string array
    missingExtensions: string array
    exclude: string array
    requireGitClean: bool
    resolveAlgo: string
}

type ConfigType = {
    fileTypes: string seq
    missingExtensions: string seq
    exclude: string seq
    requireGitClean: bool
    resolveAlgo: string
    dryRun: bool
}

let parseConfig = JsonConvert.DeserializeObject<ConfigJsonType>

let hasDryRun args = args |> Seq.exists ((=) "--dry-run")

let toConfigType args (jsonConfig: ConfigJsonType) = {
    fileTypes = jsonConfig.fileTypes
    missingExtensions = jsonConfig.missingExtensions
    exclude = jsonConfig.exclude
    requireGitClean = jsonConfig.requireGitClean
    resolveAlgo = jsonConfig.resolveAlgo
    dryRun = hasDryRun args }
    
let readAndParse = prefixCwd >> readWholeFile >> parseConfig
    
let findBestConfig x =
    match x with 
    | Some x when (doesFileExist x) -> readAndParse x
    | _ when (doesFileExist EXPECTED_CONFIG_NAME) -> readAndParse EXPECTED_CONFIG_NAME
    | _ -> parseConfig defaultConfig

let getConfig args =
    args
    |> Array.tryLast
    |> Spy.inspect "arg-last"
    |> findBestConfig
    |> toConfigType args
