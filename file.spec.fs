module FileSpec

open Xunit
open File
open Config
open result

let startList = List.sort ["node_modules"; ".git"]
let endList = List.sort [".jsx"; ".js"]
let config = { defaultConfigObj with ConfigType.fileTypes = endList; exclude = startList }
let listEnds = List.sort [".jsx"; ".js"]
let list = List.sort ["node_modules"; ".git"]
let npms = List.sort ["redux-saga"; "lodash"; "angularjs"; "fs"]
let extns = List.sort [".js"; ".jsx"]
let allFiles = ["X/Y/realFile.jsx"]

[<Fact>]
let ``projectFileFilter :: Will return true when a path matches end and doesn't match start``() =
    let file = "src/A/B/main.js"
    Assert.Equal(true, _projectFileFilter config file)

[<Fact>]
let ``projectFileFilter:: Will return false when a path matches end and start``() =
    let file = "node_modules/A/B/main.js"
    Assert.Equal(false, _projectFileFilter config file)

[<Fact>]
let ``projectFileFilter:: Will return false when a path doesn't match end or start``() =
    let file = "src/A/B/main.md"
    Assert.Equal(false, _projectFileFilter config file)

[<Fact>]
let ``projectFileFilter:: Will return false when a path doesn't match end and matches start``() =
    let file = "node_modules/A/B/main.txt"
    Assert.Equal(false, _projectFileFilter config file)

[<Fact>]
let ``isStartInList:: Will be true when the path starts with an excluded folder``() =
    Assert.Equal(true, _isStartInList list "node_modules/shelljs")

[<Fact>]
let ``isStartInList:: Will be false when the path does NOT start with an excluded folder``() =
    Assert.Equal(false, _isStartInList list "./ios")

[<Fact>]
let ``isEndInList:: Will be true when the file type is in the list``() =
    Assert.Equal(true, _isEndInList listEnds "A/B/file.js")

[<Fact>]
let ``isEndInList:: Will be false when the file type is NOT in the list``() =
    Assert.Equal(false, _isEndInList listEnds "A/B/file.txt")

/// assumes a unix variant OS for tests!
/// other OS might break this test due to node's path.sep being different
[<Fact>]
let ``relativeToAbsolute:: Will get the full path based on file path and relative reference``() =
    let result1 = relativeToAbsolute "/home/user/package.json" "./src/main.js"
    Assert.Equal("/home/user/src/main.js", result1)
    let result2 = relativeToAbsolute "/home/user/src/main.js" "../index.js"
    Assert.Equal("/home/user/index.js", result2)

[<Fact>]
let ``absoluteToRef:: Will convert path to an import/require reference``() =
    let result = absoluteToRef "A/B/C/file.js" "A/Y/Z/ref.js"
    Assert.Equal(Some "../../Y/Z/ref", result)

[<Fact>]
let ``absoluteToRef:: Will convert index path to an import/require reference``() =
    let result = absoluteToRef "A/B/C/file.js" "A/Y/Z/index.js"
    Assert.Equal(Some "../../Y/Z", result)

[<Fact>]
let ``absoluteToRef:: Will convert index path to an import/require reference (real)``() =
    let relativeTo = "/Users/me/dev/t/proj/features/home/Home.js"
    let abspath = "/Users/me/dev/t/proj/common/components/AppContent.js"
    let result = absoluteToRef relativeTo abspath
    Assert.Equal(Some "../../common/components/AppContent", result)

[<Fact>]
let ``absoluteToRef:: Will have a ./ for files in the current folder``() =
    let file = "A/B/myCodeFile.js"
    let ref = "A/B/myUtilsFile.js"
    Assert.Equal(Some "./myUtilsFile", absoluteToRef file ref)

[<Fact>]
let ``absoluteToRelative:: Will convert absolute path to relative``() =
    Assert.Equal(Some "../../Y/Z/ref.js", absoluteToRelative "A/B/C/file.js" "A/Y/Z/ref.js")

[<Fact>]
let ``absoluteToRelative:: Will work for the case it failed``() =
    let file = "/Users/user/dev/proj/common/testHelpers/reducerTestHelpers.js"
    let ref = "/Users/user/dev/proj/utils/baseUtil.js"
    Assert.Equal(Some "../../utils/baseUtil.js", absoluteToRelative file ref)

[<Fact>]
let ``isNpmPath:: Will correctly identify simple npm refs``() =
    let result = isNpmPath npms "lodash"
    Assert.Equal(true, result)

[<Fact>]
let ``isNpmPath:: Will correctly identify npm sub-paths``() =
    let result = isNpmPath npms "redux-saga/effects"
    Assert.Equal(true, result)

[<Fact>]
let ``isNpmPath:: Will not identify local paths``() =
    let result = isNpmPath npms "./fs"
    Assert.Equal(false, result)

[<Fact>]
let ``isNpmPath:: Will identify local paths if they are paths to node_modules``() =
    let result = isNpmPath npms "../../node_modules/fs"
    Assert.Equal(true, result)

[<Fact>]
let ``getPotentialFileNames:: Will map the prefix onto the extensions``() =
    let result = _getPotentialFileNames "prefix" [".js"; ".jsx"]
    let expected = List.sort [
                                "prefix"
                                "prefix.js"
                                "prefix.jsx"
                                "prefix/index"
                                "prefix/index.js"
                                "prefix/index.jsx" ]
    Assert.Equal(expected, Seq.sort result);

[<Fact>]
let ``getPotentialFileNames:: Will handle empty file names``() =
    let result = _getPotentialFileNames "" [".js"; ".jsx"]
    let expected = List.sort [
                                ""
                                ".js"
                                ".jsx"
                                "/index"
                                "/index.js"
                                "/index.jsx" ]
    Assert.Equal(expected, Seq.sort result)

[<Fact>]
let ``getPotentialFileNames:: Will handle null file names``() =
    let result = _getPotentialFileNames null [".js"; ".jsx"]
    let expected = List.sort [
                                ""
                                ".js"
                                ".jsx"
                                "/index"
                                "/index.js"
                                "/index.jsx" ]
    Assert.Equal(expected, Seq.sort result)

[<Fact>]
let ``getPotentialFileNames:: Will handle empty extensions``() =
    let result = _getPotentialFileNames "prefix" []
    let expected = List.sort ["prefix"; "prefix/index"]
    Assert.Equal(expected, Seq.sort result)

[<Fact>]
let ``findFilesWithMatchingNames:: Will return true if the file can be found at the given path``() =
    let result = findFilesWithMatchingNames allFiles extns "realFile"
    let expected = ["X/Y/realFile.jsx"]
    Assert.Equal(expected, result)

[<Fact>]
let ``findFilesWithMatchingNames:: Will return empty set if the file can NOT be found at the given path``() =
    let result = findFilesWithMatchingNames allFiles extns "missingFile"
    Assert.Equal([], result)

[<Fact>]
let ``endsWithi:: Will return true if ending matches``() =
    Assert.Equal(true, endsWithi "fox" "x")

[<Fact>]
let ``endsWithi:: Will return false when ending doesn't match``() =
    Assert.Equal(false, endsWithi "fox" "m")
    Assert.Equal(false, endsWithi "fox" "M")
    Assert.Equal(false, endsWithi "FOX" "m")

[<Fact>]
let ``endsWithi:: Will return true if ending matches case insensitively``() =
    Assert.Equal(true, endsWithi "fox" "X")
    Assert.Equal(true, endsWithi "FOX" "x")

[<Fact>]
let ``doesFileExistWithExtnLookup:: Will return true if the file can be found at the given path``() =
    let result = doesFileExistWithExtnLookup allFiles extns "X/Y/realFile"
    Assert.Equal(true, result)

[<Fact>]
let ``doesFileExistWithExtnLookup:: Will return false if the file can NOT be found at the given path``() =
    let result = doesFileExistWithExtnLookup allFiles extns "X/Y/missingFile"
    Assert.Equal(false, result)

[<Fact>]
let ``getExisting:: Will return any potentials existing in allfiles``() =
    let potentials = [
        "X/Y/realFile"
        "X/Y/realFile.js"
        "X/Y/realFile.jsx" ]
    let expected = ["X/Y/realFile.jsx"]
    Assert.Equal(expected, _getExisting allFiles potentials)

[<Fact>]
let ``getExisting:: Will be empty when potentials don't exist``() =
    let potentials = [
        "X/Y/fakeFile"
        "X/Y/fakeFile.js"
        "X/Y/fakeFile.jsx" ]
    Assert.Equal([], _getExisting allFiles potentials)

[<Fact>]
let ``getExisting:: Will match basename not just full path``() =
    let potentials = ["realFile"; "realFile.js"; "realFile.jsx"]
    let expected = ["X/Y/realFile.jsx"]
    Assert.Equal(expected, _getExisting allFiles potentials)

(* -- Mocked
[<Fact>]
let ``replaceContent:: Will read content, replace and write the new content``() =
    Assert.Equal("file Content A", io.readFile "A")
    let resolveObj = { (buildResolveObj "A" "e" "/full/e") with resultRef = "X" }
    let result = replaceContent resolveObj "closestSolution"
    Assert.Equal(io._writeFile).toHaveBeenCalledWith("A", "filX ContXnt A")
*)

[<Fact>]
let ``replaceAll:: Will replace all occurrences``() =
    let orig = "the quick brown fox jumps over the lazy dog"
    let expected = "NEW quick brown fox jumps over NEW lazy dog"
    Assert.Equal(expected, _replaceAll "the" "NEW" orig)
