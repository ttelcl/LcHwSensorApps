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
  cp "\frNYI\f0."
  1

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


