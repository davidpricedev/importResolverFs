module File

open System
open result
open Path
open Config
open IO
open Utils
open Parser

let _isEndInList (ends: string seq) (str: string) =
    ends
    |> Seq.tryFind (fun e -> endsWith e str)
    |> Option.isSome

let _isStartInList (starts: string seq) (str: string) =
    starts
    |> Seq.tryFind (fun s -> startsWith s str)
    |> Option.isSome

let _projectFileFilter (config: ConfigType) =
    both (_isEndInList config.fileTypes) (not << (_isStartInList config.exclude))

let getProjectFiles config =
    (getAllFiles ".") |> Array.filter (stripCwd >> (_projectFileFilter config))

let relativeToAbsolute relativeToFile =
    pathJoin (dirName relativeToFile)

let absoluteToRelative (relativeToFile: string) (abspath: string) = 
    getRelativePath (dirName relativeToFile) abspath

let removeAfterLast (toRemove: string) (source: string) = 
    let i = source.LastIndexOf toRemove
    if i <= 0 then source else source.Substring(0, i)

let stripExtension = removeAfterLast(".")
let stripIndex = removeAfterLast("/index")

let addHerePath =
    ifElse (startsWith ".") id (fun x -> ("./" + x))

let absoluteToRef relativeToFile abspath =
    absoluteToRelative relativeToFile abspath
    |> Option.map (stripExtension >> stripIndex >> addHerePath)

let private isRelativeNpm (refpath: string) =
    refpath.Contains "node_modules"

let private isDirectRef refpath =
    (not (refpath |> startsWith "."))

let private startsWithAnNpm allNpms (refpath: string) =
    let firstPathPart = refpath.Split(pathSep, StringSplitOptions.None) |> Array.head
    allNpms |> Seq.contains firstPathPart
    
let isNpmPath allNpms refpath =
    if (String.IsNullOrEmpty(refpath)) then false
    else 
        (refpath |> isRelativeNpm) ||
        (refpath |> isDirectRef) ||
        (refpath |> startsWithAnNpm allNpms)


let private rawExtensions extns = extns |> Seq.append [""]

///<summary>
/// import and require allow dropping file extensions
/// import and require also allow a folder name - containing an index file
///</summary>
let private indexExtensions extns = rawExtensions extns |> Seq.map (fun x -> "/index" + x)

let _getPotentialFileNames filename extns =
    rawExtensions extns
    |> Seq.append (indexExtensions extns)
    |> Seq.map (fun x -> filename + x)

let doesFileExistWithExtnLookupRaw excludedExtns filename =
    _getPotentialFileNames filename excludedExtns
    |> Seq.filter doesFileExist
    |> (Seq.isEmpty >> not)

let _getExisting allFiles potentials =
    innerJoin (fun y x -> endsWith x y) allFiles potentials

let endsWithi (str: string) (ending: string) =
    endsWith (ending.ToLower()) (str.ToLower())

let _getExistingi allFiles potentials =
    innerJoin endsWithi allFiles potentials

let findFilesWithMatchingNames allFiles excludedExtns filename =
    _getPotentialFileNames filename excludedExtns
    |> (_getExisting allFiles)

let findFilesWithMatchingNamesi allFiles excludedExtns filename =
    _getPotentialFileNames filename  excludedExtns
    |> (_getExistingi allFiles)

let doesFileExistWithExtnLookup allFiles excludedExtns filename =
    findFilesWithMatchingNames allFiles excludedExtns filename
    |> (Seq.isEmpty >> not)

let getRefsFromFile filename = getRefsFromFileContent (readWholeFile filename)

let _replaceAll (oldValue: string) newValue (source: string) =
    source.Replace(oldValue, newValue)

let replaceContent (resolveObj: Result) =
    let iReplaceContent resultRef =
        readWholeFile >>
        (_replaceAll resolveObj.oldPath resultRef) >>
        (writeFile resolveObj.filename)

    match resolveObj.resultRef with
    | None -> ()
    | Some resultRef -> 
        iReplaceContent resultRef resolveObj.filename
