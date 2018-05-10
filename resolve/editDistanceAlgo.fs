module EditDistanceAlgo

open System
open Table2d

let tableToStringOffset x = x - 1

let getCharInStr (str: string) (n: int) = str.Chars(tableToStringOffset n)

let getDelCost node = 1 + (prevValueInRow node)

let getInsCost node = 1 + (prevValueInCol node)

let currentReplaceCost (a, b) = if a = b then 0 else 1

let calculateReplaceCost last twoChars =
    last + (currentReplaceCost twoChars)

let getReplaceCost str1 str2 node =
    let (row, col, _, _) = node
    calculateReplaceCost (prevValueDiag node) (getCharInStr str1 row, getCharInStr str2 col)

let getLastValue (_, data) =
    Array.get data ((Array.length data) - 1)
          
///<summary>
/// Implements a "dynamic programming" solution to calculate edit distance
/// adapted from https://nlp.stanford.edu/IR-book/html/htmledition/edit-distance-1.html
///</summary>
let calculateDistance (strh: string) (strv: string) = 
    let matrix = createTable (strh.Length + 1) (strv.Length + 1) 0

    let costOptions node = [
        getReplaceCost strh strv node
        getDelCost node
        getInsCost node ]

    let mapper node =
        let (row, col, _, _) = node
        if isInFirstCol node then row
        elif isInFirstRow node then col
        else
            costOptions node
            //|> Spy.inspect (sprintf "picking best for (%d,%d) from" row col)
            |> List.fold (fun x y -> if x < y then x else y) Int32.MaxValue

    matrixMap matrix mapper |> getLastValue

