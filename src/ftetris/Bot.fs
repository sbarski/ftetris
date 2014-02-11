module Bot

open System

open Game
open Playfield
open Tetronimo

open OpenTK.Input

let getNextMove tetronimo playfield =
    Move.Down