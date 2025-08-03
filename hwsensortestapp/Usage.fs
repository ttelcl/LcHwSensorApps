// (c) 2025  ttelcl / ttelcl
module Usage

open CommonTools
open ColorPrint

let usage focus =
  cp "\fohwsensortestapp \fybasic \f0[\fg-update\f0|\fg-noupdate\f0]"
  cp "   Basic hardware sensor reading test"
  cp "\fohwsensortestapp \fysensorcsv \f0[\fg-O\f0|\fg-o \fcfile.csv\f0]"
  cp "   Dump current sensor values to a CSV file. Running as admin includes"
  cp "   more sensors."
  cp "   \fg-O\fx\f0              Generate an output name (default)"
  cp "   \fg-o \fcfile.csv\f0     Use the given output file name"
  cp "\fohwsensortestapp \fylog \f0{\fb/sensorid\f0} [\fg-interval \fcseconds\f0]"
  cp "   Log sensor values for the selected sensors at the given interval"
  cp "   until interrupted with CTRL-C"
  cp "   \fb/sensorid\f0\fx       A sensor ID to include (use \fohwsensortestapp \fysensorcsv\f0 to get a list)"
  cp "   \fg-interval \fcsec\f0   Interval between recordings in seconds. Default 1."
  cp ""
  cp "\fg-v               \f0Verbose mode"



