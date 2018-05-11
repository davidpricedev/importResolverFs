module Random

open System
open result

let randomRange lower upper = 
    let r = new Random()
    r.Next(lower, upper)

let randomElem list = list |> Seq.item (randomRange 0 (list |> Seq.length))

let applyRandom fileAndRef =
    { fileAndRef with
        random =  fileAndRef.potentials |> Option.map randomElem }
