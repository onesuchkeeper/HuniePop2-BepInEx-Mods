using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;

[HarmonyPatch(typeof(UiWindowKyuButt))]
public static class UiWindowKyuButtPatch
{
    [HarmonyPatch(nameof(UiWindowKyuButt.Init))]
    [HarmonyPostfix]
    public static void Init(UiWindowKyuButt __instance)
        => ExpandedUiWindowKyuButt.Get(__instance).Init();

    [HarmonyPatch(nameof(UiWindowKyuButt.OnHidden))]
    [HarmonyPostfix]
    public static void OnHidden(UiWindowKyuButt __instance)
        => ExpandedUiWindowKyuButt.Get(__instance).OnHidden();
}

public class ExpandedUiWindowKyuButt
{
    private static Dictionary<UiWindowKyuButt, ExpandedUiWindowKyuButt> _expansions = new();

    public static ExpandedUiWindowKyuButt Get(UiWindowKyuButt core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiWindowKyuButt(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private FieldInfo f_middleDoll = AccessTools.Field(typeof(UiWindowKyuButt), "_middleDoll");
    private MethodInfo m_OnDialogLineStarted = AccessTools.Method(typeof(UiWindowKyuButt), "OnDialogLineStarted");

    private UiWindowKyuButt _core;

    public ExpandedUiWindowKyuButt(UiWindowKyuButt core)
    {
        _core = core;
    }

    public void Init()
    {
        var dollExp = ExpandedUiDoll.Get(f_middleDoll.GetValue<UiDoll>(_core));
        dollExp.DialogLineStartedEvent -= OnDialogLineStarted;
    }

    public void OnHidden()
    {
        var dollExp = ExpandedUiDoll.Get(f_middleDoll.GetValue<UiDoll>(_core));
        dollExp.DialogLineStartedEvent -= OnDialogLineStarted;
    }

    private void OnDialogLineStarted(UiDoll doll) => m_OnDialogLineStarted?.Invoke(_core, [doll]);
}
