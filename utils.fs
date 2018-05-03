module utils

let inline T x f = f x
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