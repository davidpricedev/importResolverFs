module Table2d

open Utils

type Table<'a> = int * 'a array
type Node<'a> = int * int * 'a * Table<'a>

let createTable<'a> width height (initVal: 'a) =
    (width, Array.create (width * height) initVal)

let printTable table =
    let (width, data) = table
    printfn "Table:: "
    (partitionArray width data)
    |> printfn "%A" 
    table 

let indextowh<'a> (index: int) ((width, _): 'a Table) =
    let fdiv = (float index) / (float width)
    let col = int (floor fdiv)
    let row = index - (col * width)
    ( row, col )

let whtoindex row col ((width, _): Table<'a>) = row + (col * width)

let getTableLength ((_, data): Table<'a>) = Array.length data

let getEntryValue row col (table: Table<'a>) =
    let (_, data) = table
    Array.get data (whtoindex row col table)

let getEntry row col (table: Table<'a>) =
    let value = getEntryValue row col table
    (row, col, value, table)

let getEntryByIndex index (table: Table<'a>) =
    let (row, col) = indextowh index table
    getEntry row col table
       
let valueFromEntry ((_, _, value, _): Node<'a>) = value

let prevInRow ((row, col, _, table): Node<'a>) = getEntry (row - 1) col table

let prevInCol ((row, col, _, table): Node<'a>) = getEntry row (col - 1) table

let prevDiag ((row, col, _, table): Node<'a>) = getEntry (row - 1) (col - 1) table

let prevValueInRow node = (prevInRow >> valueFromEntry) node

let prevValueInCol node = (prevInCol >> valueFromEntry) node

let prevValueDiag node = (prevDiag >> valueFromEntry) node

let valueAtIndex index table = getEntryByIndex index table |> valueFromEntry

let valueAtRowCol row col table = getEntry row col table |> valueFromEntry

let isInFirstCol ((_, col, _, _): Node<'a>) = col = 0

let isInFirstRow ((row, _, _, _): Node<'a>) = row = 0

///<summary>
/// Mutates! maps each value to a new value in-place in the array
///</summary>
let inPlaceMap data fn =
    for i in 0 .. (Array.length data - 1) do
        Array.set data i (fn i)
    data

///<summary>
/// Maps the given callback across the table.
/// the callback should expect a Node (row, col, value, table) as the argument
/// Mutates the table's inner table
///</summary>
let matrixMap (table: Table<'a>) (mapFn: Node<'a>->'a) =
    let (width, data) = table
    let fn = (fun i -> mapFn (getEntryByIndex i table))
    inPlaceMap data fn |> ignore
    (width, data)
    
