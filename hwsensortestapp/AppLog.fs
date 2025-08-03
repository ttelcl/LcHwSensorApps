module AppLog

open System
open System.IO
open System.Security.Principal

open LibreHardwareMonitor
open LibreHardwareMonitor.Hardware

open Ttelcl.HwAppLib

open CommonTools
open ColorPrint
open System.Globalization

type private Options = {
  IntervalMillis: int
  Sensors: string list
}

let private runLog o =
  let isAdmin =
    let identity = WindowsIdentity.GetCurrent()
    let principal = new WindowsPrincipal(identity);
    principal.IsInRole(WindowsBuiltInRole.Administrator)
  if isAdmin then
    cp "\fbRunning as admin\f0."
  else
    cp "\fyRunning as non-admin\f0. Some information may be missing."
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
  computer.Accept(new UpdateVisitor())
  let sensormap =
    let sensormapVisitor = new SensorMapVisitor()
    sensormapVisitor |> computer.Accept
    sensormapVisitor.SensorMap
  let mutable sensorsMissing = false
  let sensorCollection = new SensorCollection()
  for sensorName in o.Sensors do
    let ok, sensor = sensorName |> sensormap.TryGetValue
    if ok then
      let added = sensor |> sensorCollection.AddSensor
      if added |> not then
        cp $"\foWarning:\fy ignoring duplicate sensor \fb{sensorName}\f0."
    else
      sensorsMissing <- true
      cp $"\frError:\fo Unknown sensor \fy{sensorName}\f0."
  if sensorsMissing then
    cp ""
    Usage.usage "log"
    1
  else
    cp $"All \fb{sensorCollection.Sensors.Count}\f0 sensors are available (across \fb{sensorCollection.Hardwares.Count}\f0 devices)."
    let outputname =
      let adminphrase = if isAdmin then "admin" else "non-admin"
      let ts = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss")
      let name = $"sensorlog.{adminphrase}.{ts}.csv"
      name |> Path.GetFullPath
    cp $"Logging to \fg{outputname |> Path.GetFileName}\f0."
    do
      use w = outputname |> startFile
      let writeHeader () =
        w.Write("time")
        for s in sensorCollection.Sensors do
          w.Write(',')
          w.Write(s.Identifier.ToString())
        w.WriteLine()
      let writeValues () =
        for h in sensorCollection.Hardwares do
          h.Update()
        // use actual time, not intended stamp
        let ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        w.Write(ts)
        for s in sensorCollection.Sensors do
          w.Write(',')
          if s.Value.HasValue then // else "write an empty string" (i.e. do nothing)
            let txt = s.Value.Value.ToString("G", CultureInfo.InvariantCulture)
            w.Write(txt)
      writeHeader()
      writeValues()
      let mutable stamp = DateTime.Now
      cp "\frImplementation incomplete\f0 - no timer yet"
      ()
    outputname |> finishFile

    0

let run args =
  let rec parseMore o args =
    match args with
    | "-v" :: rest ->
      verbose <- true
      rest |> parseMore o
    | "-h" :: _ ->
      None
    | "-interval" :: secondsText :: rest 
    | "-i" :: secondsText :: rest ->
      let ok, seconds = Double.TryParse(secondsText, CultureInfo.InvariantCulture)
      if ok then
        let millis = int(seconds * 1000.0)
        if millis < 500 || millis > 600000 then // 0.5 sec to 10 min
          cp $"\frError\fo: Value out of range: \fb{secondsText}\f0 Valid range is 0.5 - 600."
          None
        else
          rest |> parseMore {o with IntervalMillis = millis}
      else
        cp $"\frError\fo: Unrecognized number format in \fb{secondsText}\f0."
        None
    | [] ->
      if o.Sensors |> List.isEmpty then
        cp "\foNo sensors specified\f0."
        None
      else
        {o with Sensors = o.Sensors |> List.rev} |> Some
    | x :: rest ->
      if x.StartsWith('/') then
        rest |> parseMore {o with Sensors = x :: o.Sensors}
      else
        cp $"\frUnrecognized argument:\fo {x}\f0."
        None
  let oo = args |> parseMore {
    IntervalMillis = 1000
    Sensors = []
  }
  match oo with
  | None ->
    Usage.usage "log"
    1
  | Some(o) ->
    o |> runLog


