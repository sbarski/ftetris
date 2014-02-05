open System
open System.Drawing

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open OpenTK.Input

open Playfield
open Tetronimo
open Menu

let defaultx = 4
let defaulty = 0
let width = 10
let height = 20

type GLWindow() as this = 
    inherit GameWindow(600, 800, GraphicsMode.Default)

    let gridList = 1
    let random = new Random()

    let mutable playfield = Playfield.init gridList width height
    let mutable tetronimo = Tetronimo.create random
    let mutable elapsedTime = 0.0
    let mutable score = 0
    let mutable isGameRunning = true

    let (display, attributes) = TextWriter.init this.Size (new Size(this.Width, 30)) "Score: 0" Brushes.White  

    let resize(e: EventArgs) = 
        GL.Viewport(0, 0, this.Width, this.Height)
        GL.MatrixMode(MatrixMode.Projection)
        GL.LoadIdentity()

        GL.Ortho(0.0, 10.0, 20.0, 0.0, -1.0, 1.0)
        GL.MatrixMode(MatrixMode.Modelview)
        GL.LoadIdentity()

    let load(e: EventArgs) = 
        this.VSync <- VSyncMode.On

    let updateGameState update =
        if (fst update <> playfield) then

            playfield <- fst update

        score <- score + snd update
        tetronimo <- Tetronimo.create random

        TextWriter.update display attributes ("Score: " + score.ToString())

    let handleCollision key =
        if (key <> Key.Left && key <> Key.Right) then
            Playfield.savePosition tetronimo playfield //handle a normal collision
                
            let y = tetronimo.shape |> Array2D.length1 |> fun size -> Array.init size (fun i -> i + tetronimo.y) |> Array.filter (fun x -> x < 20) |> Seq.toList
            let update = Playfield.update playfield y
            
            updateGameState update

    let restartGame message =
        playfield <- Playfield.restart playfield
        tetronimo <- Tetronimo.create random
        score <- 0
        
    let moveTetronimo key =
        let move = Tetronimo.move key tetronimo |> Playfield.getNextPosition playfield
        
        match move with
        | (space.Empty, position) -> tetronimo <- position
        | (space.Occupied, _) -> handleCollision key
        | (space.Filled, _) -> restartGame ""
        | (space.Wall, _) -> ()  //do nothing and continue
        | _ -> () //handle impossible case

    let keyDown(e: KeyboardKeyEventArgs) = 
        match e.Key with
        | Key.Escape -> isGameRunning <- not isGameRunning
        | Key.Down | Key.Left | Key.Right | Key.Up | Key.Space -> if isGameRunning then moveTetronimo e.Key
        | _ -> ()

    let renderFrame(e: FrameEventArgs) =
        GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        GL.ClearColor(Color4.Black)

        Menu.draw width height Menu.Speed
//        Tetronimo.draw tetronimo
//        Playfield.draw playfield
//        TextWriter.draw display attributes this.Width this.Height

        this.SwapBuffers()

    let updateFrame(e: FrameEventArgs) = 
        elapsedTime <- elapsedTime + e.Time

        if isGameRunning then
            if elapsedTime >= tetronimo.speed then 
                moveTetronimo Key.Down
                elapsedTime <- 0.0

    let mouseUp(e: MouseEventArgs) =
        System.Console.WriteLine("Position: " + e.Position.ToString() + ", x and y: " + e.X.ToString() + ", "+ e.Y.ToString())

    do this.Resize.Add(resize)
    do this.Load.Add(load)
    do this.KeyDown.Add(keyDown)
    do this.RenderFrame.Add(renderFrame)
    do this.UpdateFrame.Add(updateFrame)
    do this.Mouse.ButtonUp.Add(mouseUp)


[<EntryPoint>]
let main argv = 
    printfn "%A" argv

    let game = new GLWindow()
    do game.Run(60.0)
    0 // return an integer exit code
