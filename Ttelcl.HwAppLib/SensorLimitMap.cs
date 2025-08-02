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
/// The collection of <see cref="SensorLimit"/> instances
/// for a <see cref="Computer"/>
/// </summary>
public class SensorLimitMap: IDisposable
{
  private readonly Dictionary<string, SensorLimit> _limits;
  private readonly Dictionary<string, IHardware> _hardwares;

  /// <summary>
  /// Create a new SensorLimitMap
  /// </summary>
  public SensorLimitMap(
    Computer sensorHost,
    IEnumerable<SensorLimitConfig> limitConfigs)
  {
    var mapVisitor = new SensorMapVisitor();
    sensorHost.Accept(mapVisitor);
    var sensorMap = mapVisitor.SensorMap;
    _limits = [];
    _hardwares = [];
    foreach(var config in limitConfigs)
    {
      if(_limits.ContainsKey(config.EventNamePart))
      {
        throw new InvalidOperationException(
          $"Duplicate event name '{config.EventNamePart}' in configuration");
      }
      if(!sensorMap.TryGetValue(config.SensorId, out var sensor))
      {
        throw new InvalidOperationException(
          $"Configured sensor not found: '{config.SensorId}' ({config.EventNamePart})");
      }
      var limit = new SensorLimit(config, sensor);
      _limits.Add(config.EventNamePart, limit);
      var hardware = sensor.Hardware;
      // The same key-value pair may be stored multiple times, but that
      // doesn't matter since the same key always maps to the same instance.
      _hardwares[hardware.Identifier.ToString()] = hardware;
    }
  }

  /// <summary>
  /// The collection containing all <see cref="SensorLimit"/> instances
  /// </summary>
  public IReadOnlyCollection<SensorLimit> Limits => _limits.Values;

  /// <summary>
  /// The collection of all hardwares covering all sensors that are in use.
  /// </summary>
  public IReadOnlyCollection<IHardware> Hardwares => _hardwares.Values;

  /// <summary>
  /// True after disposal
  /// </summary>
  public bool Disposed { get; private set; }

  /// <summary>
  /// Update all hardwares and then update all sensor limit objects
  /// </summary>
  public void UpdateAll()
  {
    ObjectDisposedException.ThrowIf(Disposed, this);
    foreach(var hardware in Hardwares)
    {
      hardware.Update();
    }
    foreach(var limit in Limits)
    {
      limit.Update();
    }
  }

  /// <summary>
  /// Clean up, emptying <see cref="Limits"/> and <see cref="Hardwares"/>
  /// and disposing all limits (thereby disposing all events)
  /// </summary>
  public void Dispose()
  {
    if(!Disposed)
    {
      Disposed = true;
      _hardwares.Clear();
      var limits = _limits.Values.ToList();
      _limits.Clear();
      foreach(var limit in limits)
      {
        limit.Dispose();
      }
    }
  }
}