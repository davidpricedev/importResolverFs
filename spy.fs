///<summary>
/// Several utilities to transparently do logging side effects
///</summary>
module Spy

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