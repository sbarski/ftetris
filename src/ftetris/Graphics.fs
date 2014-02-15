module Graphics

open Game

open System
open System.Drawing

open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open OpenTK.Input

let random = new Random()
let backgroundTextures = Array.zeroCreate<int> 5

let drawBackground width height = 
    let id = backgroundTextures.[3]

    GL.Viewport(0, 0, width, height)
    GL.MatrixMode(MatrixMode.Projection);
    GL.PushMatrix();
    GL.LoadIdentity();

    GL.Ortho(0.0, (float)width, (float)height, 0.0, -1.0, 1.0)

    GL.MatrixMode(MatrixMode.Modelview);
    GL.PushMatrix();
    GL.LoadIdentity();

    GL.Disable(EnableCap.Lighting);
    GL.Enable (EnableCap.Texture2D)

    GL.BindTexture(TextureTarget.Texture2D, id)

    GL.Begin(PrimitiveType.Quads)
    GL.TexCoord2(0, 0)
    GL.Vertex2(0, 0)

    GL.TexCoord2(1, 0)
    GL.Vertex2(width, 0)

    GL.TexCoord2(1, 1)
    GL.Vertex2(width, height)

    GL.TexCoord2(0, 1)
    GL.Vertex2(0, height)
    GL.End()

    GL.Disable(EnableCap.Texture2D);
    GL.PopMatrix();

    GL.MatrixMode(MatrixMode.Projection);
    GL.PopMatrix();

    GL.MatrixMode(MatrixMode.Modelview);

let loadTexture filename =
    if String.IsNullOrEmpty filename then
        raise (System.ArgumentException(filename))

    GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);

    let id = GL.GenTexture()
    GL.BindTexture(TextureTarget.Texture2D, id)

    let image = new Bitmap(filename)
    let data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb)

    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0)
    image.UnlockBits(data)

    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat)

    id


let resizeLeftFrame width height game =
    GL.Viewport(0, 0, width/2, height)
    GL.MatrixMode(MatrixMode.Projection)
    GL.LoadIdentity()
    GL.Ortho(0.0, 10.0, 20.0, 0.0, -1.0, 1.0)
    GL.Scale(0.8, 0.8, 0.0)
    GL.Translate(1.0, 2.0, 0.0)

//    match game with
//    | Game.Local ->
//        GL.Scale(0.8, 0.8, 0.0)
//        GL.Translate(1.0, 2.0, 0.0)
//    | _ -> ()

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

let load =
    backgroundTextures.[0] <- loadTexture @"assets/starburst_1024_blue.png"
    backgroundTextures.[1] <- loadTexture @"assets/starburst_1024_green.png"
    backgroundTextures.[2] <- loadTexture @"assets/starburst_1024_orange.png"
    backgroundTextures.[3] <- loadTexture @"assets/starburst_1024_purple.png"
    backgroundTextures.[4] <- loadTexture @"assets/starburst_1024_red.png"



