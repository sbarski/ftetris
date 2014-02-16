namespace ftetris.types

[<AutoOpen>]
module Game =
    open System
    open System.Drawing

    type space = Empty = 0 | Wall = 1 | Occupied = 2 | Blocked = 3 | Filled = 4
    type cell = {space:space; color:Color}
    type playfield = {field: cell[,]; list:int; width:int; height:int}