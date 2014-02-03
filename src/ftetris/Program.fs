open System
open System.Drawing

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open OpenTK.Input

open Playfield
open Tetronimo

let defaultx = 4
let defaulty = 0

type GLWindow() as this = 
    inherit GameWindow(600, 800, GraphicsMode.Default)

    let gridList = 1
    let playfield = new Playfield(gridList)
    let factory = new TetronimoFactory(defaultx, defaulty)
    let mutable tetronimo = factory.Create
    let mutable elapsedTime = 0.0

    let resize(e: EventArgs) = 
        GL.Viewport(0, 0, this.Width, this.Height)
        GL.MatrixMode(MatrixMode.Projection)
        GL.LoadIdentity()

        GL.Ortho(0.0, 10.0, 20.0, 0.0, -1.0, 1.0)
        GL.MatrixMode(MatrixMode.Modelview)
        GL.LoadIdentity()

    let load(e: EventArgs) = 
        this.VSync <- VSyncMode.On

    let nextTetronimo =
        tetronimo <- factory.Create
        ()

    let saveMove t =
        tetronimo <- t
        ()

    let moveTetronimo key =
        factory.AttemptMove key tetronimo
        |> playfield.GetNextPosition 
        |> (fun (result, newtetromino) -> 
            match result with
            | space.Empty -> saveMove newtetromino //if empty then save
            | space.Occupied -> if (key <> Key.Left && key <> Key.Right) then
                                    do playfield.SavePosition tetronimo //handle a normal collision
                                    do tetronimo <- factory.Create 
            | space.Filled ->   do playfield.Restart //handle occupied board
                                do tetronimo <- factory.Create
            | space.Wall -> ()  //do nothing and continue
            | _ -> ())

    let keyDown(e: KeyboardKeyEventArgs) = 
        match e.Key with
        | Key.Escape -> this.Exit()
        | Key.Down | Key.Left | Key.Right | Key.Up | Key.Space -> moveTetronimo e.Key
        | _ -> ()

    let renderFrame(e: FrameEventArgs) =
        GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        GL.ClearColor(Color4.Black)
        tetronimo.Draw()
        playfield.Draw
        this.SwapBuffers()

    let updateFrame(e: FrameEventArgs) = 
        elapsedTime <- elapsedTime + e.Time

        if elapsedTime >= tetronimo.Speed then 
            moveTetronimo Key.Down
            elapsedTime <- 0.0

    do this.Resize.Add(resize)
    do this.Load.Add(load)
    do this.KeyDown.Add(keyDown)
    do this.RenderFrame.Add(renderFrame)
    do this.UpdateFrame.Add(updateFrame)


[<EntryPoint>]
let main argv = 
    printfn "%A" argv

    let game = new GLWindow()
    do game.Run(60.0)
    0 // return an integer exit code
