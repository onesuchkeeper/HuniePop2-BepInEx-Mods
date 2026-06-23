using System.Collections.Generic;
using System.Collections.Specialized;
using Hp2BaseMod;

namespace HuniePopUltimate;

/// <summary>
/// Debugging helpers for inspecting raw HP1 serialized data dictionaries.
/// All methods are static — there is no instance state.
/// </summary>
public static class HpDebugLog
{
    public static void AudioMessage(string msg)  => ModInterface.Log.Message($"[AudioCache] {msg}");
    public static void AudioWarning(string msg)  => ModInterface.Log.Warning($"[AudioCache] {msg}");

    public static void SpriteMessage(string msg) => ModInterface.Log.Message($"[Sprite] {msg}");
    public static void SpriteWarning(string msg) => ModInterface.Log.Warning($"[Sprite] {msg}");

    public static void GirlMessage(string msg)   => ModInterface.Log.Message($"[Girl] {msg}");
    public static void GirlWarning(string msg)   => ModInterface.Log.Warning($"[Girl] {msg}");
    public static void GirlError(string msg)     => ModInterface.Log.Error($"[Girl] {msg}");

    public static void DialogWarning(string msg) => ModInterface.Log.Warning($"[Dialog] {msg}");
    public static void DialogError(string msg)   => ModInterface.Log.Error($"[Dialog] {msg}");

    public static void LocationMessage(string msg) => ModInterface.Log.Message($"[Location] {msg}");

    /// <summary>
    /// Recursively logs every key/value pair in <paramref name="dict"/>,
    /// indenting nested structures.
    /// </summary>
    public static void LogAll(OrderedDictionary dict)
    {
        var keys = dict.Keys.GetEnumerator();
        var vals = dict.Values.GetEnumerator();

        while (keys.MoveNext() && vals.MoveNext())
        {
            if (vals.Current is OrderedDictionary subDict)
            {
                using (ModInterface.Log.MakeIndent(keys.Current.ToString())) LogAll(subDict);
            }
            else if (vals.Current is List<object> list)
            {
                using (ModInterface.Log.MakeIndent(keys.Current.ToString()))
                {
                    int i = 0;
                    foreach (var entry in list)
                    {
                        using (ModInterface.Log.MakeIndent($"{i++}"))
                        {
                            if (entry is OrderedDictionary od)
                            {
                                LogAll(od);
                            } 
                            else
                            {
                                ModInterface.Log.Message(entry?.ToString() ?? "null");
                            }
                                
                        }
                    }
                }
            }
            else
            {
                ModInterface.Log.Message($"{keys.Current} : {vals.Current?.ToString() ?? "null"}");
            }
        }
    }
}