using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Hp2BaseMod;

[HarmonyPatch(typeof(UiDoll))]
public static class UiDollPatch
{
    [HarmonyPatch("PurifyDialogText")]
    [HarmonyPostfix]
    public static void PurifyDialogText(UiDoll __instance, string text, ref string __result)
        => ExpandedUiDoll.Get(__instance).PurifyDialogText(ref __result);

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiDoll __instance)
        => ExpandedUiDoll.Get(__instance).OnDestroy();
}

public class ExpandedUiDoll
{
    private static Dictionary<UiDoll, ExpandedUiDoll> _expansions
            = new Dictionary<UiDoll, ExpandedUiDoll>();

    public static ExpandedUiDoll Get(UiDoll core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiDoll(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    protected UiDoll _core;
    private ExpandedUiDoll(UiDoll core)
    {
        _core = core;
    }

    public void PurifyDialogText(ref string __result)
    {
        if (string.IsNullOrWhiteSpace(__result)) { return; }

        var cleaned = new StringBuilder();
        var write = true;
        var bracketDepth = 0;
        foreach (var character in __result)
        {
            if (bracketDepth > 0)
            {
                cleaned.Append(character);

                if (character == '>')
                {
                    bracketDepth--;
                }
            }
            else if (character == '<')
            {
                cleaned.Append(character);
                bracketDepth++;
            }
            else if (character == '*')
            {
                write = !write;
            }
            else if (write)
            {
                cleaned.Append(character);
            }
        }

        if (!write)
        {
            ModInterface.Log.Warning($"Encountered text with unpaired asterisk: {__result}");
            return;
        }
        else if (bracketDepth != 0)
        {
            ModInterface.Log.Warning($"Encountered unpaired angle brackets: {__result}");
            return;
        }
        else
        {
            __result = cleaned.ToString();
        }
    }

    public void OnDestroy() => _expansions.Remove(_core);
}
