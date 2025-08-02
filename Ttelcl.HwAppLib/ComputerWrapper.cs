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
/// Wraps <see cref="LibreHardwareMonitor.Hardware.Computer"/> to make it disposable, calling 
/// <see cref="Computer.Close"/> upon disposal
/// </summary>
public class ComputerWrapper: IDisposable
{
  private readonly Computer _computer;

  /// <summary>
  /// Create a new ComputerWrapper, wrapping an existing instance
  /// </summary>
  public ComputerWrapper(Computer computer, bool open)
  {
    _computer = computer;
    if(open)
    {
      Computer.Open();
    }
  }

  /// <summary>
  /// The <see cref="LibreHardwareMonitor.Hardware.Computer"/> instance
  /// that is wrapped. Accessing this after calling <see cref="Dispose"/>
  /// will throw an <see cref="ObjectDisposedException"/>.
  /// </summary>
  public Computer Computer {
    get {
      ObjectDisposedException.ThrowIf(Disposed, this);
      return _computer;
    }
  }

  /// <summary>
  /// True after calling dispose
  /// </summary>
  public bool Disposed { get; private set; }

  /// <summary>
  /// Dispose this instance and call Close on the wrapped instance
  /// </summary>
  public void Dispose()
  {
    if(!Disposed)
    {
      Disposed = true;
      _computer.Close();
    }
  }
}
