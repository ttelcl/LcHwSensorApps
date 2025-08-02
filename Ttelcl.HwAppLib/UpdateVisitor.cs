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
/// Visitor for updating data
/// </summary>
public class UpdateVisitor: IVisitor
{
  /// <summary>
  /// Create a new UpdateVisitor
  /// </summary>
  public UpdateVisitor()
  {
  }

  void IVisitor.VisitComputer(IComputer computer)
  {
    computer.Traverse(this);
  }

  void IVisitor.VisitHardware(IHardware hardware)
  {
    hardware.Update();
    foreach(var subHardware in hardware.SubHardware)
    {
      subHardware.Accept(this);
    }
  }

  void IVisitor.VisitSensor(ISensor sensor)
  {
  }

  void IVisitor.VisitParameter(IParameter parameter)
  {
  }
}
