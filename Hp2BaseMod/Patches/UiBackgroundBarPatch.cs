using HarmonyLib;

namespace Hp2BaseMod;

/// <summary>The location bar says "NON-STOP DATE" for a date without a pair, which is a puzzle state.</summary>
[HarmonyPatch(typeof(UiBackgroundBar))]
internal static class UiBackgroundBarStatePatch
{
    [HarmonyPatch(nameof(UiBackgroundBar.Refresh))]
    [HarmonyPrefix]
    private static bool Refresh(UiBackgroundBar __instance, LocationDefinition locationDef, int daytimeElapsed, GirlPairDefinition girlPairDef, bool sidesFlipped)
    {
        var locationText = "- " + locationDef.locationName.ToUpper() + " -";
        var daytimeText = ((ClockDaytimeType)(daytimeElapsed % 4)).ToString().ToUpper();
        var hasSpecialLabel = false;

        if (locationDef.specialLabels.Count > 0)
        {
            for (var i = 0; i < locationDef.specialLabels.Count; i++)
            {
                var label = locationDef.specialLabels[i];

                if (StringUtils.IsEmpty(label.flagName)
                    || (!label.inverse && Game.Persistence.playerFile.GetFlagValue(label.flagName) >= 0)
                    || (label.inverse && Game.Persistence.playerFile.GetFlagValue(label.flagName) < 0))
                {
                    daytimeText = label.labelText;
                    hasSpecialLabel = true;
                    break;
                }
            }
        }

        if (!hasSpecialLabel)
        {
            if (ModInterface.GameState.CurrentState is IPuzzleState && girlPairDef == null)
            {
                daytimeText = "NON-STOP DATE";
            }
            else if (girlPairDef != null && !girlPairDef.specialPair)
            {
                var nameOne = girlPairDef.girlDefinitionOne.specialCharacter || Game.Persistence.playerFile.GetPlayerFileGirl(girlPairDef.girlDefinitionOne).playerMet
                    ? girlPairDef.girlDefinitionOne.girlName.ToUpper()
                    : null;

                var nameTwo = girlPairDef.girlDefinitionTwo.specialCharacter || Game.Persistence.playerFile.GetPlayerFileGirl(girlPairDef.girlDefinitionTwo).playerMet
                    ? girlPairDef.girlDefinitionTwo.girlName.ToUpper()
                    : null;

                if (!StringUtils.IsEmpty(nameOne) && !StringUtils.IsEmpty(nameTwo))
                {
                    daytimeText += "  |  " + (!sidesFlipped ? nameOne + " & " + nameTwo : nameTwo + " & " + nameOne);
                }
            }
        }

        __instance.locationLabel.text = locationText;
        __instance.daytimeLabel.text = daytimeText;
        return false;
    }
}