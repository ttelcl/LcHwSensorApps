/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Ttelcl.HwAppLib;

/// <summary>
/// Configuration for one sensor + limit + event name configuration
/// </summary>
public class SensorLimitConfig
{
  /// <summary>
  /// Create a new SensorLimitConfig
  /// </summary>
  public SensorLimitConfig(
    string sensor,
    string eventname,
    double? limit)
  {
    if(!IsValidEventNamePart(eventname))
    {
      throw new ArgumentOutOfRangeException(
        nameof(eventname), "The partial event name does not meet the requirements");
    }
    SensorId = sensor;
    EventNamePart = eventname;
    Limit = limit;
  }

  /// <summary>
  /// Identifies the sensor to monitor
  /// </summary>
  [JsonProperty("sensor")]
  public string SensorId { get; }

  /// <summary>
  /// A part used to construct the <see cref="EventWaitHandle"/> name
  /// </summary>
  [JsonProperty("eventname")]
  public string EventNamePart { get; }

  /// <summary>
  /// The limit for the sensor value that determines the state of the associated event.
  /// If null, this SensorLimit is disabled
  /// </summary>
  [JsonProperty("limit")]
  public double? Limit { get; }

  /// <summary>
  /// Constructs the event name from an event name part
  /// (by prefixing it with 'Global\SensorLimit.')
  /// </summary>
  /// <param name="eventNamePart"></param>
  /// <returns></returns>
  public static string GetEventName(string eventNamePart)
  {
    return "Global\\SensorLimit." + eventNamePart;
  }

  /// <summary>
  /// Check if <paramref name="eventNamePart"/> meets the requirements for being a
  /// valid value for <see cref="EventNamePart"/>.
  /// </summary>
  /// <param name="eventNamePart"></param>
  /// <returns></returns>
  public static bool IsValidEventNamePart(string eventNamePart)
  {
    return Regex.IsMatch(eventNamePart, @"^[a-zA-Z][a-zA-Z0-9]*([-_][a-zA-Z0-9]+)*$");
  }

  /// <summary>
  /// Load a JSON list of zero or more <see cref="SensorLimitConfig"/> instances.
  /// </summary>
  /// <param name="filename"></param>
  /// <returns></returns>
  public static IReadOnlyList<SensorLimitConfig> LoadConfigurations(
    string filename)
  {
    var json = File.ReadAllText(filename);
    var configurations = JsonConvert.DeserializeObject<List<SensorLimitConfig>>(json)!;
    return configurations.AsReadOnly();
  }

}
