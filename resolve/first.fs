module First

open System
open result

let applyFirst (fileAndRef: Result) =
    { fileAndRef with
        first = fileAndRef.potentials |> Option.map Seq.head }
        