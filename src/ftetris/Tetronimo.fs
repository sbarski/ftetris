module Tetronimo

open System
open System.Drawing

open OpenTK.Input
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL

type Tetronimo = {shape: int[,]; colour:Color; x:int; y:int; speed:float}

let private I = array2D [|
                        [|1; 1; 1; 1|]
                        [|0; 0; 0; 0|]
                        |]

let private O = array2D [|
                        [|1; 1;|]
                        [|1; 1;|]
                        |]

let private T = array2D [|
                        [|0; 1; 0|]
                        [|1; 1; 1|]
                        [|0; 0; 0|]
                        |]

let private S = array2D [|
                        [|0; 1; 1|]
                        [|1; 1; 0|]
                        [|0; 0; 0|]
                        |]

let private Z = array2D [|
                        [|1; 1; 0|]
                        [|0; 1; 1|]
                        [|0; 0; 0|]
                        |]

let private J = array2D [|
                        [|1; 0; 0|]
                        [|1; 1; 1|]
                        [|0; 0; 0|]
                        |]

let private L = array2D [|
                        [|0; 0; 1|]
                        [|1; 1; 1|]
                        [|0; 0; 0|]
                        |]

let private tetronimos = [I; O; T; S; Z; J; L]
let private colours = [Color.Aquamarine; Color.Yellow; Color.Pink; Color.LightCyan; Color.Red; Color.Blue; Color.Orange]

let private defaultX = 3
let private defaultY = 0
let private speed = 0.8
let private fastSpeed = 0.001

let private mirror x y (shape:int[,]) width height =
    let offset = if width % 2 = 0 || height % 2 = 0 then 1 else 2

    match y with
    | y when y = 0 -> shape.[y+offset,x]
    | y when y = offset -> shape.[y-offset,x]
    | _ -> shape.[y,x]

let private transpose shape =
    let width = Array2D.length2 shape
    let height = Array2D.length1 shape

    Array2D.init width height (fun x y -> mirror x y shape width height) 

let private drawsquare (x:int) y =
    GL.Vertex2(x, y)
    GL.Vertex2(x, y + 1)
    GL.Vertex2(x + 1, y + 1)
    GL.Vertex2(x + 1, y)

let draw tetromino =
    GL.Color3(tetromino.colour);
    GL.Begin(PrimitiveType.Quads)
    tetromino.shape |> Array2D.iteri (fun row column elem -> if elem = 1 then drawsquare (column + tetromino.x) (row + tetromino.y))
    GL.End()

let create (random:Random) =
    let selection = random.Next(0, tetronimos.Length-1)
    let shape = tetronimos.[selection]
    let colour = colours.[selection]
    
    {shape = shape; colour = colour; x = defaultX; y = defaultY; speed = speed}

let move key tetromino = 
     match key with
     | Key.Left -> {shape = tetromino.shape; colour = tetromino.colour; x = tetromino.x - 1; y = tetromino.y; speed = tetromino.speed }
     | Key.Right -> {shape = tetromino.shape; colour = tetromino.colour; x = tetromino.x + 1; y = tetromino.y; speed = tetromino.speed }
     | Key.Down -> {shape = tetromino.shape; colour = tetromino.colour; x = tetromino.x; y = tetromino.y + 1; speed = tetromino.speed }
     | Key.Space -> {shape = tetromino.shape; colour = tetromino.colour; x = tetromino.x; y = tetromino.y; speed = fastSpeed }
     | _ -> {shape = transpose tetromino.shape; colour = tetromino.colour; x = tetromino.x; y = tetromino.y; speed = tetromino.speed }