// Hp2BaseMod 2026, By OneSuchKeeper

using System;
using System.Reflection;
using HarmonyLib;

namespace Hp2BaseMod.Utility;

/// <summary>Helpers for controlling subscriptions via reflection.</summary>
internal static class EventUtil
{
    private const BindingFlags All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary><c>publisher.eventName += subscriber.handlerName</c> for events and handlers that are not accessible.</summary>
    public static void Subscribe(object publisher, string eventName, object subscriber, string handlerName)
    {
        var ev = publisher.GetType().GetEvent(eventName, All);
        ev.AddEventHandler(publisher, Delegate.CreateDelegate(ev.EventHandlerType, subscriber, AccessTools.Method(subscriber.GetType(), handlerName)));
    }

    /// <summary><c>publisher.eventName -= subscriber.handlerName</c></summary>
    public static void Unsubscribe(object publisher, string eventName, object subscriber, string handlerName)
    {
        var ev = publisher.GetType().GetEvent(eventName, All);
        ev.RemoveEventHandler(publisher, Delegate.CreateDelegate(ev.EventHandlerType, subscriber, AccessTools.Method(subscriber.GetType(), handlerName)));
    }

    /// <summary>Raises an event of an object from outside of it.</summary>
    public static void Raise(object publisher, string eventName, params object[] args)
        => (AccessTools.Field(publisher.GetType(), eventName)?.GetValue(publisher) as Delegate)?.DynamicInvoke(args);
}
