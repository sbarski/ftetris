module ZeroConf

open System
open Mono.Zeroconf
open Mono.Zeroconf.Providers
open Mono.Zeroconf.Providers.Bonjour

open System.Net.Sockets

type net = Join | Host
type game = {ip:Net.IPAddress; port: int16; hostTarget: string}
type server = {ip:Net.IPAddress; port: int16; advertse:bool; bonjour: Mono.Zeroconf.Providers.Bonjour.RegisterService; listener: TcpListener}

let private browser = new ServiceBrowser()
let private currentGames = ref (Array.zeroCreate<game> 0)

let private serviceResolved (args:Mono.Zeroconf.ServiceResolvedEventArgs) =
    let service = args.Service

    let ipAddress = service.HostEntry.AddressList.[0]
    let port = service.Port
    let hostTarget = service.HostTarget

    if not (currentGames.Value |> Array.exists (fun x -> x.ip = ipAddress)) then
        Array.Resize(currentGames, currentGames.Value.Length + 1)
        currentGames.Value.[currentGames.Value.Length-1] <- {ip = ipAddress; port = port; hostTarget = hostTarget}


let private serviceAdded (args:Mono.Zeroconf.ServiceBrowseEventArgs) =
    args.Service.Resolved.Add(serviceResolved)
    args.Service.Resolve()

let init action =
    let port = 3689
    let ip = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.[0].MapToIPv4()

    match action with
    | Join ->     
                let client = Client.connect ip port

                browser.ServiceAdded.Add(serviceAdded)
                browser.Browse((uint32)0, Mono.Zeroconf.AddressProtocol.Any, "_daap._tcp", "local.")
    | Host -> 
                let listener = Server.start System.Net.IPAddress.Loopback port

                let service = new RegisterService()
                service.Name <- "ftetris"
                service.RegType <- "_daap._tcp"
                service.ReplyDomain <- "local."
                service.AddressProtocol <- Mono.Zeroconf.AddressProtocol.Any
                service.InterfaceIndex <- (uint32)0
                service.Port <- (int16)3689
                service.Register()



