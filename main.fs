module main

open System

open config
open utils
open io

let run = 
    let config = getConfig(getProcArgs());
    let allFiles = getProjectFiles(config);
    let allNpms = getAllNpms();

    let myRefExists = refExists(allFiles, config.missingExtensions);
    let myIsNpmPath = isNpmPath(allNpms);

    allFiles
    //|> log "Looking for broken references")
    |> Seq.map (getBrokenRefs myRefExists myIsNpmPath)
    // remove files that don't have any broken refs
    |> Seq.filter (Seq.isEmpty >> not)
    |> Seq.concat
    //|> log "Looking for potential solutions")
    |> Seq.map (applyAndMerge (findPotentials allFiles config))
    //|> log "Looking for the best solution")
    |> Seq.map (applyAndMerge (resolveRef allFiles resolve))
    |> Seq.map (applyAndMerge (resultToRef config))
    |> Seq.map (applyOrDisplay config)

let applyAndMerge f x = merge x (f x)

let getBrokenRefs doesExist isNpm filename =
    (getRefsFromFile filename)
        |> Seq.filter (complement isNpm)
        |> Seq.map (buildResolveObj filename)
        |> Seq.filter (complement doesExist)

let buildResolveObj filename oldPath =
    Map.empty.
        Add("filename", filename). 
        Add("oldPath", oldPath).
        Add("fullOldPath", (relativeToAbsolute filename oldPath))

let refExists allFiles excludedExtensions refObj =
    doesFileExistWithExtnLookup
        allFiles,
        excludedExtensions,
        (prop "fullOldPath" refObj)

let findPotentials allFiles config fileAndRef = 
    Map.empty.Add(
        "potentials", findFilesWithMatchingNamesi
            allFiles
            config.missingExtensions
            (path.basename fileAndRef.oldPath))

let resolveRef allFiles resolver fileAndRef =
    merge fileAndRef (resolver fileAndRef)

let resultToRef config resolveObj = Map.empty.Add(
    "resultRef", absoluteToRef
        (prop "filename" resolveObj)
        (getResult config resolveObj))

let getResult config resolveObj =
    prop config.resolveAlgo resolveObj

let displayChange x = 
    printfn "[%s]: \n\t%s\n\t -> %s" x.filename x.oldPath x.resultRef

let displayError x =
    printfn "[%s]: \n\t%s\n\t ~ %s" x.filename x.oldPath x.message

let display = ifElse (has "message") displayError displayChange

let applyChange x = 
    replaceContent x
    display x

let applyOrDisplay = ifElse (prop "dryRun") (K display) (K applyChange)