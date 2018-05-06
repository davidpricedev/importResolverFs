module File

open System
open result
open Path
open Config
open IO
open Utils
open Parser

let private isEndInList (ends: string seq) (str: string) =
    ends
    |> Seq.tryFind (fun e -> endsWith e str)
    |> Option.isSome

let private isStartInList (starts: string seq) (str: string) =
    starts
    |> Seq.tryFind (fun s -> startsWith s str)
    |> Option.isSome

let private fileFilter (config: ConfigType) =
    both (isEndInList config.fileTypes) (not << (isStartInList config.exclude))

let getProjectFiles config =
    (getAllFiles ".") |> Array.filter (stripCwd >> (fileFilter config))

let relativeToAbsolute relativeToFile =
    pathJoin (dirName relativeToFile)

let absoluteToRelative (relativeToFile: string) (abspath: string) = 
    getRelativePath (dirName relativeToFile) abspath

let removeAfterLast (toRemove: string) (source: string) = 
    let i = source.LastIndexOf toRemove
    source.Substring(0, i)

let stripExtension = removeAfterLast(".")
let stripIndex = removeAfterLast("/index")

let addHerePath =
    ifElse (startsWith ".") I (fun x -> (x + "./"))

let absoluteToRef relativeToFile abspath =
    absoluteToRelative relativeToFile abspath
    |> Option.map (stripExtension >> stripIndex >> addHerePath)

let isNpmPath allNpms (refpath: string) =
    if (String.IsNullOrEmpty(refpath)) then false
    else 
        // handle sub-nav into npm modules i.e. `import ... from 'redux-saga/effects'`
        let parts = refpath.Split(pathSep, StringSplitOptions.None)
        let firstPathPart = parts |> Array.head
        if ((firstPathPart.StartsWith(".")) && (not (refpath.Contains("node_modules")))) then
            false
        else 
            allNpms |> Seq.contains firstPathPart

let private rawExtensions extns = extns |> Seq.append [""]

///<summary>
/// import and require allow dropping file extensions
/// import and require also allow a folder name - containing an index file
///</summary>
let private indexExtensions extns = rawExtensions extns |> Seq.map (fun x -> "/index" + x)

let private getPotentialFileNames filename extns =
    rawExtensions extns
    |> Seq.append (indexExtensions extns)
    |> Seq.map (fun x -> filename + x)

let doesFileExistWithExtnLookupRaw excludedExtns filename =
    getPotentialFileNames filename excludedExtns
    |> Seq.filter doesFileExist
    |> (Seq.isEmpty >> not)

let private getExisting allFiles potentials =
    innerJoin (fun y x -> endsWith x y) allFiles potentials

let endsWithi (str: string) (ending: string) =
    endsWith (ending.ToLower()) (str.ToLower())

let private getExistingi allFiles potentials =
    innerJoin endsWithi allFiles potentials

let findFilesWithMatchingNames allFiles excludedExtns filename =
    getPotentialFileNames filename excludedExtns
    |> (getExisting allFiles)

let findFilesWithMatchingNamesi allFiles excludedExtns filename =
    getPotentialFileNames filename  excludedExtns
    |> (getExistingi allFiles)

let doesFileExistWithExtnLookup allFiles excludedExtns filename =
    findFilesWithMatchingNames allFiles excludedExtns filename
    |> (Seq.isEmpty >> not)

let getRefsFromFile filename = getRefsFromFileContent (readWholeFile filename)

let private replaceAll (oldValue: string) newValue (source: string) =
    source.Replace(oldValue, newValue)

let replaceContent (resolveObj: Result) =
    let iReplaceContent resultRef =
        readWholeFile >>
        (replaceAll resolveObj.oldPath resultRef) >>
        (writeFile resolveObj.filename)

    match resolveObj.resultRef with
    | None -> ()
    | Some resultRef -> 
        iReplaceContent resultRef resolveObj.filename
