/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LibreHardwareMonitor.Hardware;

namespace Ttelcl.HwAppLib;

/// <summary>
/// Description of SensorLimit
/// </summary>
public class SensorLimit: IDisposable
{
  private readonly EventWaitHandle _eventWaitHandle;
  private readonly ISensor _sensor;

  /// <summary>
  /// Create a new SensorLimit
  /// </summary>
  internal SensorLimit(
    SensorLimitConfig config,
    ISensor sensor)
  {
    Config = config;
    EventName = SensorLimitConfig.GetEventName(config.EventNamePart);
    _eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
    _sensor = sensor;
    if(sensor.Identifier.ToString() != config.SensorId)
    {
      throw new ArgumentException(
        $"The sensor ({sensor.Identifier}) does not match the limit ({config.SensorId})");
    }
  }

  /// <summary>
  /// This limit's configuration
  /// </summary>
  public SensorLimitConfig Config { get; }

  /// <summary>
  /// The name of the manual reset event controlled by this limit
  /// </summary>
  public string EventName { get; }

  /// <summary>
  /// The (manual reset) event controlled by this limit
  /// </summary>
  public EventWaitHandle Event => _eventWaitHandle;

  /// <summary>
  /// True after disposal
  /// </summary>
  public bool Disposed { get; private set; }

  /// <summary>
  /// Update this limit's event based on the sensor value
  /// </summary>
  public void Update()
  {
    if(!Config.Limit.HasValue || !_sensor.Value.HasValue)
    {
      // silently ignore. Event will stay reset
      return;
    }
    var state = _sensor.Value < Config.Limit.Value;
    if(state)
    {
      _eventWaitHandle.Set();
    }
    else
    {
      _eventWaitHandle.Reset();
    }
  }

  /// <summary>
  /// Clean up, deleting the event object
  /// </summary>
  public void Dispose()
  {
    if(!Disposed)
    {
      Disposed = true;
      _eventWaitHandle.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}
