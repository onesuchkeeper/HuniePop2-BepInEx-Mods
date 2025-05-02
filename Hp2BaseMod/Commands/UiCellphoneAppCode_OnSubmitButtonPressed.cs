using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine.UI;

namespace Hp2BaseMod.Commands;

[HarmonyPatch(typeof(UiCellphoneAppCode), "OnSubmitButtonPressed")]
public static class UiCellphoneAppCode_OnSubmitButtonPressed
{
    private static MethodInfo ShowCodeResult = AccessTools.Method(typeof(UiCellphoneAppCode), "ShowCodeResult");
    private static FieldInfo _currentFieldText = AccessTools.Field(typeof(UiCellphoneAppCode), "_currentFieldText");
    private static FieldInfo _inputField = AccessTools.Field(typeof(UiCellphoneAppCode), "inputField");

    public static bool Prefix(UiCellphoneAppCode __instance)
    {
        if (!(_inputField.GetValue(__instance) is InputField inputField))
        {
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

            ShowCodeResult.Invoke(__instance, [result, !success]);

            return false;
        }

        return true;
    }

    public static void Postfix()
    {
        ModInterface.Events.NotifyPostCodeSubmitted();
    }
}