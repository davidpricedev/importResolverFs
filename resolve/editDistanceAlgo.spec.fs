module EditDistanceAlgoSpec

open Xunit
open EditDistanceAlgo

[<Fact>]
let ``Will create an initialized 2d table``() =
    let t = Table(10, 10, 37)
    Assert.Equal((t.Get 9 9), 37)

[<Fact>]
let ``Will map over the entire table``() =
    let t = Table(10, 10, 10)
    mmap t (fun n -> n.Get() * 2) |> ignore
    Assert.Equal((t.Get 0 0), 20)

