open System
open System.Drawing

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open OpenTK.Input

open Playfield
open Tetronimo
open Menu
open Bot
open Game
open Graphics

let defaultx = 4
let defaulty = 0
let width = 10
let height = 20

type GLWindow() as this = 
    inherit GameWindow(1200, 800, GraphicsMode.Default)

    let random = new Random()

    let playfield_first = ref (Playfield.init 1 width height 0 0)
    let playfield_second = ref (Playfield.init 2 width height 0 0)

    let tetronimo_first = ref (Tetronimo.create random)
    let tetronimo_second = ref (Tetronimo.create random)

    let mutable elapsedTimeFirst = 0.0
    let mutable elapsedTimeSecond = 0.0
    let mutable score = 0
    let mutable gameState = Game.Run
    let mutable gameType = Game.Local

    let (display, attributes) = TextWriter.init this.Size (new Size(this.Width, 30)) "Score: 0" Brushes.White  

    let resize(e: EventArgs) = 
        Graphics.resizeLeftFrame this.Width this.Height gameType
        Graphics.resizeRightFrame this.Width this.Height gameType

    let load(e: EventArgs) = 
        this.VSync <- VSyncMode.On

    let updateGameState (tetronimo: ref<_>) (playfield: ref<_>) update =
        let upd = (fst update)
        let rows = snd update

        if (upd <> playfield.Value) then
            playfield := upd

        score <- score + rows
        tetronimo := Tetronimo.create random

        if rows > 0 then
            match gameType with
            | Type.Local | Type.AI -> 
                                    if playfield = playfield_first then 
                                        playfield_second := Playfield.addRows playfield_second.Value rows 
                                    else 
                                        playfield_first := Playfield.addRows playfield_first.Value rows
            | _ -> ()

        TextWriter.update display attributes ("Score: " + score.ToString())

    let handleCollision (tetronimo : ref<_>) (playfield : ref<_>)  (key:Move) =
        if (key <> Move.Left && key <> Move.Right) then
            Playfield.savePosition tetronimo.Value playfield.Value //handle a normal collision
                
            let y = tetronimo.Value.shape |> Array2D.length1 |> fun size -> Array.init size (fun i -> i + tetronimo.Value.y) |> Array.filter (fun x -> x < 20) |> Seq.toList
            let update = Playfield.update playfield.Value y
            
            updateGameState tetronimo playfield update

    let restartGame message =
        playfield_first := Playfield.restart playfield_first.Value
        playfield_second := Playfield.restart playfield_second.Value

        tetronimo_first := Tetronimo.create random
        tetronimo_second := Tetronimo.create random

        score <- 0
        
    let moveTetronimo (tetronimo : ref<_>) (playfield : ref<_>) (key:Move) =
        let move = Tetronimo.move key tetronimo.Value |> Playfield.getNextPosition playfield.Value
        
        match move with
        | (space.Empty, position) -> tetronimo := position
        | (space.Occupied, _) -> handleCollision tetronimo playfield key
        | (space.Filled, _) -> restartGame  ""
        | (space.Wall, _) -> ()  //do nothing and continue
        | _ -> () //handle impossible case

    let keyDown(e: KeyboardKeyEventArgs) = 
        let key = Game.keyToCommand gameType e.Key

        match e.Key with
        | Key.Escape -> match gameState with
                        | Game.Run -> gameState <- Game.Pause
                        | Game.Pause -> gameState <- Game.Run
                        | _ -> ()
        | Key.A | Key.D | Key.W | Key.S | Key.X -> 
                        if gameState = Game.Run then moveTetronimo tetronimo_first playfield_first key
        | Key.Down | Key.Left | Key.Right | Key.Up | Key.Space -> 
                        if gameState = Game.Run then 
                            if gameType = Game.Local then
                                moveTetronimo tetronimo_second playfield_second key
                            else
                                moveTetronimo tetronimo_first playfield_first key
        | _ -> ()

    let renderFrame(e: FrameEventArgs) =
        GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        GL.ClearColor(Color4.Black)

        match gameState with
        | Game.Menu -> Menu.draw width height Menu.Level
        | Game.Run ->   
                resizeLeftFrame this.Width this.Height gameType
                Tetronimo.draw tetronimo_first.Value         
                Playfield.draw playfield_first.Value
                TextWriter.draw display attributes this.Width this.Height
                
                resizeRightFrame this.Width this.Height gameType
                match gameType with
                | Game.AI   -> 
                                Tetronimo.draw tetronimo_second.Value
                                Playfield.draw playfield_second.Value
                | Game.Local ->  
                                Tetronimo.draw tetronimo_second.Value
                                Playfield.draw playfield_second.Value
                | _ -> ()
        | _ -> ()

        this.SwapBuffers()

    let updateFrame(e: FrameEventArgs) = 
        elapsedTimeFirst <- elapsedTimeFirst + e.Time
        elapsedTimeSecond <- elapsedTimeSecond + e.Time

        if gameState = Game.Run then
            if elapsedTimeFirst >= tetronimo_first.Value.speed then 
                elapsedTimeFirst <- 0.0
                moveTetronimo tetronimo_first playfield_first Move.Down
            
            match gameType with
            | Game.AI ->    if elapsedTimeSecond >= tetronimo_second.Value.speed then
                                let move = Bot.getNextMove tetronimo_second playfield_second
                                moveTetronimo tetronimo_second playfield_second move
                                elapsedTimeSecond <- 0.0
            | Game.Local -> if elapsedTimeSecond >= tetronimo_second.Value.speed then 
                                elapsedTimeSecond <- 0.0
                                moveTetronimo tetronimo_second playfield_second Move.Down
                            
            | _ -> ()


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
