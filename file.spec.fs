module FileSpec

open Xunit
open File
open Config

let startList = ["node_modules", ".git"]
let endList = [".jsx", ".js"]
let config = { defaultConfig with fileTypes = endList exclude = startList }

[<Fact>]
let ``fileFilter :: Will return true when a path matches end and doesn't match start``() =
    let file = "/home/user/src/A/B/main.js"
    Assert.Equal(true, fileFilter config file)

[<Fact>]
let ``fileFilter:: Will return false when a path matches end and start``() =
    let file = "/home/user/node_modules/A/B/main.js"
    Assert.Equal(false, fileFilter config file)

[<Fact>]
let ``fileFilter:: Will return false when a path doesn't match end or start``() =
    let file = "/home/user/src/A/B/main.md"
    Assert.Equal(false, fileFilter config file)

[<Fact>]
let ``fileFilter:: Will return false when a path doesn't match end and matches start``() =
    let file = "/home/user/node_modules/A/B/main.txt"
    Assert.Equal(false, fileFilter config file)

let list = ["node_modules", ".git"]

[<Fact>]
let ``isStartInList:: Will be true when the path starts with an excluded folder``() =
    Assert.Equal(true, isStartInList list "node_modules/shelljs")

[<Fact>]
let ``isStartInList:: Will be false when the path does NOT start with an excluded folder``() =
    Assert.Equal(false, isStartInList list "./ios")

let list = [".jsx", ".js"]

[<Fact>]
let ``isEndInList:: Will be true when the file type is in the list``() =
    Assert.Equal(true, isStartInList list "A/B/file.js")

[<Fact>]
let ``isEndInList:: Will be false when the file type is NOT in the list``() =
    Assert.Equal(false, isStartInList list "A/B/file.txt")

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
    Assert.Equal("../../Y/Z/ref", absoluteToRef "A/B/C/file.js" "A/Y/Z/ref.js")

[<Fact>]
let ``absoluteToRef:: Will convert index path to an import/require reference``() =
    Assert.Equal("../../Y/Z", absoluteToRef "A/B/C/file.js" "A/Y/Z/index.js")

[<Fact>]
let ``absoluteToRef:: Will convert index path to an import/require reference``() =
    let relativeTo = "/Users/me/dev/t/proj/features/home/Home.js"
    let abspath = "/Users/me/dev/t/proj/common/components/AppContent.js"
    Assert.Equal("../../common/components/AppContent", absoluteToRef relativeTo abspath)

[<Fact>]
let ``absoluteToRef:: Will have a ./ for files in the current folder``() =
    let file = "A/B/myCodeFile.js"
    let ref = "A/B/myUtilsFile.js"
    Assert.Equal("./myUtilsFile", absoluteToRef file ref)

[<Fact>]
let ``absoluteToRelative:: Will convert absolute path to relative``() =
    Assert.Equal("../../Y/Z/ref.js", absoluteToRelative "A/B/C/file.js" "A/Y/Z/ref.js")

[<Fact>]
let ``absoluteToRelative:: Will work for the case it failed``() =
    let file = "/Users/user/dev/proj/common/testHelpers/reducerTestHelpers.js"
    let ref = "/Users/user/dev/proj/utils/baseUtil.js"
    Assert.Equal("../../utils/baseUtil.js", absoluteToRelative file ref)

[<Fact>]
let ``takeUntilLast:: Will take the substring until the last occurrence``() =
    Assert.Equal("A/B/file", takeUntilLast "." "A/B/file.js")

[<Fact>]
let ``takeUntilLast:: Will skip early occurrences``() =
    Assert.Equal("A/index/B", takeUntilLast "/index" "A/index/B/index.js")

let npms = ["redux-saga", "lodash", "angularjs", "fs"]

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
    let result = isNpmPath(npms)("./fs")
    Assert.Equal(result).toBe(false)

[<Fact>]
let ``isNpmPath:: Will identify local paths if they are paths to node_modules``() =
    let result = isNpmPath(npms)("../../node_modules/fs")
    Assert.Equal(result).toBe(true)

[<Fact>]
let ``getPotentialFileNames:: Will map the prefix onto the extensions``() =
    let result = _getPotentialFileNames("prefix", [".js", ".jsx"])
    Assert.Equal(result).toEqual([
        "prefix"
        "prefix.js"
        "prefix.jsx"
        "prefix/index"
        "prefix/index.js"
        "prefix/index.jsx" ]

[<Fact>]
let ``getPotentialFileNames:: Will handle empty file names``() =
    let result = _getPotentialFileNames("", [".js", ".jsx"])
    Assert.Equal(result).toEqual([
        ""
        ".js"
        ".jsx"
        "/index"
        "/index.js"
        "/index.jsx" ]

[<Fact>]
let ``getPotentialFileNames:: Will merge text``() =
    let { join } = require("ramda")
    let mergeText = (a, b) => join(" ", [a, b])
    Assert.Equal(mergeText("a", null)).toBe("a ")
    Assert.Equal(mergeText("a")).toBe("a ")

[<Fact>]
let ``getPotentialFileNames:: Will handle null file names``() =
    let result = _getPotentialFileNames(null, [".js", ".jsx"])
    Assert.Equal(result).toEqual([
        ""
        ".js"
        ".jsx"
        "/index"
        "/index.js"
        "/index.jsx" ]

[<Fact>]
let ``getPotentialFileNames:: Will handle empty extensions``() =
    let result = _getPotentialFileNames("prefix", [])
    Assert.Equal(result).toEqual(["prefix", "prefix/index"])

[<Fact>]
let ``getPotentialFileNames:: Will handle null extensions``() =
    let result = _getPotentialFileNames("prefix", null)
    Assert.Equal(result).toEqual(["prefix", "prefix/index"])

let extns = [".js", ".jsx"]
let allFiles = ["X/Y/realFile.jsx"]

[<Fact>]
let ``findFilesWithMatchingNames:: Will return true if the file can be found at the given path``() =
    let result = findFilesWithMatchingNames(
        allFiles,
        extns,
        "realFile"
    ).toArray()
    Assert.Equal(result).toEqual(["X/Y/realFile.jsx"])

[<Fact>]
let ``findFilesWithMatchingNames:: Will return empty set if the file can NOT be found at the given path``() =
    let result = findFilesWithMatchingNames(
        allFiles,
        extns,
        "missingFile"
    ).toArray()
    Assert.Equal(result).toEqual([])

[<Fact>]
let ``endsWithi:: Will return true if ending matches``() =
    Assert.Equal(endsWithi("fox", "x")).toBe(true)

[<Fact>]
let ``endsWithi:: Will return false when ending doesn't match``() =
    Assert.Equal(endsWithi("fox", "m")).toBe(false)
    Assert.Equal(endsWithi("fox", "M")).toBe(false)
    Assert.Equal(endsWithi("FOX", "m")).toBe(false)

[<Fact>]
let ``endsWithi:: Will return true if ending matches case insensitively``() =
    Assert.Equal(endsWithi("fox", "X")).toBe(true)
    Assert.Equal(endsWithi("FOX", "x")).toBe(true)

let extns = [".js", ".jsx"]
let allFiles = ["X/Y/realFile.jsx"]

[<Fact>]
let ``doesFileExistWithExtnLookup:: Will return true if the file can be found at the given path``() =
            let result = doesFileExistWithExtnLookup(
                allFiles,
                extns,
                "X/Y/realFile"
            )
            Assert.Equal(result).toBe(true)

[<Fact>]
let ``doesFileExistWithExtnLookup:: Will return false if the file can NOT be found at the given path``() =
            let result = doesFileExistWithExtnLookup(
                allFiles,
                extns,
                "X/Y/missingFile"
            )
            Assert.Equal(result).toBe(false)

[<Fact>]
let ``getExisting:: Will return any potentials existing in allfiles``() =
            let potentials = [
                "X/Y/realFile",
                "X/Y/realFile.js",
                "X/Y/realFile.jsx",
            ]
            Assert.Equal(_getExisting(allFiles)(potentials)).toEqual([
                "X/Y/realFile.jsx",
            ])
        })

[<Fact>]
let ``getExisting:: Will be empty when potentials don't exist``() =
            let potentials = [
                "X/Y/fakeFile",
                "X/Y/fakeFile.js",
                "X/Y/fakeFile.jsx",
            ]
            Assert.Equal(_getExisting(allFiles)(potentials)).toEqual([])
        })

[<Fact>]
let ``getExisting:: Will match basename not just full path``() =
            let potentials = ["realFile", "realFile.js", "realFile.jsx"]
            Assert.Equal(_getExisting(allFiles)(potentials)).toEqual([
                "X/Y/realFile.jsx",

[<Fact>]
let ``replaceContent:: Will read content, replace and write the new content``() =
            Assert.Equal(io.readFile("A")).toBe("file Content A")
            let resolveObj = {
                filename: "A",
                oldPath: "e",
                resultRef: "X",
            }
            replaceContent(resolveObj, "closestSolution")
            Assert.Equal(io._writeFile).toHaveBeenCalledWith("A", "filX ContXnt A")

[<Fact>]
let ``replaceAll:: Will replace all occurrences``() =
            let orig = "the quick brown fox jumps over the lazy dog"
            let Assert.Equaled = "NEW quick brown fox jumps over NEW lazy dog"
            Assert.Equal(_replaceAll("the", "NEW")(orig)).toBe(Assert.Equaled)
