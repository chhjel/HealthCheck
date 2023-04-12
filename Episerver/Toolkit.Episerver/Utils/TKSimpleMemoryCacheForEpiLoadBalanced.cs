using EPiServer.Events.Clients;
using QoDL.Toolkit.Core.Util;
using System;

namespace QoDL.Toolkit.Episerver.Utils;

/// <summary>
/// Invalidates cache on other instances when needed using <see cref="IEventRegistry"/>.
/// </summary>
public class TKSimpleMemoryCacheForEpiLoadBalanced : TKSimpleMemoryCacheLoadBalancedBase
{
    /// <summary>
    /// True if any events has been raised.
    /// </summary>
    public bool HasRaisedAnyEvents { get; private set; }

    /// <summary>
    /// True if any events has been received.
    /// </summary>
    public bool HasReceivedAnyEvents { get; private set; }

    /// <summary>
    /// For debugging use.
    /// </summary>
    public Action<Guid, string> OnEventRaised { get; set; }

    internal Guid? _raiserIdOverride;
    internal static readonly Guid _raiserId = Guid.NewGuid();
    internal static readonly Guid _eventId = new("f3b4c847-6482-402b-9a30-d3f8e8b6a239");
    private Guid RaiserId => _raiserIdOverride ?? _raiserId;
    private readonly IEventRegistry _eventRegistry;
    private const string _messageParameterPrefix = "__tk_invalidate_cache|";

    /// <summary>
    /// Invalidates cache on other instances when needed using <see cref="IEventRegistry"/>.
    /// </summary>
    public TKSimpleMemoryCacheForEpiLoadBalanced(IEventRegistry eventRegistry)
    {
        _eventRegistry = eventRegistry;

        var invalidationEvent = eventRegistry.Get(_eventId);
        invalidationEvent.Raised += InvalidationEvent_Raised;
    }

    /// <inheritdoc />
    public override void InvalidateCacheOnOtherInstances(string key = null)
        => RaiseEvent($"{_messageParameterPrefix}{key}");

    private void RaiseEvent(string message)
    {
        HasRaisedAnyEvents = true;
        var ev = _eventRegistry.Get(_eventId);
        ev.Raise(RaiserId, message);
        OnEventRaised?.Invoke(ev.Id, message);
    }

    internal void InvalidationEvent_Raised(object sender, EPiServer.Events.EventNotificationEventArgs e)
    {
        if (
            // Don't process events locally raised
            e.RaiserId == RaiserId
            // Expect parameter as a string
            || e.Param is not string parameter
            // Only handle messages with our prefix
            || parameter?.StartsWith(_messageParameterPrefix) != true) return;

        HasReceivedAnyEvents = true;

        var value = parameter[_messageParameterPrefix.Length..];
        if (string.IsNullOrEmpty(value))
        {
            ClearAll(allowDistribute: false);
        }
        else
        {
            ClearKey(value, allowDistribute: false);
        }
    }
}
