module EditDistanceAlgoSpec

open Xunit
open Table2d
open EditDistanceAlgo

[<Fact>]
let ``Will create an initialized 2d table``() =
    let t = createTable 10 10 37
    Assert.Equal(37, (valueAtRowCol 9 9 t))

[<Fact>]
let ``Will map over the entire table``() =
    let t = createTable 10 10 10
    let t2 = matrixMap t (fun n -> (valueFromEntry n) * 2)
    Assert.Equal(20, (valueAtRowCol 0 0 t2))

