module Client

open System
open Mono.Zeroconf
open Mono.Zeroconf.Providers
open Mono.Zeroconf.Providers.Bonjour

open System.Net.Sockets
open ftetris.debug
open ftetris.types

let rec asyncGetResponse (stream : NetworkStream) = async {
   while true do
      let! bytes = stream.AsyncRead(DataConversion.sizeOfNetworkStructure)
      let currentType = DataConversion.convertByteArrayToType bytes
      Debug.writef "%A" currentType
}

let send (client:TcpClient) output = 
    let stream = client.GetStream()
    let output = DataConversion.convertTypeToByteArray output
    stream.WriteAsync(output, 0, output.Length)

let connect (ip:System.Net.IPAddress) (port:int) =
    let client = new System.Net.Sockets.TcpClient()

    try
        client.Connect(ip, port)

        printf "Connected to %A %A..." ip port
        let stream = client.GetStream()

        printf "Begin Async Operations"
//        asyncSendInput stream |> Async.Start
        asyncGetResponse stream |> Async.RunSynchronously
    with
    | :? System.Net.Sockets.SocketException -> Debug.writef("Could not connect to server")    

    client
