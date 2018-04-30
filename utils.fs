module utils

let inline T x f = f x
let inline I x = x
let inline K x y = x

let inline ifElse pred thenCase elseCase data = 
    if pred data then thenCase data else elseCase data

