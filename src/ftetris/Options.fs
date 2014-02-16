module Options

type LocalNetworkOption = Host | Join | None

type CommandLineOptions = {
    localNetworkOptions: LocalNetworkOption
}

type private ParseMode = TopLevel | LocalNetwork
type private FoldState = {options: CommandLineOptions; parseMode: ParseMode}

let private parseTopLevel arg optionsSoFar = 
    match arg with 
    | "/lan" -> 
        {options=optionsSoFar; parseMode=LocalNetwork}
    | x -> 
        printfn "Option '%s' is unrecognized" x
        {options=optionsSoFar; parseMode=TopLevel}

let private parseLocalNetwork arg optionsSoFar = 
    match arg with
    | "host" -> 
        let newOptionsSoFar = { optionsSoFar with localNetworkOptions=Host}
        {options=newOptionsSoFar; parseMode=TopLevel}
    | "join" -> 
        let newOptionsSoFar = { optionsSoFar with localNetworkOptions=Join}
        {options=newOptionsSoFar; parseMode=TopLevel}
    | _ -> 
        printfn "LocalNetwork needs a second argument"
        {options=optionsSoFar; parseMode=TopLevel}

let private foldFunction state element  = 
    match state with
    | {options=optionsSoFar; parseMode=TopLevel} ->
        parseTopLevel element optionsSoFar

    | {options=optionsSoFar; parseMode=LocalNetwork} ->
        parseLocalNetwork element optionsSoFar
           
// create the "public" parse function
let parseCommandLine args = 

    let defaultOptions = {
        localNetworkOptions = None
    }
      
    let initialFoldState = {options=defaultOptions; parseMode=TopLevel}

    args |> List.fold foldFunction initialFoldState |> (fun x -> x.options)

