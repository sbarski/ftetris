module Menu

open System
open System.Drawing

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open OpenTK.Input

type menus = Speed

let private speedMenu width height = 
        GL.Color3(Color.Green);
        GL.Begin(PrimitiveType.Triangles)
        GL.Vertex2(0, 0)
        GL.Vertex2(2, 0)
        GL.Vertex2(0, 4)
        GL.End()

        GL.Color3(Color.GreenYellow)
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex2(2, 0)
        GL.Vertex2(4, 0)
        GL.Vertex2(0, 8)
        GL.Vertex2(0, 4)
        GL.End()

        GL.Color3(Color.Coral)
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex2(4, 0)
        GL.Vertex2(6, 0)
        GL.Vertex2(0, 12)
        GL.Vertex2(0, 8)
        GL.End()

        GL.Color3(Color.LightBlue)
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex2(6, 0)
        GL.Vertex2(8, 0)
        GL.Vertex2(0, 16)
        GL.Vertex2(0, 12)
        GL.End()

        GL.Color3(Color.LightYellow)
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex2(8, 0)
        GL.Vertex2(10, 0)
        GL.Vertex2(0, 20)
        GL.Vertex2(0, 16)
        GL.End()

        GL.Color3(Color.LightPink)
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex2(10, 0)
        GL.Vertex2(10, 4)
        GL.Vertex2(2, 20)
        GL.Vertex2(0, 20)
        GL.End()

        GL.Color3(Color.PaleVioletRed)
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex2(10, 4)
        GL.Vertex2(10, 8)
        GL.Vertex2(4, 20)
        GL.Vertex2(2, 20)
        GL.End()

        GL.Color3(Color.DarkOrange)
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex2(10, 8)
        GL.Vertex2(10, 12)
        GL.Vertex2(6, 20)
        GL.Vertex2(4, 20)
        GL.End()

        GL.Color3(Color.DarkMagenta)
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex2(10, 12)
        GL.Vertex2(10, 16)
        GL.Vertex2(8, 20)
        GL.Vertex2(6, 20)
        GL.End()

        GL.Color3(Color.DarkRed)
        GL.Begin(PrimitiveType.Triangles)
        GL.Vertex2(10, 16)
        GL.Vertex2(10, 20)
        GL.Vertex2(8, 20)
        GL.End()

let draw width height menu =
    match menu with
    | menu when menu = Speed -> speedMenu width height
    | _ -> ()