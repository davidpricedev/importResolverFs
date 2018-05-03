module file

open System
open path
open config
open io
open utils
open parser

let _isEndInList (ends: string seq) (str: string) =
    ends |> Seq.tryFind (fun e -> str.EndsWith(e))

let _isStartInList (starts: string seq) (str: string) =
    starts |> Seq.tryFind (fun s -> str.StartsWith(s))

let _fileFilter (config: ConfigType) =
    stripCwd >> 
        both
            (_isEndInList config.fileTypes)
            (not (_isStartInList config.exclude))

let getProjectFiles config =
    (getAllFiles ".") |> Seq.filter (_fileFilter config)

let relativeToAbsolute relativeToFile relpath =
    pathJoin (dirName relativeToFile) relpath

let absoluteToRef relativeToFile abspath =
    let relpathWithExtn = absoluteToRelative relativeToFile abspath
    (stripExtension >> stripIndex >> addHerePath) relpathWithExtn

let absoluteToRelative (relativeToFile: string) (abspath: string) = 
    if (String.IsNullOrEmpty(abspath)) then None
    else getRelativePath (dirName relativeToFile) abspath

let removeAfterLast (toRemove: string) (source: string) = 
    let i = source.LastIndexOf(toRemove)
    source.Substring(0, i)

let stripExtension = removeAfterLast(".")
let stripIndex = removeAfterLast("/index")

let addHerePath =
    ifElse
        (fun (x: string) -> (x.StartsWith(".")))
        I
        (fun (x: string) -> (x + "./"))

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

let _rawExtensions extns = extns |> Array.append [|""|]

///<summary>
/// import and require allow dropping file extensions
/// import and require also allow a folder name - containing an index file
///</summary>
let _indexExtensions extns = _rawExtensions extns |> Seq.map (concat "/index")

let _getPotentialFileNames filename extns =
    _rawExtensions extns
    |> Seq.append (_indexExtensions extns)
    |> Seq.map (concat (defaultTo "" filename))

let doesFileExistWithExtnLookupRaw excludedExtns filename =
    _getPotentialFileNames(filename, excludedExtns)
    |> Seq.filter doesFileExist
    |> (Seq.isEmpty >> not)

let doesFileExistWithExtnLookup =
    findFilesWithMatchingNames >> Seq.isEmpty >> not

// LINQ?
let _getExisting allFiles potentials =
    innerJoin (fun y x -> endsWith x y) allFiles potentials

let endsWithi (str: string) (ending: string) =
    str.ToLower().EndsWith(ending.ToLower())

let _getExistingi allFiles potentials =
    innerJoin endsWithi allFiles potentials

let findFilesWithMatchingNames allFiles excludedExtns filename =
    _getPotentialFileNames filename excludedExtns
    |> Seq.map (_getExisting allFiles)

let findFilesWithMatchingNamesi allFiles excludedExtns filename =
    _getPotentialFileNames filename  excludedExtns
    |> Seq.map (_getExistingi allFiles)

let getRefsFromFile filename = getRefsFromFileContent (readWholeFile filename)

let replaceContent resolveObj =
    let rawOrigContent = readFile resolveObj.filename
    Maybe.fromString(rawOrigContent)
        .map(_replaceAll(resolveObj.oldPath, resolveObj.resultRef))
        .fold(I, writeFile(resolveObj.filename))

let stringToRegex searchStr = getRegex (_escapeRegExp searchStr "g")

///<summary>
/// borrowed from https://stackoverflow.com/a/17606289/567493
///</summary>
let _escapeRegExp = replace(/[.*+?^${}()|[\]\\]/g, "\\$&")
