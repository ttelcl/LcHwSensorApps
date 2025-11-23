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

namespace TteLcl.IpcTools;

/// <summary>
/// A pair of <see cref="ManualResetEvent"/>s that together gate access to a
/// resource that evolves in alternating "even" and "odd" phases. This implementation
/// serves both writers and readers.
/// </summary>
public class EvenOddGate: IDisposable
{
  private readonly EventWaitHandle _evenEvent;
  private readonly EventWaitHandle _oddEvent;
  private bool _disposed;

  /// <summary>
  /// Create a new EvenOddGate
  /// </summary>
  /// <param name="prefix">
  /// The prefix of the event names. Suffixes ".EvenEvent" and ".OddEvent" will be appended
  /// </param>
  /// <param name="global">
  /// If true, use an additional "Global\" prefix for the event names
  /// </param>
  /// <param name="writable">
  /// If true the APIs to write the events are enabled, and the events are newly created.
  /// If false, the APIs to write are disabled, and existing events are opened
  /// </param>
  public EvenOddGate(
    string prefix,
    bool global = false,
    bool writable = false)
  {
    prefix = global ? "Global\\" + prefix : prefix;
    var nameEven = prefix + ".EvenEvent";
    var nameOdd = prefix + ".OddEvent";
    Writable = writable;
    if(writable)
    {
      _evenEvent = new EventWaitHandle(false, EventResetMode.ManualReset, nameEven, out var createdEven);
      _oddEvent = new EventWaitHandle(false, EventResetMode.ManualReset, nameOdd, out var createdOdd);
      if(createdEven != createdOdd)
      {
        throw new InvalidOperationException(
          "Conflicting event creation state");
      }
      if(createdEven)
      {
        PhaseOdd = false;
      }
      else
      {
        DetectPhase();
      }
      _oddEvent.Reset();
      _evenEvent.Reset();
      // we are really in limbo state before the first publication!
    }
    else
    {
      _evenEvent = EventWaitHandle.OpenExisting(nameEven);
      _oddEvent = EventWaitHandle.OpenExisting(nameOdd);
      DetectPhase();
    }
  }

  /// <summary>
  /// True during the 'odd' phase, false during the 'even' phase
  /// </summary>
  public bool PhaseOdd {  get; private set; }

  /// <summary>
  /// If true, this is the server side and we can change phases.
  /// If false, this is the reader side and we are just waiting for phase changes.
  /// </summary>
  public bool Writable { get; }

  /// <summary>
  /// Start the transition from one phase to the other.
  /// This starts the short duration transition phase in which both events are
  /// blocking.
  /// It should be followed by <see cref="CompleteTransition(bool)"/> soon after.
  /// </summary>
  public void StartTransition()
  {
    if(!Writable)
    {
      throw new InvalidOperationException(
        "This EvenOddGate is read-only");
    }
    // One of these is a no-op
    _evenEvent.Reset();
    _oddEvent.Reset();
  }

  /// <summary>
  /// Complete the transition started with <see cref="StartTransition"/>, to the
  /// indicated phase.
  /// </summary>
  /// <param name="toOdd">
  /// If true, this is the even-to-odd transition; After returning <see cref="PhaseOdd"/>
  /// will be true and the odd event will be unblocked and the even event will still block.
  /// If false, it is the other way around.
  /// </param>
  public void CompleteTransition(bool toOdd)
  {
    if(!Writable)
    {
      throw new InvalidOperationException(
        "This EvenOddGate is read-only");
    }
    PhaseOdd = toOdd;
    if(toOdd)
    {
      _oddEvent.Set();
    }
    else
    {
      _evenEvent.Set();
    }
  }

  /// <summary>
  /// Wait for the given event to become signaled. This method
  /// blocks the current thread.
  /// See also <see cref="RegisterWait{T}(bool, Action{T, bool}, T, int)"/>
  /// for a non-blocking alternative.
  /// </summary>
  /// <param name="odd">
  /// If true, wait for the odd event (indicating new odd data)
  /// If false, wait for the even event instead
  /// </param>
  /// <param name="timeoutMillis">
  /// The timeout in milliseconds. If -1, the timeout is infinite
  /// </param>
  /// <returns>
  /// True on success, false on timeout.
  /// </returns>
  public bool WaitOne(bool odd, int timeoutMillis)
  {
    if(odd)
    {
      return _oddEvent.WaitOne(timeoutMillis);
    }
    else
    {
      return _evenEvent.WaitOne(timeoutMillis);
    }
  }

  /// <summary>
  /// Register a callback to be triggered when the indicated event is signaled. This overload allows passing
  /// a state object that is passed to the callback
  /// </summary>
  /// <typeparam name="T">
  /// The type of your state object.
  /// </typeparam>
  /// <param name="odd">
  /// True to wait for the odd data, false to wait for the even data
  /// </param>
  /// <param name="callback">
  /// The callback to be invoked when the event is signaled or the timeout expires
  /// </param>
  /// <param name="state">
  /// Your state object
  /// </param>
  /// <param name="timeoutMillis">
  /// The timeout in milliseconds
  /// </param>
  /// <returns>
  /// The <see cref="RegisteredWaitHandle"/>. Call its <see cref="RegisteredWaitHandle.Unregister(WaitHandle?)"/>
  /// method to unregister the registration (with a null argument)
  /// </returns>
  public RegisteredWaitHandle RegisterWait<T>(
    bool odd, Action<T, bool> callback, T state, int timeoutMillis)
    where T: class
  {
    // Note: the executeOnlyOnce argument needs to be "true" for ManualResetEvents
    return ThreadPool.RegisterWaitForSingleObject(
      odd ? _oddEvent : _evenEvent, (o, timedOut) => callback((T)o!, timedOut), state, timeoutMillis, true);
  }

  //public RegisteredWaitHandle RegisterWait(bool odd, Action<bool> callback, int timeoutMillis)
  //{
  //  // Note: the executeOnlyOnce argument needs to be "true" for ManualResetEvents
  //  return ThreadPool.RegisterWaitForSingleObject(
  //    odd ? _oddEvent : _evenEvent, (_, timedOut) => callback(timedOut), null, timeoutMillis, true);
  //}

  private void DetectPhase()
  {
    // Best effort phase detection.
    // In a transition state this may block for up to 100 milliseconds
    var x = WaitHandle.WaitAny([_evenEvent, _oddEvent], 100);
    if(x == WaitHandle.WaitTimeout)
    {
      // recovering from a crashed state, I guess?
      PhaseOdd = false;
    }
    else
    {
      PhaseOdd = x == 1;
    }
  }

  /// <summary>
  /// Dispose pattern
  /// </summary>
  /// <param name="disposing"></param>
  protected virtual void Dispose(bool disposing)
  {
    if(!_disposed)
    {
      _disposed=true;
      if(disposing)
      {
        _evenEvent.Dispose();
        _oddEvent.Dispose();
      }
    }
  }

  /// <inheritdoc/>
  public void Dispose()
  {
    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}
