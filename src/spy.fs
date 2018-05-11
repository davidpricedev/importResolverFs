///<summary>
/// Several utilities to transparently do logging side effects
///</summary>
module Spy
open System.Diagnostics

let inspectP pred prefix x =
    if pred x then 
        printfn "inspect item %s: %A" prefix x 
        x
    else 
        x

let inspect prefix = inspectP (fun x -> true) prefix

let aside msg x = 
    printfn "%s" msg
    x

let profile prefix fn =
    let sw = new Stopwatch()
    sw.Start()
    let result = fn()
    sw.Stop()
    printfn "Time taken for %s: %d" prefix sw.ElapsedMilliseconds
    result

let profileT fn =
    let sw = new Stopwatch()
    sw.Start()
    let result = fn()
    sw.Stop()
    (sw.ElapsedMilliseconds, result)
