module Table2dSpec

open Xunit
open Table2d

[<Fact>]
let ``Will create an initialized 2d table``() =
    let t = createTable 10 10 37
    Assert.Equal(37, (valueAtRowCol 9 9 t))

[<Fact>]
let ``Will map over the entire table``() =
    let t = createTable 10 10 10
    let t2 = matrixMap t (fun n -> (valueFromEntry n) * 2)
    Assert.Equal(20, (valueAtRowCol 0 0 t2))

[<Fact>]
let ``Will translate x/y coords to a single-array index``() =
    let t = createTable 10 10 10
    Assert.Equal(75, whtoindex 5 7 t)

[<Fact>]
let ``Will translate an index to width and height coords``() =
    let t = createTable 10 10 10
    let (width, data) = t
    Assert.Equal(10 * 10, Array.length data)
    Assert.Equal(10, width)
    Assert.Equal((5, 7), indextowh 75 t)

[<Fact>]
let ``Will indicate first in column``() =
    let t = createTable 10 10 10
    let n = getEntry 0 0 t
    Assert.Equal(true, isInFirstCol n)
    Assert.Equal(true, isInFirstRow n)