module Server

open System
open Mono.Zeroconf
open Mono.Zeroconf.Providers
open Mono.Zeroconf.Providers.Bonjour

open System.Net.Sockets
open ftetris.debug

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

let private Handle (client:TcpClient) =
  let clientInfo = client.Client.RemoteEndPoint.ToString()
  Debug.writef "%A" (String.Format("Got connection request from {0}", clientInfo))
  let buffer = Array.zeroCreate 4096

  async {
    use networkStream = client.GetStream()
    let rec loop () = async {
     try
      let! packet = networkStream.AsyncRead buffer
      printfn "%A" ("Got " + packet.ToString() + " bytes from client")
      match packet with
      | 0 -> printfn "%A" (String.Format("Closing the client connection - {0}",
                            clientInfo))
             client.Close()
             ()
      | _ ->
             let res = System.Text.Encoding.UTF32.GetChars(buffer)
             Debug.writef "%A" res
             do! networkStream.AsyncWrite (buffer, 0, packet)
             return! loop()
     with
     | x -> printfn "%A" x
  }
    return! loop()
  } |> Async.Start

let start ip (port:int) =
  let listener = new TcpListener(ip, port)
  listener.Start()

  let rec loop () = async {
    Debug.writef "%A" ("Waiting for connections...");
    try 
      let! client = listener.AcceptTcpClientAsync() |> Async.AwaitTask
      let stream = client.GetStream()
      asyncGetResponse stream |> Async.RunSynchronously
    with
    | _ -> ()
    return! loop()
  }

  loop() |> Async.Start



