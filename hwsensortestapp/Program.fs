// (c) 2025  ttelcl / ttelcl

open System

open CommonTools
open ColorPrint
open ExceptionTool
open Usage

let rec run arglist =
  // For subcommand based apps, split based on subcommand here
  match arglist with
  | "-v" :: rest ->
    verbose <- true
    rest |> run
  | "--help" :: _
  | "-h" :: _
  | [] ->
    usage ""
    0  // program return status code to the operating system; 0 == "OK"
  | "basic" :: rest ->
    rest |> AppBasic.run
  | "sensorcsv" :: rest ->
    rest |> AppSensorCsv.run
  | x :: _ ->
    cp $"\foUnrecognized command\f0: \fy{x}\f0."
    1

[<EntryPoint>]
let main args =
  try
    args |> Array.toList |> run
  with
  | ex ->
    ex |> fancyExceptionPrint verbose
    resetColor ()
    1



