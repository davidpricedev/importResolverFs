module Program

open Main

[<EntryPoint>]
let program argv =
    printfn "Starting: %A" argv
    run argv |> ignore
    0 // return an integer exit code
