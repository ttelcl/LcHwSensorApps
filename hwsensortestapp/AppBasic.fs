module AppBasic

open System
open System.IO

open LibreHardwareMonitor
open LibreHardwareMonitor.Hardware

open Ttelcl.HwAppLib

open CommonTools
open ColorPrint


type private Options = unit

let private runApp o =
  use wrapper =
    let computer = new Computer()
    computer.IsCpuEnabled <- true
    computer.IsGpuEnabled <- true
    computer.IsMotherboardEnabled <- true
    computer.IsControllerEnabled <- true
    computer.IsPsuEnabled <- true
    new ComputerWrapper(computer, false)
  cp "Wrapper created"
  let computer = wrapper.Computer
  cp "Opening wrapper.Computer"
  computer.Open()
  cp "Requesting report"
  let report = computer.GetReport()
  let ts = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss-fff")
  let reportfile = $"basic-report.{ts}.txt"
  cp $"Saving report as \fg{reportfile}\f0"
  File.WriteAllText(reportfile, report)
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
      Some ()
    | x :: _ ->
      cp $"\frUnrecognized argument:\fo {x}\f0."
      None
  let oo = args |> parseMore ()
  match oo with
  | None ->
    Usage.usage "basic"
    1
  | Some(o) ->
    o |> runApp
      

