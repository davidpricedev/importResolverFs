module file

open System
open io
open utils
open parser

let getProjectFiles config =
    (getAllFiles ".") |> Seq.filter (_fileFilter config)

let _fileFilter config =
    stripCwd >> 
        allTrue
            _isEndInList config.fileTypes
            not (_isStartInList config.exclude) 

let _isEndInList ends str =
    ends
    |> Seq.map (fun e -> str.EndsWith(e))
    |> Seq.fold reduceOr false
    
let _isStartInList starts str =
    starts
    |> Seq.map (fun s -> str.StartsWith(s))
    |> Seq.fold reduceOr false

let relativeToAbsolute relativeToFile relpath =
    if not relpath then None 
    else Some path.join(path.dirname(relativeToFile), relpath);

let absoluteToRef relativeToFile abspath =
    let relpathWithExtn = absoluteToRelative(relativeToFile, abspath);
    return pipe(stripExtension, stripIndex, addHerePath)(relpathWithExtn);

let absoluteToRelative relativeToFile abspath = 
    if not abspath then None
    else Some path.relative(path.dirname(relativeToFile), abspath);

let takeUntilLast untilChar = WB take (lastIndexOf untilChar)

let stripExtension = takeUntilLast(".");
let stripIndex = takeUntilLast("/index");

let addHerePath = ifElse (fun x -> x.StartsWith(".")) I (fun x -> x.Concat("./"))

let isNpmPath allNpms refpath =
    if not refpath then false
    else 
        // handle sub-nav into npm modules i.e. `import ... from 'redux-saga/effects';`
        let firstPathPart = head(split("/", refpath))

        if (firstPathPart.startsWith(".") && !contains("node_modules", refpath)) then
            false

        any(eqBy(firstPathPart))(allNpms)

let doesFileExistWithExtnLookupRaw excludedExtns filename =
    _getPotentialFileNames(filename, excludedExtns)
    |> Seq.filter doesFileExist
    |> (Seq.isEmpty >> not)

let doesFileExistWithExtnLookup ...args =
    findFilesWithMatchingNames.apply args |> (Seq.isEmpty not)

let findFilesWithMatchingNames allFiles excludedExtns filename =
    _getPotentialFileNames filename excludedExtns
    |> Seq.map (_getExisting allFiles)

// LINQ?
let _getExisting allFiles potentials =
    innerJoin(flip(endsWith), List.toArray(allFiles), potentials);

let findFilesWithMatchingNamesi allFiles excludedExtns filename =
    _getPotentialFileNames filename  excludedExtns
    |> Seq.map (_getExistingi allFiles)

let endsWithi (str: string) (ending: string) =
    str.ToLower().EndsWith(ending.ToLower())

let _getExistingi allFiles potentials =
    innerJoin(endsWithi, List.toArray(allFiles), potentials);

let _getPotentialFileNames filename extns =
    _rawExtensions extns
    |> Seq.append (_indexExtensions extns)
    |> Seq.map (concat (defaultTo "" filename))

// import and require allow dropping file extensions
let _rawExtensions extns = 
    extns |> Array.append [|""|]

// import and require also allow a folder name - containing an index file
let _indexExtensions extns = _rawExtensions extns |> Seq.map (concat "/index")

let getRefsFromFile filename = getRefsFromFileContent (readFile filename)

let replaceContent resolveObj =
    let rawOrigContent = readFile resolveObj.filename
    Maybe.fromString(rawOrigContent)
        .map(_replaceAll(resolveObj.oldPath, resolveObj.resultRef))
        .fold(I, writeFile(resolveObj.filename));

let stringToRegex searchStr = getRegex (_escapeRegExp searchStr "g")

///<summary>
/// borrowed from https://stackoverflow.com/a/17606289/567493
///</summary>
let _escapeRegExp = replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
