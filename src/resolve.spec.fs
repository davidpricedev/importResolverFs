module ResolveSpec

open Xunit
open Resolve
open File
open result
open Closest
open First
open Random
open EditDistance

let filename = "fakeFile"
let refpath = "fakeRefPath"
let inputBase = buildResolveObj filename refpath refpath

[<Fact>]
let ``applyFirst:: Will return the first option``() =
    let potentials = [| "fakeOptionA"; "fakeOptionB" |] 
    let input = {
        inputBase with potentials = Some (Seq.ofArray potentials) }
    let expected = { input with first = Some "fakeOptionA" }
    Assert.Equal(expected, applyFirst input)

[<Fact>]
let ``applyClosest:: Will find the closest option``() =
    let potentials = [|
        "../../../A/B/Z/X/Y/util.js"
        "../../../A/B/util.js" |]
    let input = {
        inputBase with potentials = Some (Seq.ofArray potentials) }
    let expected = { input with closest = Some potentials.[1] }
    Assert.Equal(expected, applyClosest input)

[<Fact>]
let ``applyEditDistance:: Will find the nearest path by edit distance``() =
    let potentials = [|
        "../../features/cosmicChameleon"
        "../../view/cosmicChameleon" |]
    let input = {
        inputBase with
            oldPath = "../../feature/cosmicChameleon"
            potentials = Some (Seq.ofArray potentials) }
    let expected = { input with editDistance = Some potentials.[0] }
    Assert.Equal(expected, applyEditDistance input)

[<Fact>]
let ``resolve:: Will resolve all algorithms``() =
    let potentials = [|
        "../../features/cosmicChameleon"
        "../../view/cosmicChameleon" |]
    let input = {
        inputBase with
            oldPath = "../../feature/cosmicChameleon"
            potentials = Some (Seq.ofArray potentials) }
    let result = resolve input
    Assert.Equal(Some potentials.[0], result.first)
    Assert.Equal(Some potentials.[0], result.closest)
    Assert.Equal(Some potentials.[0], result.editDistance)
