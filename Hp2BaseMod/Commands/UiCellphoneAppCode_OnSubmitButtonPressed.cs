using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine.UI;

namespace Hp2BaseMod.Commands;

[HarmonyPatch(typeof(UiCellphoneAppCode), "OnSubmitButtonPressed")]
public static class UiCellphoneAppCode_OnSubmitButtonPressed
{
    private static MethodInfo m_showCodeResult = AccessTools.Method(typeof(UiCellphoneAppCode), "ShowCodeResult");
    private static FieldInfo _currentFieldText = AccessTools.Field(typeof(UiCellphoneAppCode), "_currentFieldText");
    private static FieldInfo _inputField = AccessTools.Field(typeof(UiCellphoneAppCode), "inputField");

    public static bool Prefix(UiCellphoneAppCode __instance, out string __state)
    {
        if (!(_inputField.GetValue(__instance) is InputField inputField))
        {
            __state = null;
            return true;
        }

        var input = inputField.text?.ToUpper().Trim();

        if (input != null && input.StartsWith("/"))
        {
            inputField.text = "";
            _currentFieldText.SetValue(__instance, "");

            ModInterface.Log.LogInfo($"Submitted code: \"{input}\" handled as command input");
            var inputSplit = input.Substring(1, input.Length - 1).Split(' ');

            var success = ModInterface.TryExecuteCommand(inputSplit.First(), inputSplit.Skip(1).ToArray(), out var result);

            ModInterface.Log.LogInfo($"Command {(success ? "succeeded" : "failed")}: {result}");

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
