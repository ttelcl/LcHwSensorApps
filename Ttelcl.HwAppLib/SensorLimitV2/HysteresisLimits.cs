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

using Newtonsoft.Json;

namespace Ttelcl.HwAppLib.SensorLimitV2;

/// <summary>
/// Settings to convert a sequence of sensor values to a boolean to control
/// a <see cref="ManualResetEvent"/>.
/// </summary>
public class HysteresisLimits
{
  /// <summary>
  /// Create a new HysteresisLimits
  /// </summary>
  public HysteresisLimits(
    IEnumerable<string> parts,
    string id,
    string eventkey,
    double passlimit,
    double blocklimit,
    bool nanblocks = true)
  {
    SensorId = id;
    EventKey = eventkey;
    Parts = ComputerParts.FromList(parts);
    EventKey = eventkey;
    PassLimit = passlimit;
    BlockLimit = blocklimit;
    NanBlocks = nanblocks;
  }

  /// <summary>
  /// <see cref="Parts"/> expressed as a sequence of part names
  /// </summary>
  [JsonProperty("parts")]
  public IEnumerable<string> PartNames => Parts.ToList();
  
  /// <summary>
  /// The computer parts that need to be enabled for access to this sensor
  /// </summary>
  [JsonIgnore]
  public ComputerParts Parts { get; }

  /// <summary>
  /// The identifier of the sensor providing the input data
  /// </summary>
  [JsonProperty("id")]
  public string SensorId { get; }

  /// <summary>
  /// Part of the event key (without the 'Global\SensorSwitch.' prefix)
  /// </summary>
  [JsonProperty("eventkey")]
  public string EventKey { get; }

  /// <summary>
  /// The full event name
  /// </summary>
  [JsonIgnore]
  public string EventName => "Global\\SensorSwitch." + EventKey;

  /// <summary>
  /// The limit beyond which the switch always passes.
  /// "Beyond" is to be understood as "on the other side than <see cref="BlockLimit"/> is"
  /// </summary>
  [JsonProperty("passlimit")]
  public double PassLimit { get; }

  /// <summary>
  /// The limit beyond which the switch always blocks.
  /// "Beyond" is to be understood as "on the other side than <see cref="PassLimit"/> is"
  /// </summary>
  [JsonProperty("blocklimit")]
  public double BlockLimit { get; }

  /// <summary>
  /// If true, a NaN value causes the switch to block. If false NaN values are ignored.
  /// </summary>
  [JsonProperty("nanblocks")]
  public bool NanBlocks { get; }
}
