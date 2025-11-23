/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LibreHardwareMonitor.Hardware;

using Newtonsoft.Json;

namespace Ttelcl.HwAppLib;

/// <summary>
/// A serializable selection of computer parts to enable
/// </summary>
public class ComputerParts
{
  /// <summary>
  /// Create a new ComputerParts
  /// </summary>
  public ComputerParts(
    bool battery = false,
    bool controller = false,
    bool cpu = false,
    bool gpu = false,
    bool memory = false,
    bool motherboard = false,
    bool network = false,
    bool psu = false,
    bool storage = false)
  {
    BatteryEnabled = battery;
    ControllerEnabled = controller;
    CpuEnabled = cpu;
    GpuEnabled = gpu;
    MemoryEnabled = memory;
    MotherboardEnabled = motherboard;
    NetworkEnabled = network;
    PsuEnabled = psu;
    StorageEnabled = storage;
  }

  /// <summary>
  /// Create a ComputerParts instance from a list of part names
  /// </summary>
  /// <param name="partNames"></param>
  /// <returns></returns>
  public static ComputerParts FromList(IEnumerable<string> partNames)
  {
    var parts = new ComputerParts();
    foreach(var partName in partNames)
    {
      parts[partName] = true;
    }
    return parts;
  }

  /// <summary>
  /// True if battery data is included
  /// </summary>
  [JsonProperty("battery")]
  public bool BatteryEnabled { get; private set; }

  /// <summary>
  /// True if controller data is enabled
  /// </summary>
  [JsonProperty("controller")]
  public bool ControllerEnabled { get; private set; }

  /// <summary>
  /// True if CPU data is enabled
  /// </summary>
  [JsonProperty("cpu")]
  public bool CpuEnabled { get; private set; }

  /// <summary>
  /// True if GPU data is enabled
  /// </summary>
  [JsonProperty("gpu")]
  public bool GpuEnabled { get; private set; }

  /// <summary>
  /// True if Memory data is enabled
  /// </summary>
  [JsonProperty("memory")]
  public bool MemoryEnabled { get; private set; }

  /// <summary>
  /// True if motherboard data is enabled
  /// </summary>
  [JsonProperty("motherboard")]
  public bool MotherboardEnabled { get; private set; }

  /// <summary>
  /// True if network data is enabled
  /// </summary>
  [JsonProperty("network")]
  public bool NetworkEnabled { get; private set; }

  /// <summary>
  /// True if PSU data is enabled
  /// </summary>
  [JsonProperty("psu")]
  public bool PsuEnabled { get; private set; }

  /// <summary>
  /// True if storage data is enabled
  /// </summary>
  [JsonProperty("storage")]
  public bool StorageEnabled { get; private set; }

  /// <summary>
  /// Get or set a flag by name
  /// </summary>
  [JsonIgnore]
  public bool this[string partName] {
    get {
      return partName.ToLowerInvariant() switch {
        "battery" => BatteryEnabled,
        "controller" => ControllerEnabled,
        "cpu" => CpuEnabled,
        "gpu" => GpuEnabled,
        "memory" => MemoryEnabled,
        "motherboard" => MotherboardEnabled,
        "network" => NetworkEnabled,
        "psu" or "power" => PsuEnabled,
        "storage" => StorageEnabled,
        _ => throw new ArgumentException(
          $"Unrecognized computer part key: {partName}"),
      };
    }
    set {
      switch(partName.ToLowerInvariant())
      {
        case "battery":
          BatteryEnabled = value;
          break;
        case "controller":
          ControllerEnabled = value;
          break;
        case "cpu":
          CpuEnabled = value;
          break;
        case "gpu":
          GpuEnabled = value;
          break;
        case "memory":
          MemoryEnabled = value;
          break;
        case "motherboard":
          MotherboardEnabled = value;
          break;
        case "network":
          NetworkEnabled = value;
          break;
        case "psu":
        case "power":
          PsuEnabled = value;
          break;
        case "storage":
          StorageEnabled = value;
          break;
        default:
          throw new ArgumentException(
            $"Unrecognized computer part key: {partName}");
      }
    }
  }

  /// <summary>
  /// Emit the enabled parts as a sequence of part names
  /// </summary>
  /// <returns></returns>
  public IEnumerable<string> ToList()
  {
    if(BatteryEnabled)
      yield return "battery";
    if(ControllerEnabled)
      yield return "controller";
    if(CpuEnabled)
      yield return "cpu";
    if(GpuEnabled)
      yield return "gpu";
    if(MemoryEnabled)
      yield return "memory";
    if(MotherboardEnabled)
      yield return "motherboard";
    if(NetworkEnabled)
      yield return "network";
    if(PsuEnabled)
      yield return "psu";
    if(StorageEnabled)
      yield return "storage";
  }

  /// <summary>
  /// Apply these settings to a <see cref="Computer"/> instance
  /// </summary>
  /// <param name="computer"></param>
  public void Apply(Computer computer)
  {
    computer.IsBatteryEnabled = BatteryEnabled;
    computer.IsControllerEnabled = ControllerEnabled;
    computer.IsCpuEnabled = CpuEnabled;
    computer.IsGpuEnabled = GpuEnabled;
    computer.IsMemoryEnabled = MemoryEnabled;
    computer.IsMotherboardEnabled = MotherboardEnabled;
    computer.IsNetworkEnabled = NetworkEnabled;
    computer.IsPsuEnabled = PsuEnabled;
    computer.IsStorageEnabled = StorageEnabled;
  }
}
