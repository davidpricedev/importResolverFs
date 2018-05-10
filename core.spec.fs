module CoreSpec

open Xunit
open Core
open result
open Config

let resultList = [|
    { (buildResolveObj "filename" "../brokenRef" "/full/brokenRef") with
        resultRef = Some "../../resolvedRef" }
|]

[<Fact>]
let ``applyOrDisplay:: Will display the result when dry-run is specified``() =
    let config = { defaultConfigObj with dryRun = true }
    let expected = "[filename]: \n\t../brokenRef\n\t -> ../../resolvedRef"
    Assert.Equal(expected, applyOrDisplay config resultList.[0] )
