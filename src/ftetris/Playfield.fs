module Playfield

open System
open System.Drawing

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL

open Tetronimo

type space = Empty = 0 | Wall = 1 | Occupied = 2 | Filled = 3

type Playfield(list:int, ?width: int, ?height: int) =
    let width = defaultArg width 10
    let height = defaultArg height 20
    let mutable field = Array2D.zeroCreate<space> width height
    let displayListHandle = GL.GenLists(list)

    let buildList = 
        GL.NewList(displayListHandle, ListMode.Compile)
        GL.Color3(Color.Green);
        
        for i = 0 to height do
           GL.Begin(PrimitiveType.Lines)
           GL.Vertex2(0.0, Convert.ToDouble(i))
           GL.Vertex2(Convert.ToDouble(width), Convert.ToDouble(i))
           GL.End()
        
        for i = 0 to width do
            GL.Begin(PrimitiveType.Lines)
            GL.Vertex2(Convert.ToDouble(i), 0.0)
            GL.Vertex2(Convert.ToDouble(i), Convert.ToDouble(height))
            GL.End()

        GL.EndList()

    let drawPolygon (x:int) y =
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex2(x, y)
        GL.Vertex2(x, y + 1)
        GL.Vertex2(x + 1, y + 1)
        GL.Vertex2(x + 1, y)
        GL.End()

    let isEmptyCell x y = 
        match (x, y) with
        | (x,y) when x < 0 -> space.Wall
        | (x,y) when x > width - 1 -> space.Wall
        | (x,y) when y > height - 1 -> space.Occupied
        | (x,y) when field.[x,y] = space.Occupied && y = 1 -> space.Filled
        | (x,y) when field.[x,y] = space.Occupied -> space.Occupied
        | _ -> space.Empty

    let rec updateBoard y slices =
        match y with
        | [] -> slices
        | head :: tail -> 
            let slice = field.[0..width-1, head]

            if slice |> Array.forall (fun x -> x = space.Occupied) then 
                let top = field.[0..width-1, 0..head-1] //cut the top half (without the completed row)
                let bottom = field.[0..width-1, head+1..height-1] //cut the second half (without the completed row)
                let f = Array2D.zeroCreate<space> width height //create new array
            
                if top.Length > 0 then
                    Array2D.blit top 0 0 f 0 1 (Array2D.length1 top) (Array2D.length2 top) //merge the top half one row down

                if bottom.Length > 0 then
                    Array2D.blit bottom 0 0 f 0 (head+1) (Array2D.length1 bottom) (Array2D.length2 bottom) //merge the bottom half in place

                field <- f

                let score = slices + 1
                updateBoard tail score
            else
                let score = slices
                updateBoard tail score

    member x.Update y =
        let result = updateBoard y 0
        result

    member x.Draw = 
        field |> Array2D.iteri (fun x y e -> if e = space.Occupied then drawPolygon x y)
        GL.CallList(displayListHandle)

    member x.GetNextPosition (t:Tetronimo) =
        let currentField = field |> Array2D.copy
        let mutable position = space.Empty

        for i = 0 to (Array2D.length1 t.Shape - 1) do
            for j = 0 to (Array2D.length2 t.Shape - 1) do
                if t.Shape.[i,j] = 1 then
                    let move = isEmptyCell (j + t.X) (i + t.Y)

                    if not (move = space.Empty) && (position = space.Empty) then 
                        position <- move

        (position, t)                           

    member x.SavePosition (t:Tetronimo) =
        t.Shape |> Array2D.iteri (fun x y e -> if e = 1 then field.[y + t.X, x + t.Y] <- space.Occupied)
        //t.Shape |> Array2D.iteri (fun x y e -> if e = 1 then updateBoard (y + t.X) (x + t.Y))

    member x.Restart =
        field <- Array2D.zeroCreate<space> width height

