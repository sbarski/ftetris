module Graphics

open Game

open System

open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open OpenTK.Input

let resizeLeftFrame width height game =
    GL.Viewport(0, 0, width/2, height)
    GL.MatrixMode(MatrixMode.Projection)
    GL.LoadIdentity()
    GL.Ortho(0.0, 10.0, 20.0, 0.0, -1.0, 1.0)

    match game with
    | Game.Local ->
        GL.Scale(0.8, 0.8, 0.0)
        GL.Translate(1.0, 2.0, 0.0)
    | _ -> ()

    GL.MatrixMode(MatrixMode.Modelview)
    GL.LoadIdentity()   

let resizeRightFrame width height game =
    GL.Viewport(width/2, 0, width/2, height)
    GL.MatrixMode(MatrixMode.Projection)
    GL.LoadIdentity()
    GL.Ortho(0.0, 10.0, 20.0, 0.0, -1.0, 1.0)

    match game with
    | Game.AI ->
        GL.Scale(0.5, 0.5, 0.0)
        GL.Translate(5.0, 16.0, 0.0)
    | Game.Local -> 
        GL.Scale(0.8, 0.8, 0.0)
        GL.Translate(1.0, 2.0, 0.0)
    | _ -> ()

    GL.MatrixMode(MatrixMode.Modelview)
    GL.LoadIdentity()