module Game

open OpenTK.Input

type Type = Single | AI | Online | Local
type State = Menu | Level | Run | Pause
type Move = Left | Right | Down | Transpose | Drop | None | Pause

let keyToCommand key =
    match key with
    | Key.Escape -> Pause
    | Key.Left | Key.A -> Move.Left
    | Key.Right | Key.D -> Move.Right
    | Key.Down | Key.S -> Move.Down
    | Key.Up | Key.W -> Move.Transpose 
    | Key.X | Key.Space -> Move.Drop
    | _ -> Move.None