module IO

open System
open Utils
open CliWrap
open System.IO
open FSharp.Core
open Path

(******************
 * All the IO side effects
 *****************)


let prefixCwd x = pathJoin (Directory.GetCurrentDirectory()) x

let stripCwd (x: string) = x.Replace(Directory.GetCurrentDirectory(), "")

let writeFile path content = File.WriteAllText(path, content) 

let doesFileExist path = File.Exists path

let readFile = ifElse doesFileExist (fun path -> File.ReadLines path) (K Seq.empty)

let readWholeFile = ifElse doesFileExist (fun path -> File.ReadAllText path) (K "")

let getAllFiles path = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)

let getNpmFolders = Directory.GetDirectories(prefixCwd "node_modules")

///<summary>
/// hard-code for now since we can't use the node runtime itself
/// could use: https://github.com/sindresorhus/builtin-modules
/// - if there was a way to consume json easily
///</summary>
let getNpmBuiltins = seq { yield "fs"; yield "os"; }

let getAllNpms = Seq.append getNpmFolders getNpmBuiltins