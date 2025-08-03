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
/// A collection of distinct <see cref="ISensor"/>s. Also tracks the distinct
/// <see cref="IHardware"/>s they are hosted in
/// </summary>
public class SensorCollection
{
  private readonly List<ISensor> _sensorList;
  private readonly Dictionary<Identifier, ISensor> _sensorMap;
  private readonly Dictionary<Identifier, IHardware> _hardwareMap;

  /// <summary>
  /// Create a new empty SensorCollection
  /// </summary>
  public SensorCollection()
  {
    _sensorList = [];
    _sensorMap = [];
    _hardwareMap = [];
  }

  /// <summary>
  /// Try to add a sensor, ignoring requests to add the same sensor
  /// again.
  /// </summary>
  /// <param name="sensor">
  /// The sensor to add
  /// </param>
  /// <returns>
  /// True on success, false if the sensor was already present before.
  /// </returns>
  public bool AddSensor(ISensor sensor)
  {
    if(_sensorMap.ContainsKey(sensor.Identifier))
    {
      return false;
    }
    _sensorMap[sensor.Identifier] = sensor;
    _hardwareMap[sensor.Hardware.Identifier] = sensor.Hardware;
    _sensorList.Add(sensor);
    return true;
  }

  /// <summary>
  /// The (distinct) sensors in this collection
  /// </summary>
  public IReadOnlyList<ISensor> Sensors => _sensorList;

  /// <summary>
  /// The distinct hardwares hosting the sensors in this collection
  /// </summary>
  public IReadOnlyCollection<IHardware> Hardwares => _hardwareMap.Values;

  /// <summary>
  /// Clear this collection
  /// </summary>
  public void Clear()
  {
    _sensorList.Clear();
    _sensorMap.Clear();
    _hardwareMap.Clear();
  }

}
