/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TteLcl.IpcTools;

/// <summary>
/// The header bytes of a shared buffer
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 32)]
public struct SharedBufferHeader
{
  [FieldOffset(0)] private readonly int _blobSize;
  [FieldOffset(4)] private readonly int _headerSize;
  [FieldOffset(8)] private readonly int _rowCount;
  [FieldOffset(12)] private readonly int _rowSize;
  [FieldOffset(16)] private int _rowIndex;
  [FieldOffset(20)] private readonly int _reserved;
  [FieldOffset(24)] private long _ticks;

  /// <summary>
  /// Create a new SharedBufferHeader
  /// </summary>
  public SharedBufferHeader(
    int rowCount,
    int rowSize,
    int rowIndex,
    DateTime utcStamp)
  {
    _headerSize = 32;
    _reserved = 0;
    _rowCount = rowCount;
    _rowSize = rowSize;
    _rowIndex = rowIndex;
    _ticks = utcStamp.ToUniversalTime().Ticks;
    _blobSize = _headerSize + _rowCount * _rowSize;
  }

  /// <summary>
  /// The blob size (<see cref="HeaderSize"/> + <see cref="RowCount"/> * <see cref="RowSize"/>
  /// </summary>
  public readonly int BlobSize => _blobSize;

  /// <summary>
  /// The header size (32)
  /// </summary>
  public readonly int HeaderSize => _headerSize;

  /// <summary>
  /// The number of allocated rows
  /// </summary>
  public readonly int RowCount => _rowCount;

  /// <summary>
  /// The size in bytes of a row
  /// </summary>
  public readonly int RowSize => _rowSize;

  /// <summary>
  /// The logical index of the next row to write
  /// </summary>
  public int NextIndex {
    get => _rowIndex;
    set => _rowIndex = value;
  }

  /// <summary>
  /// The physical index of the next row to write
  /// (<see cref="NextIndex"/> % <see cref="RowCount"/>)
  /// </summary>
  public int PhysicalNextIndex => _rowIndex % _rowCount;

  /// <summary>
  /// The UTC timestamp of latest modification
  /// </summary>
  public DateTime TimeStamp {
    get => new DateTime(_ticks, DateTimeKind.Utc);
    set => _ticks = value.ToUniversalTime().Ticks;
  }

}
