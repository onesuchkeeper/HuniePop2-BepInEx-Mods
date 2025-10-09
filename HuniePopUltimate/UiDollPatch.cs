using System.Text;
using HarmonyLib;
using Hp2BaseMod;

[HarmonyPatch(typeof(UiDoll))]
public static class UiDollPatch
{
    [HarmonyPatch("PurifyDialogText")]
    [HarmonyPostfix]
    public static void PurifyDialogText(UiDoll __instance, string text, ref string __result)
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
            ModInterface.Log.LogWarning($"Encountered text with unpaired asterisk: {__result}");
            return;
        }
        else if (bracketDepth != 0)
        {
            ModInterface.Log.LogWarning($"Encountered unpaired angle brackets: {__result}");
            return;
        }
        else
        {
            __result = cleaned.ToString();
        }
    }
}