module EditDistanceAlgo

open System
open Utils

///<summary>
/// Implements a 2d table / matrix
///</summary>
type Table(width, height, initVal: int) = 
    member this.Width: int = width
    member this.Height: int = height
    member this.Len = width * height
    member this.Table = [| for i in 0 .. this.Len -> initVal |]

    member this.Whtoindex row col = row + (col * this.Width)

    member this.Set row col value =
        Array.set this.Table (this.Whtoindex row col) value

    member this.Get row col =
        Array.get this.Table (this.Whtoindex row col)

    member this.Indextowh (index: int) =
        let fdiv = (float index) / (float this.Width)
        let col = int (floor fdiv)
        let row = index - (col * this.Width)
        ( row, col )

    member this.Print () =
        printfn "Table:: %A" this.Table
       
type Node(table: Table, row: int, col: int) = 
    member this.RowIndex = row
    member this.ColIndex = col
    member this.Get () = table.Get row col
    member this.Set () = table.Set row col
    member this.PrevInCol () = Node(table, row, (col - 1))
    member this.PrevInRow () = Node(table, (row - 1),  col)
    member this.PrevDiag () = Node(table, (row - 1), (col - 1))
    member this.IsFirstInCol = col = 0
    member this.IsFirstInRow = row = 0
    
let nodeAt table row col = Node(table, row, col)

///<summary>
/// Mutates!, maps the given callback across the table.
/// the callback should expect a Node as the argument
///</summary>
let mmap (table: Table) (mapFn: Node -> int) =
    [| for i in 0 .. table.Len -> 
        let ( row, col ) = table.Indextowh i
        let node = nodeAt table row col
        Array.set table.Table i (mapFn node)
    |]
    
let tableToStringOffset x = x - 1
let getCharInStr (str: string) (n: int) =
    str.Chars(tableToStringOffset n)

let getPrevInCol (node: Node) = node.PrevInCol().Get()
let getPrevInRow (node: Node) = node.PrevInRow().Get();
let getPrevDiag (node: Node) = node.PrevDiag().Get();

let getDelCost = getPrevInRow >> ((+) 1)
let getInsCost = getPrevInCol >> ((+) 1)

let currentReplaceCost = ifElse (fun (x, y) -> x = y) (K 0) (K 1)
let calculateReplaceCost last twoChars =
    last + (currentReplaceCost twoChars)
let getReplaceCost str1 str2 node =
    calculateReplaceCost
        (getPrevDiag node)
        ( node.RowIndex |> getCharInStr str1,
          node.ColIndex |> getCharInStr str2 )
          
///<summary>
/// Implements a "dynamic programming" solution to calculate edit distance
/// adapted from https://nlp.stanford.edu/IR-book/html/htmledition/edit-distance-1.html
///</summary>
let calculateDistance (strh: string) (strv: string) = 
    let matrix = Table(strh.Length + 1, strv.Length + 1, 0)
    let myReplaceCost = getReplaceCost strh strv

    let mapper (node: Node) =
        if node.IsFirstInCol then node.RowIndex;
        elif node.IsFirstInRow then node.ColIndex;
        else [
                myReplaceCost node
                getDelCost node
                getInsCost node ]
            |> List.fold (fun x y -> if x <= y then x else y) Int32.MaxValue

    mmap matrix mapper |> ignore
    matrix.Get (matrix.Width - 1) (matrix.Height - 1)

