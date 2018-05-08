module ConfigSpec

open Xunit
open Config

[<Fact>]
let ``Will return default config when no file is provided``() = 
    Assert.Equal(defaultConfigObj, getConfig [||])
