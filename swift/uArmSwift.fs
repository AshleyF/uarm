namespace Brief.Robotics

open System
open System.Threading
open System.IO.Ports

type Mode = Normal = 0 | Laser = 1 | Printing3D = 2 | UniversalHolder = 3

type UArmSwift(port: string) =
    let com = new SerialPort(port, 115200)
    let exec cmd = com.WriteLine cmd; let r = com.ReadLine() in if r.StartsWith("ok") then r else failwith r
    let command req = exec req |> ignore
    let request cmd = (exec cmd).Substring(3) // "ok ..."
    let boolToInt = function true -> 1 | _ -> 0
    let parseFlag (str: string) = if str.[1] = '0' then false else true
    let parseName (str: string) = str.Substring(1)
    let parseFloat (str: string) = str |> parseName |> Double.Parse
    let parseTripple (str: string) = str.Split().[..2] |> Seq.map parseFloat |> List.ofSeq |> function [x; y; z] -> (x, y, z) | _ -> failwith "Malformed response"
    interface IDisposable with member x.Dispose() = x.Disconnect()
    member this.Connect() = com.Open(); try command "init" |> ignore with _ -> () // hack to clear buffers
    member this.Disconnect() = com.Close()
    member this.IsReachableXYZ(x: float, y: float, z: float) = sprintf "M2222 X%f Y%f Z%f P0" x y z |> request |> parseFlag
    member this.MoveXYZ(x: float, y: float, z: float, speed: float, laser: bool) = sprintf "G%i X%f Y%f Z%f F%f" (boolToInt laser) x y z speed |> command
    member this.MoveXYZ(x: float, y: float, z: float, speed: float) = this.MoveXYZ(x, y, z, speed, false)
    member this.MoveRelativeXYZ(x: float, y: float, z: float, speed: float) = sprintf "G2204 X%f Y%f Z%f F%f" x y z speed |> command
    member this.GetXYZ() = request "P2220" |> parseTripple
    member this.IsReachableRTZ(r: float, t: float, z: float) = sprintf "M2222 X%f Y%f Z%f P0" r t z |> request |> parseFlag
    member this.MoveRTZ(radius: float, theta: float, z: float, speed: float) = sprintf "G2201 S%f R%f H%f F%f" radius theta z speed |> command
    member this.MoveRelativeRTZ(radius: float, theta: float, z: float, speed: float) = sprintf "G2205 S%f R%f H%f F%f" radius theta z speed |> command
    member this.GetRTZ() = request "P2221" |> parseTripple
    member this.GetJointAngle(joint: int) = sprintf "P2206 N%i" joint |> request |> parseFloat
    member this.GetAllJointAngles() = request "P2200" |> parseTripple
    member this.AttachAll() = command "M2017"
    member this.Attach(motor: int) = sprintf "M2201 N%i" motor |> command
    member this.DetachAll() = command "M2019"
    member this.Detach(motor: int) = sprintf "M2202 N%i" motor |> command
    member this.IsAttached(motor: int) = sprintf "M2203 N%i" motor |> request |> parseFlag
    member this.IsMoving() = request "M2200" |> parseFlag
    member this.Pump(on: bool) = sprintf "M2231 V%i" (boolToInt on) |> command
    member this.IsPumping() = request "P2231" |> parseFlag // working/grabbing -> true
    member this.Grip(closed: bool) = sprintf "M2232 V%i" (boolToInt closed) |> command
    member this.IsGripping() = request "P2232" |> parseFlag // working/grabbing -> true
    member this.Joint(num: int, angle: float) = sprintf "G2202 N%i V%f" num angle
    member this.Buzz(frequency, duration) = sprintf "M2210 F%i T%i" frequency duration |> command
    member this.Bluetooth(enabled: bool) = sprintf "M2234 V%i" (boolToInt enabled) |> command
    member this.BluetoothName(name: string) = sprintf "M2245 V%s" name |> command
    member this.ButtonFunction(dflt: bool) = sprintf "M2213 V%i" (boolToInt dflt) |> command
    member this.Mode(mode: Mode) = sprintf "M2400 S%i" (int mode) |> command
    member this.DeviceName() = "P2201" |> request |> parseName
    member this.HardwareVersion() = "P2202" |> request |> parseName
    member this.SoftwareVersion() = "P2203" |> request |> parseName
    member this.APIVersion() = "P2204" |> request |> parseName
    member this.XYZtoJoints(x: float, y: float, z: float) = sprintf "M2220 X%f Y%f Z%f" x y z |> request |> parseTripple
    member this.JointsToXYZ(bse: float, left: float, right: float) = sprintf "M2221 B%f L%f R%f" bse left right |> request |> parseTripple
    member this.SetDigitalOutputPin(pin: int, value: int) = sprintf "M2240 N%i V%i" pin value |> command
    member this.SetZeroPosition() = command "M2401"
    member this.SetHeightZeroPoint() = command "M2410"
    member this.SetEndEffectorOffset(mm: float) = sprintf "M2411 S%f" mm |> command
    member this.IsLimitSwitchTripped() = request "P2233" |> parseFlag
    member this.IsPowerConnected() = request "P2234" |> parseFlag
    member this.GetDigitalPinValue(pin: int) = sprintf "P2240 N%i" pin |> request |> parseFlag
    member this.GetAnalogPinValue(pin: int) = sprintf "P2241 N%i" pin |> request |> parseFloat

    // TOOD: Set time cycle of feedback - M2120
    // TODO: Read/write EEPROM (M2211/M2212)