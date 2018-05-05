module ResolveUtil

open System
open result

let init = ("", Int32.MaxValue)

let minReducer (a: string*int) (n: string*int) =
    let (_, adist) = a
    let (_, ndist) = n
    if ndist < adist then n else a

let getBest (calc: string->(string*int)) reducer (fileAndRef: Result) =
    let proc =
        (Seq.map calc) >>
        (Seq.fold reducer init) >>
        (fun (ref, _) -> ref)
    fileAndRef.potentials |> Option.map proc
