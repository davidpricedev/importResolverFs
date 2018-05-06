module Utils

let inline flip f y x = f x y
let inline I x = x
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