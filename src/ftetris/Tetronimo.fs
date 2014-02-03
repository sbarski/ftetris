module Tetronimo

open System
open System.Drawing

open OpenTK.Input
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL

let I = array2D [|
                [|1; 1; 1; 1|]
                [|0; 0; 0; 0|]
                |]

let O = array2D [|
                [|1; 1;|]
                [|1; 1;|]
                |]

let T = array2D [|
                [|0; 1; 0|]
                [|1; 1; 1|]
                [|0; 0; 0|]
                |]

let S = array2D [|
                [|0; 1; 1|]
                [|1; 1; 0|]
                [|0; 0; 0|]
                |]

let Z = array2D [|
                [|1; 1; 0|]
                [|0; 1; 1|]
                [|0; 0; 0|]
                |]

let J = array2D [|
                [|1; 0; 0|]
                [|1; 1; 1|]
                [|0; 0; 0|]
                |]

let L = array2D [|
                [|0; 0; 1|]
                [|1; 1; 1|]
                [|0; 0; 0|]
                |]

let mirror x y (shape:int[,]) width height =
    let offset = if width % 2 = 0 || height % 2 = 0 then 1 else 2

    match y with
    | y when y = 0 -> shape.[y+offset,x]
    | y when y = offset -> shape.[y-offset,x]
    | _ -> shape.[y,x]

let transpose shape =
    let width = Array2D.length2 shape
    let height = Array2D.length1 shape

    Array2D.init width height (fun x y -> mirror x y shape width height) 

type Tetronimo(name: string, shape: int[,], ?color: Color, ?x: int, ?y: int, ?speed: float) =
    let mutable y = defaultArg y 0
    let mutable x = defaultArg x 3
    let mutable shape = Array2D.init (Array2D.length1 shape) (Array2D.length2 shape) (fun x y -> shape.[x,y])
    let mutable speed = defaultArg speed 0.8

    let name = name
    let color = match color with
                | None -> Color.Gray
                | Some c -> c

    let draw (row:int) (column:int) =
        let x = row + x
        let y = column + y

        GL.Vertex2(x, y)
        GL.Vertex2(x, y + 1)
        GL.Vertex2(x + 1, y + 1)
        GL.Vertex2(x + 1, y)

    member this.Draw() =
        GL.Color3(color);
        GL.Begin(PrimitiveType.Quads)
        shape |> Array2D.iteri (fun row column elem -> if elem = 1 then draw column row)
        GL.End()

    member this.Shape with get() = shape 
    member this.X with get() = x and set(_x) = x <- _x
    member this.Y with get() = y and set(_y) = y <- _y
    member this.Name with get() = name
    member this.Color with get() = color
    member this.Speed with get() = speed and set(_speed) = speed <- _speed

type TetronimoFactory(x, y) =
    let fallSpeed = 0.001
    let random = System.Random()
    let tetronimos = [I; O; T; S; Z; J; L]
    let colours = [Color.Aquamarine; Color.Yellow; Color.Pink; Color.LightCyan; Color.Red; Color.Blue; Color.Orange]

    member this.Create =
        let selection = random.Next(0, tetronimos.Length-1)
        let piece = tetronimos.[selection]
        let colour = colours.[selection]
        new Tetronimo("tetronimo", piece, colour, x, y)

    member this.AttemptMove key (t:Tetronimo)= 
        match key with
        | Key.Left -> new Tetronimo(t.Name, t.Shape, t.Color, t.X - 1, t.Y, t.Speed)
        | Key.Right -> new Tetronimo(t.Name, t.Shape, t.Color, t.X + 1, t.Y, t.Speed)
        | Key.Down -> new Tetronimo(t.Name, t.Shape, t.Color, t.X, t.Y + 1, t.Speed)
        | Key.Space -> new Tetronimo(t.Name, t.Shape, t.Color, t.X, t.Y + 1, fallSpeed)
        | _ -> new Tetronimo(t.Name, transpose t.Shape, t.Color, t.X, t.Y, t.Speed)