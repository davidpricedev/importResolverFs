module ParserSpec

open Xunit
open Parser

[<Theory>]
[<InlineData("import { funC-123_B }  from \"./A/B/theRefPath\"")>]
[<InlineData("import lib from './A/B/theRefPath'")>]
[<InlineData("""
            import { 
                A,
                B,
            } from './A/B/theRefPath'
""")>]
let ``getImportMatches:: Will find the import statement``(importStr) =
    let result = getImportMatches importStr
    Assert.Equal(["./A/B/theRefPath"], result)

[<Fact>]
let ``getImportMatches:: Will find all the imports in a big block"``() =
    let imports = """
        import x from 'a'
        import x as y from 'b'
        import { x } from 'a'
        import {
        x, y 
        } from 'c'
        import lib from "./A/B/1/theRefPath"
        import { funC-123_B }  from "./A/B/2/theRefPath"
        import { 
                        A,
                        B,
                    } from `./A/B/3/theRefPath`
        """
    let expected = List.sort [
                                "a"
                                "b"
                                "a"
                                "c"
                                "./A/B/1/theRefPath"
                                "./A/B/2/theRefPath"
                                "./A/B/3/theRefPath" ]
    Assert.Equal(expected, getImportMatches imports |> Seq.sort)

[<Theory>]
[<InlineData("const { x } = require('./x/y/therefpath')")>]
[<InlineData("const b = require(\"./x/y/therefpath\")")>]
[<InlineData("const c = require('./x/y/therefpath').property")>]
[<InlineData(@"
        const x = require(
            './x/y/therefpath'
        )")>]
let ``getRequireMatches:: Will find the require statement``(str) =
    let result = getRequireMatches str
    Assert.Equal(["./x/y/therefpath"], result)

[<Fact>]
let ``getRequireMatches:: Will find all the requires in a big block"``() =
    let requires = """
        const x = require('a')
        const { x } = require('b')
        const {
        x, y 
        } = require('c')
        const lib = require("./A/B/1/theRefPath")
        const funC-123_B = require(
            "./A/B/2/theRefPath"
        )
        const { 
                        A,
                        B,
                    } = require(`./A/B/3/theRefPath`)
        """
    let expected = List.sort [
                                "a"
                                "b"
                                "c"
                                "./A/B/1/theRefPath"
                                "./A/B/2/theRefPath"
                                "./A/B/3/theRefPath" ]
    let result = getRequireMatches requires |> Seq.sort
    Assert.Equal(expected, result)

[<Fact>]
let ``getRefsFromFileContent:: Will find the imports and requires from a mixed block"``() =
    let mixedBlock = """
        import x from 'a'
        import x as y from 'b'
        import { x } from 'a';
        const x = require('d');
        import {
        x, y 
        } from 'c';
        import lib from "./A/B/theRefPath"
        import { funC-123_B }  from "./A/B/theRefPath"
        import { 
                        A,
                        B,
                    } from \`./A/B/theRefPath\`;
        const { x } = require('b');
        const {
        x, y 
        } = require('c');
        const lib = require("./A/B/theRefPath")
        const funC-123_B = require(
            "./A/B/theRefPath"
        );
        const { 
                        A,
                        B,
                    } = require(\`./A/B/theRefPath\`) 
        """
    let expected = List.sort ["a"; "b"; "c"; "./A/B/theRefPath"; "d"]
    Assert.Equal(expected, getRefsFromFileContent mixedBlock |> Seq.sort)
