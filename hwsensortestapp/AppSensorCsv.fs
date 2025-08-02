module AppSensorCsv

open System
open System.IO
open System.Security.Principal

open LibreHardwareMonitor
open LibreHardwareMonitor.Hardware

open Ttelcl.HwAppLib

open CommonTools
open ColorPrint

type private OutputNameType =
  | Generate
  | Given of string

type private Options = {
  OutputName: OutputNameType
}

let private runApp o =
  let isAdmin =
    let identity = WindowsIdentity.GetCurrent()
    let principal = new WindowsPrincipal(identity);
    principal.IsInRole(WindowsBuiltInRole.Administrator)
  if isAdmin then
    cp "\fbRunning as admin\f0."
  else
    cp "\fyRunning as non-admin\f0. Some information may be missing."
  let outputname =
    match o.OutputName with
    | Given(name) ->
      name |> Path.GetFullPath
    | Generate ->
      let adminphrase = if isAdmin then "admin" else "non-admin"
      let ts = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss")
      let name = $"sensorvalues.{adminphrase}.{ts}.csv"
      name |> Path.GetFullPath
  cp $"Output file name is \fg{outputname |> Path.GetFileName}\f0."
  use wrapper =
    let computer = new Computer()
    computer.IsCpuEnabled <- true
    computer.IsGpuEnabled <- true
    computer.IsMotherboardEnabled <- true
    computer.IsControllerEnabled <- true
    computer.IsPsuEnabled <- true
    new ComputerWrapper(computer, false)
  let computer = wrapper.Computer
  computer.Open()
  // cp "Running an update."
  let visitor = new UpdateVisitor()
  computer.Accept(visitor)
  do
    use w = outputname |> startFile
    w.WriteLine("id,hwkind,kind,value,min,max,name")
    let rec writeHardwareSensors (hardware: IHardware) =
      cp $"Processing \fg{hardware.Identifier}\f0 (\fc{hardware.HardwareType}\f0)"
      for sensor in hardware.Sensors do
        let id = sensor.Identifier.ToString()
        let hwkind = hardware.HardwareType.ToString()
        let kind = sensor.SensorType.ToString()
        let value = if sensor.Value.HasValue then sensor.Value.Value.ToString("G") else ""
        let min = if sensor.Min.HasValue then sensor.Min.Value.ToString("G") else ""
        let max = if sensor.Max.HasValue then sensor.Max.Value.ToString("G") else ""
        let name = sensor.Name
        let line =
          String.Join(',', [|
            id
            hwkind
            kind
            value
            min
            max
            name
            |])
        w.WriteLine(line)
        ()
      for subHardware in hardware.SubHardware do
        subHardware |> writeHardwareSensors
    for hardware in computer.Hardware do
      hardware |> writeHardwareSensors
  outputname |> finishFile
  cp "\fgDone\f0."
  0

let run args =
  let rec parseMore (o:Options) args =
    match args with
    | "-v" :: rest ->
      verbose <- true
      rest |> parseMore o
    | "-h" :: _ ->
      None
    | [] ->
      Some o
    | "-O" :: rest ->
      rest |> parseMore {o with OutputName = Generate}
    | "-o" :: filename :: rest ->
      rest |> parseMore {o with OutputName = filename |> Given}
    | x :: _ ->
      cp $"\frUnrecognized argument:\fo {x}\f0."
      None
  let oo = args |> parseMore {
    OutputName = Generate
  }
  match oo with
  | None ->
    Usage.usage "sensorcsv"
    1
  | Some(o) ->
    o |> runApp

