module Playfield

open System
open System.Drawing

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL

open Tetronimo

type space = Empty = 0 | Wall = 1 | Occupied = 2 | Blocked = 3 | Filled = 4
type cell = {space:space; color:Color}
type playfield = {field: cell[,]; list:int; width:int; height:int}

let private createEmptyField width height =
    Array2D.init<cell> width height (fun i j -> {space = space.Empty; color = Color.Black})

let private buildList (displayListHandle:int) (width:int) (height:int) (x:int) (y:int) = 
    GL.NewList(displayListHandle, ListMode.Compile)
    GL.Color3(Color.Black);
        
    let x = Convert.ToDouble(x)
    let y = Convert.ToDouble(y)

    for i = 0 to height do
        GL.Begin(PrimitiveType.Lines)
        GL.Vertex2(x, Convert.ToDouble(i) + y)
        GL.Vertex2(Convert.ToDouble(width) + x, Convert.ToDouble(i) + y)
        GL.End()
        
    for i = 0 to width do
        GL.Begin(PrimitiveType.Lines)
        GL.Vertex2(Convert.ToDouble(i) + x, y)
        GL.Vertex2(Convert.ToDouble(i) + x, Convert.ToDouble(height) + y)
        GL.End()

    GL.EndList()

let private drawPolygon (x:int) y (color:Color) =
    GL.Color3(color)

    GL.Begin(PrimitiveType.Quads)
    GL.Vertex2(x, y)
    GL.Vertex2(x, y + 1)
    GL.Vertex2(x + 1, y + 1)
    GL.Vertex2(x + 1, y)
    GL.End()

let private isEmptyCell playfield x y = 
    match (x, y) with
    | (x,y) when x < 0 -> space.Wall
    | (x,y) when x > playfield.width - 1 -> space.Wall
    | (x,y) when y > playfield.height - 1 -> space.Occupied
    | (x,y) when (playfield.field.[x,y].space = space.Occupied || playfield.field.[x,y].space = space.Blocked) && y < 2 -> space.Filled
    | (x,y) when (playfield.field.[x,y].space = space.Occupied || playfield.field.[x,y].space = space.Blocked) -> space.Occupied
    | _ -> space.Empty

let rec private updateBoard playfield y slices =
    match y with
    | [] -> playfield, slices
    | head :: tail -> 
            let slice = playfield.field.[0..playfield.width-1, head..head] |> Seq.cast<cell> |> Seq.toArray

            if slice |> Array.forall (fun x -> x.space = space.Occupied) then 
                let top = playfield.field.[0..playfield.width-1, 0..head-1] //cut the top half (without the completed row)
                let bottom = playfield.field.[0..playfield.width-1, head+1..playfield.height-1] //cut the second half (without the completed row)
                let f = createEmptyField playfield.width playfield.height //create new array
            
                if top.Length > 0 then
                    Array2D.blit top 0 0 f 0 1 (Array2D.length1 top) (Array2D.length2 top) //merge the top half one row down

                if bottom.Length > 0 then
                    Array2D.blit bottom 0 0 f 0 (head+1) (Array2D.length1 bottom) (Array2D.length2 bottom) //merge the bottom half in place

                let score = slices + 1
                let field = { playfield with field = f }
                updateBoard field tail score
            else
                let score = slices
                updateBoard playfield tail score



let update playfield y =
    updateBoard playfield y 0

let draw playfield = 
    GL.Color3(Color.AntiqueWhite)
    playfield.field |> Array2D.iteri (fun x y e -> if e.space = space.Occupied || e.space = space.Blocked then drawPolygon x y e.color)
    GL.CallList(playfield.list)
    
let addRows playfield rows =
    //cut the top slice
    let f = createEmptyField playfield.width playfield.height
    let top = playfield.field.[0..playfield.width-1, rows..playfield.height-1]
    let bottom = Array2D.init<cell> playfield.width rows (fun i j -> {space = space.Blocked; color = Color.AntiqueWhite})

    Array2D.blit top 0 0 f 0 0 (Array2D.length1 top) (Array2D.length2 top)
    Array2D.blit bottom 0 0 f 0 (Array2D.length2 f - rows) (Array2D.length1 bottom) (Array2D.length2 bottom)

    {field = f; list = playfield.list; width = playfield.width; height = playfield.height}
     
let getNextPosition playfield tetronimo  =
    let currentField = playfield.field |> Array2D.copy
    let mutable position = space.Empty

    for i = 0 to (Array2D.length1 tetronimo.shape - 1) do
        for j = 0 to (Array2D.length2 tetronimo.shape - 1) do
            if tetronimo.shape.[i,j] = 1 then
                let move = isEmptyCell playfield (j + tetronimo.x) (i + tetronimo.y)

                if not (move = space.Empty) && (position = space.Empty) then 
                    position <- move

    (position, tetronimo)                           

let savePosition tetronimo playfield =
    tetronimo.shape |> Array2D.iteri (fun x y e -> if e = 1 then playfield.field.[y + tetronimo.x, x + tetronimo.y] <- {space = space.Occupied; color = tetronimo.colour})

let init displayListHandle width height x y =
    let buildList = buildList displayListHandle width height x y
    {field = createEmptyField width height; list = displayListHandle; width = width; height = height}

let restart playfield =
    let width = playfield.width
    let height = playfield.height
    {field = createEmptyField width height; list = playfield.list; width = width; height = height}