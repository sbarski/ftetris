module DataConversion

open System
open System.Runtime
open System.Runtime.InteropServices

open ftetris.types

let sizeOfNetworkStructure = 
    sizeof<Network.TransferDto>

let convertTypeToByteArray (currentType:Network.TransferDto) =
    let size = Marshal.SizeOf(currentType)
    let bytes = Array.zeroCreate<byte> size
    let ptr = Marshal.AllocHGlobal(size)
    Marshal.StructureToPtr(currentType, ptr, false)
    Marshal.Copy(ptr, bytes, 0, size)
    Marshal.FreeHGlobal(ptr)
    bytes

let convertByteArrayToType (byteArray:byte[]) =
    let bytes = Array.zeroCreate<byte> byteArray.Length
    let ptr = Marshal.AllocHGlobal(byteArray.Length)
    Marshal.Copy(bytes, 0, ptr, byteArray.Length)
    let currentType = Marshal.PtrToStructure(ptr, typedefof<Network.TransferDto>) :?> Network.TransferDto
    Marshal.FreeHGlobal(ptr)
    currentType