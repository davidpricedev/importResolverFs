module editDistance

open System
open utils

///<summary>
/// Implements a 2d table / matrix
///</summary>
type Table(width, height, initVal: int) = 
    member this.width: int = width
    member this.height: int = height
    member this.len = width * height
    member this.table = [| for i in 0 .. this.len -> initVal |]

    member this.whtoindex row col = row + (col * this.width)

    member this.set row col value =
        Array.set this.table (this.whtoindex row col) value

    member this.get row col =
        Array.get this.table (this.whtoindex row col)

    member this.indextowh (index: int) =
        let fdiv = (float index) / (float this.width)
        let col = int (floor fdiv)
        let row = index - (col * this.width)
        ( row, col )

    member this.print () =
        printfn "Table:: %A" this.table
       
type Node(table: Table, row: int, col: int) = 
    member this.rowIndex = row
    member this.colIndex = col
    member this.get () = table.get row col
    member this.set () = table.set row col
    member this.prevInCol = Node(table, row, (col - 1))
    member this.prevInRow = Node(table, (row - 1),  col)
    member this.prevDiag = Node(table, (row - 1), (col - 1))
    member this.isFirstInCol () = col = 0
    member this.isFirstInRow () = row = 0
    
let nodeAt table row col = Node(table, row, col)

///<summary>
/// mutating, maps the given callback across the table.
/// the callback should expect a node as the argument
///</summary>
let mmap (table: Table) mapFn =
    [| for i in 0 .. table.len -> 
        let ( row, col ) = table.indextowh i
        Array.set table.table i (mapFn (nodeAt table row col))
    |]
    
let tableToStringOffset x = x - 1
let getCharInStr (str: string) (n: int) =
    str.Chars(tableToStringOffset n)
let getPrevInCol (node: Node) = node.prevInCol.get()
let getPrevInRow (node: Node) = node.prevInRow.get();
let getPrevDiag (node: Node) = node.prevDiag.get();
let getDelCost = getPrevInRow >> ((+) 1)
let getInsCost = getPrevInCol >> ((+) 1)
let currentReplaceCost = ifElse (fun (x, y) -> x = y) (K 0) (K 1)
let calculateReplaceCost last twoChars =
    last + (currentReplaceCost twoChars)
let getReplaceCost str1 str2 node =
    calculateReplaceCost
        (getPrevDiag node)
        ( node.rowIndex |> getCharInStr str1,
          node.colIndex |> getCharInStr str2 )
          
///<summary>
/// Implements a "dynamic programming" solution to calculate edit distance
/// adapted from https://nlp.stanford.edu/IR-book/html/htmledition/edit-distance-1.html
///</summary>
let calculateDistance (strh: string) (strv: string) = 
    let m = Table(strh.Length + 1, strv.Length + 1, 0)
    let myReplaceCost = getReplaceCost strh strv

    mmap m (fun (node: Node) -> 
        if node.isFirstInCol() then node.rowIndex;
        elif node.isFirstInRow() then node.colIndex;
        else [
                myReplaceCost node
                getDelCost node
                getInsCost node ]
            |> List.fold (fun x y -> if x <= y then x else y) Int32.MaxValue)
        |> ignore

    m.get (m.width - 1) (m.height - 1)

