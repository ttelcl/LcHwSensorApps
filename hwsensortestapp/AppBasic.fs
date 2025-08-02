module AppBasic

open System
open System.IO
open System.Security.Principal

open LibreHardwareMonitor
open LibreHardwareMonitor.Hardware

open Ttelcl.HwAppLib

open CommonTools
open ColorPrint

type private Options = {
  Update: bool
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
  if o.Update then
    cp "Running an update (\fg-update\f0)"
    let visitor = new UpdateVisitor()
    computer.Accept(visitor)
  cp "Requesting report"
  let report = computer.GetReport()
  let ts = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss-fff")
  let tagStrings = [|
    if o.Update then "update" else "noupdate"
    if isAdmin then "admin" else "noadmin"
    |]
  let tag = String.Join("-", tagStrings)
  let reportfile = $"basic-report.{tag}.{ts}.txt"
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
      Some o
    | "-update" :: rest ->
      rest |> parseMore {o with Update = true}
    | "-noupdate" :: rest ->
      rest |> parseMore {o with Update = false}
    | x :: _ ->
      cp $"\frUnrecognized argument:\fo {x}\f0."
      None
  let oo = args |> parseMore {
    Update = false
  }
  match oo with
  | None ->
    Usage.usage "basic"
    1
  | Some(o) ->
    o |> runApp
      

