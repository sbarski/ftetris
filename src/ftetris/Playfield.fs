module Playfield

open System
open System.Drawing

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL

open Tetronimo

type space = Empty = 0 | Wall = 1 | Occupied = 2 | Filled = 3
type playfield = {field: space[,]; list:int; width:int; height:int}

let private buildList (displayListHandle:int) (width:int) (height:int) (x:int) (y:int) = 
    GL.NewList(displayListHandle, ListMode.Compile)
    GL.Color3(Color.Green);
        
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

let private drawPolygon (x:int) y =
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
    | (x,y) when playfield.field.[x,y] = space.Occupied && y < 2 -> space.Filled
    | (x,y) when playfield.field.[x,y] = space.Occupied -> space.Occupied
    | _ -> space.Empty

let rec private updateBoard playfield y slices =
    match y with
    | [] -> playfield, slices
    | head :: tail -> 
            let slice = playfield.field.[0..playfield.width-1, head] |> Seq.cast<space> |> Seq.toArray

            if slice |> Array.forall (fun x -> x = space.Occupied) then 
                let top = playfield.field.[0..playfield.width-1, 0..head-1] //cut the top half (without the completed row)
                let bottom = playfield.field.[0..playfield.width-1, head+1..playfield.height-1] //cut the second half (without the completed row)
                let f = Array2D.zeroCreate<space> playfield.width playfield.height //create new array
            
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
    playfield.field |> Array2D.iteri (fun x y e -> if e = space.Occupied then drawPolygon x y)
    GL.CallList(playfield.list)

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
    tetronimo.shape |> Array2D.iteri (fun x y e -> if e = 1 then playfield.field.[y + tetronimo.x, x + tetronimo.y] <- space.Occupied)

let init displayListHandle width height x y =
    let buildList = buildList displayListHandle width height x y
    {field = Array2D.zeroCreate<space> width height; list = displayListHandle; width = width; height = height}

let restart playfield =
    let width = playfield.width
    let height = playfield.height
    {field = Array2D.zeroCreate<space> width height; list = playfield.list; width = width; height = height}