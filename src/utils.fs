module Utils

open System

let inline flip f y x = f x y
let inline K x y = x

let inline ifElse pred thenCase elseCase data = 
    if pred data then thenCase data else elseCase data

let inline both f g x = (f x) && (g x)

// borrowed from Ramda
let containsWith pred x list =
    list |> Seq.exists (fun y -> pred x y)

// borrowed from Ramda
let innerJoin (pred) (xs) (ys) = 
    xs |> Seq.filter (fun x -> (containsWith pred x ys))

let endsWith (e: string) (str: string) = str.EndsWith(e)
let startsWith (s: string) (str: string) = str.StartsWith(s)

let optToStr (strOpt: string option) = 
    match strOpt with
    | Some x -> x
    | None -> ""

/// this is one algorithm where a recursive algorithm is probably easier
let partitionArray (chunkSize: int) (array: array<'a>) =
    let chunks = int (Math.Floor (decimal array.Length / decimal chunkSize))

    let chunkIt chunkNum =
        let chunkStart = chunkNum * chunkSize
        let chunkEndCalc = chunkStart + chunkSize - 1
        let chunkEnd = Math.Min(chunkEndCalc, array.Length)
        array.[chunkStart..chunkEnd]

    [| 0..(chunks - 1) |]
    |> Array.map chunkIt