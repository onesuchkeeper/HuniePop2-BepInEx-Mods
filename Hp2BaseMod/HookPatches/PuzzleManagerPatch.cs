using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(PuzzleManager))]
public static class PuzzleMangerPatch
{
    private static MethodInfo _notifyPreRoundOverCutscene = AccessTools.Method(typeof(PuzzleMangerPatch), nameof(Act));

    public static void Act()
    {
        ModInterface.Events.NotifyPreRoundOverCutscene();
    }

    [HarmonyPatch("OnRoundOver")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> OnRoundOver(IEnumerable<CodeInstruction> instructions)
    {
        int step = 0;
        foreach (var instruction in instructions)
        {
            switch (step)
            {
                case 0:
                    step = instruction.opcode == OpCodes.Ldarg_0 ? 1 : 0;
                    break;
                case 1:
                    step = instruction.opcode == OpCodes.Ldfld ? 2 : 0;
                    break;
                case 2:
                    step = instruction.opcode == OpCodes.Ldloc_0 ? 3 : 0;
                    break;
                case 3:
                    step = instruction.opcode == OpCodes.Callvirt && instruction.operand?.ToString() == "Void set_gameOver(Boolean)" ? 4 : 0;
                    break;
                case 4:
                    step = -1;
                    yield return new CodeInstruction(OpCodes.Call, _notifyPreRoundOverCutscene);
                    break;
            }

            yield return instruction;
        }

        if (step != -1)
        {
            ModInterface.Log.LogError("Failed to transply hook into PuzzleManager.OnRoundOver");
        }
    }

    // [HarmonyPatch(nameof(PuzzleManager.StartPuzzle))]
    // [HarmonyPrefix]
    // public static void StartPuzzle()
    // {

    // }
    //I think this may take place in the locations logic bundles...
    //which is real stupid...
    //I could probably remove the logic bundles entierly?
    //why would you make a logic bundle and not just interface and explicitly code your behavior
    //lets make a logic bundle logging util and see what all them dos


    //when line index is -1 and on a date, it is selected by the index of the current location
    //instead we can just look up the line index in the expanded DT?
    //Game.Session.Puzzle.StartPuzzle();
    //the base just calls start puzzle, we have to handle this there

    // if (lineIndex < 0)
    // {
    //     DialogTriggerForceType forceType = dialogTriggerDef.forceType;
    //     if (forceType == DialogTriggerForceType.DATE_LOCATION && Game.Session.Location.AtLocationType(LocationType.DATE) && Game.Session.Location.dateLocationDefs.Contains(Game.Session.Location.currentLocation))
    //     {
    //         lineIndex = Game.Session.Location.dateLocationDefs.IndexOf(Game.Session.Location.currentLocation);
    //     }
    // }
    //uiDoll.ReadDialogTrigger(__instance.dtGreetings[greetingIndex], DialogLineFormat.PASSIVE, -1);
}