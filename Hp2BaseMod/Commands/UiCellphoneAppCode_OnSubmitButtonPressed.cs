// Hp2BaseMod 2025, By OneSuchKeeper

using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine.UI;

namespace Hp2BaseMod.Commands;

/// <summary>
/// Handles codes starting with '/' as commands. See <see cref="ICommand"/>
/// </summary>
[HarmonyPatch(typeof(UiCellphoneAppCode), "OnSubmitButtonPressed")]
public static class UiCellphoneAppCode_OnSubmitButtonPressed
{
    private static readonly MethodInfo m_showCodeResult = AccessTools.Method(typeof(UiCellphoneAppCode), "ShowCodeResult");
    private static readonly FieldInfo f_currentFieldText = AccessTools.Field(typeof(UiCellphoneAppCode), "_currentFieldText");
    private static readonly FieldInfo f_inputField = AccessTools.Field(typeof(UiCellphoneAppCode), "inputField");

    public static bool Prefix(UiCellphoneAppCode __instance, out string __state)
    {
        if (!(f_inputField.GetValue(__instance) is InputField inputField))
        {
            __state = null;
            return true;
        }

        var input = inputField.text?.ToUpper().Trim();

        if (input != null && input.StartsWith("/"))
        {
            inputField.text = "";
            f_currentFieldText.SetValue(__instance, "");

            ModInterface.Log.Message($"Submitted code: \"{input}\" handled as command input");
            var inputSplit = input.Substring(1, input.Length - 1).Split(' ');

            var success = ModInterface.TryExecuteCommand(inputSplit.First(), inputSplit.Skip(1).ToArray(), out var result);

            ModInterface.Log.Message($"Command {(success ? "succeeded" : "failed")}: {result}");

            m_showCodeResult.Invoke(__instance, [result, !success]);

            __state = null;
            return false;
        }

        __state = input;
        return true;
    }

    public static void Postfix(UiCellphoneAppCode __instance, string __state)
    {
        if (__state == null)
        {
            return;
        }

        Game.Manager.Settings.unlockCodes.TryGetValue(StringUtils.MD5(__state), out var codeDefinition);

        ModInterface.Events.NotifyPostCodeSubmitted(codeDefinition);
    }
}
