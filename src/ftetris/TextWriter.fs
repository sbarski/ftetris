module TextWriter

open System
open System.Drawing
open System.Drawing.Text
open System.Drawing.Imaging

open OpenTK.Graphics
open OpenTK.Graphics.OpenGL

type Display = {text:string; bitmap:Bitmap; }
type Attributes = {colour: Brush; position: PointF; textureId: int}

let private textFont = new Font(FontFamily.GenericSansSerif, 12.0f)

let private createTexture (bitmap:Bitmap) = 
    let mutable textureId = 1
    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, single TextureEnvMode.Replace)
    GL.GenTextures(1, &textureId);
    GL.BindTexture(TextureTarget.Texture2D, textureId);
    
    let data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear)
    GL.Finish()

    bitmap.UnlockBits(data)
    textureId

let private refresh display attributes =
    use gfx = Graphics.FromImage(display.bitmap)
    gfx.Clear(Color.Black)
    gfx.TextRenderingHint <- TextRenderingHint.ClearTypeGridFit
    gfx.DrawString(display.text, textFont, attributes.colour, attributes.position)

    let data = display.bitmap.LockBits(new Rectangle(0, 0, display.bitmap.Width, display.bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
    GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, display.bitmap.Width, display.bitmap.Height, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0)
    display.bitmap.UnlockBits(data)
    display, attributes

let update display attributes text = 
    let display = {text = text; bitmap = display.bitmap}
    refresh display attributes |> ignore

let init (clientSize:Size) (areaSize:Size) text colour =
    let bitmap = new Bitmap(areaSize.Width, areaSize.Height)
    let textureId = createTexture bitmap

    let attributes = {colour = colour; position = new PointF(5.0f, 5.0f); textureId = textureId}
    let display = {text = text; bitmap = bitmap}

    refresh display attributes
    
let draw display attributes width height =
    GL.PushMatrix()
    GL.LoadIdentity()

    let mutable orthoProjection = OpenTK.Matrix4.CreateOrthographicOffCenter(0.0f, single width, single height, 0.0f, -1.0f, 1.0f)
    GL.MatrixMode(MatrixMode.Projection)

    GL.PushMatrix()
    GL.LoadMatrix(&orthoProjection)

    GL.Enable(EnableCap.Blend)
    GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.DstColor)
    GL.Enable(EnableCap.Texture2D)
    GL.BindTexture(TextureTarget.Texture2D, attributes.textureId)

    GL.Begin(PrimitiveType.Quads);
    GL.TexCoord2(0, 0) 
    GL.Vertex2(0, 0)

    GL.TexCoord2(1, 0)
    GL.Vertex2(display.bitmap.Width, 0)

    GL.TexCoord2(1, 1)
    GL.Vertex2(display.bitmap.Width, display.bitmap.Height)

    GL.TexCoord2(0, 1)
    GL.Vertex2(0, display.bitmap.Height)
    GL.End()

    GL.PopMatrix()
    GL.Disable(EnableCap.Blend)
    GL.Disable(EnableCap.Texture2D)
    GL.MatrixMode(MatrixMode.Modelview)
    GL.PopMatrix()

