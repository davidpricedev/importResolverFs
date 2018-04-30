module Program

open main

[<EntryPoint>]
let program argv =
    printfn "Hello World from F#!"
    run argv
    0 // return an integer exit code
