module PathSpec

open Xunit
open Path

[<Fact>]
let ``getRelativePath :: Will build a relative path from two absolute paths``() =
    let fromPath = "/home/user/src/A/B/main.js"
    let toPath = "/home/user/src/A/C/whatever.js"
    let result = getRelativePath fromPath toPath
    let expected = Some "../C/whatever.js"
    Assert.Equal(expected, result)

[<Fact>]
let ``getRelativePath :: Will build a relative path from two relative paths``() =
    let fromPath = "src/A/B/main.js"
    let toPath = "src/A/C/whatever.js"
    let result = getRelativePath fromPath toPath
    let expected = Some "../C/whatever.js"
    Assert.Equal(expected, result)