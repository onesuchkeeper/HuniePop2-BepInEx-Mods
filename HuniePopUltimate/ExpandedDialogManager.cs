using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

[HarmonyPatch(typeof(DialogManager))]
public static class DialogManagerPatch
{
    [HarmonyPatch(nameof(DialogManager.AddDoll))]
    [HarmonyPostfix]
    public static void AddDoll(DialogManager __instance, UiDoll doll)
        => ExpandedDialogManager.Get(__instance).AddDoll(doll);

    [HarmonyPatch(nameof(DialogManager.RemoveDoll))]
    [HarmonyPostfix]
    public static void RemoveDoll(DialogManager __instance, UiDoll doll)
        => ExpandedDialogManager.Get(__instance).RemoveDoll(doll);
}

public class ExpandedDialogManager
{
    private static Dictionary<DialogManager, ExpandedDialogManager> _expansions = new();

    public static ExpandedDialogManager Get(DialogManager core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedDialogManager(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private MethodInfo m_OnDialogLineStarted = AccessTools.Method(typeof(DialogManager), "OnDialogLineStarted");

    private DialogManager _core;

    public ExpandedDialogManager(DialogManager core)
    {
        _core = core;
    }

    public void AddDoll(UiDoll doll)
    {
        var dollExp = ExpandedUiDoll.Get(doll);
        dollExp.DialogLineStartedEvent -= OnDialogLineStarted;
    }

    public void RemoveDoll(UiDoll doll)
    {
        var dollExp = ExpandedUiDoll.Get(doll);
        dollExp.DialogLineStartedEvent -= OnDialogLineStarted;
    }

    private void OnDialogLineStarted(UiDoll doll) => m_OnDialogLineStarted?.Invoke(_core, [doll]);
}
