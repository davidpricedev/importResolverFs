module EditDistanceAlgoSpec

open Xunit
open EditDistanceAlgo

[<Fact>]
let ``calculateReplaceCost:: Will calculate replacement cost``() =
    Assert.Equal(0, calculateReplaceCost 0 ("a", "a"))
    Assert.Equal(5, calculateReplaceCost 5 ("a", "a"))
    Assert.Equal(6, calculateReplaceCost 5 ("a", "b"))
    Assert.Equal(1, calculateReplaceCost 0 ("a", "b"))

[<Theory>]
[<InlineData("cat", "dog", 3)>]
[<InlineData("fat", "cat", 1)>]
[<InlineData("cat", "catalog", 4)>]
let ``calculateDistance:: Will return an Asserted edit distance``(str1, str2, expDist) =
    let result = calculateDistance str1 str2
    Assert.Equal(expDist, result)
