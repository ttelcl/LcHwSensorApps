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

namespace Ttelcl.HwAppLib;

/// <summary>
/// Description of SensorMapVisitor
/// </summary>
public class SensorMapVisitor: IVisitor
{
  /// <summary>
  /// Create a new SensorMapVisitor
  /// </summary>
  public SensorMapVisitor()
  {
    SensorMap = [];
  }

  /// <summary>
  /// The mapping of identifiers to sensors being built by this visitor
  /// </summary>
  public Dictionary<string, ISensor> SensorMap { get; }

  void IVisitor.VisitComputer(IComputer computer)
  {
    // Console.WriteLine($"[Visiting computer]");
    computer.Traverse(this);
  }

  void IVisitor.VisitHardware(IHardware hardware)
  {
    // Console.WriteLine($"[Traversing hardware {hardware.Identifier}]");
    hardware.Traverse(this);
    // Console.WriteLine($"[Visiting children of hardware {hardware.Identifier}]");
    foreach(var subHardware in hardware.SubHardware)
    {
      subHardware.Accept(this);
    }
  }

  void IVisitor.VisitSensor(ISensor sensor)
  {
    // Console.WriteLine($"[Visiting sensor {sensor.Identifier}]");
    SensorMap[sensor.Identifier.ToString()] = sensor;
  }

  void IVisitor.VisitParameter(IParameter parameter)
  {
  }
}