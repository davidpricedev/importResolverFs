module Program

open Main

let doEditDistance a b =
    let result = EditDistanceAlgo.calculateDistance a b
    printfn "distance between %s and %s is %d" a b result


[<EntryPoint>]
let program argv =
    printfn "Starting: %A" argv
    //run argv |> ignore
    doEditDistance "abc" "bbc" |> ignore
    0 // return an integer exit code
