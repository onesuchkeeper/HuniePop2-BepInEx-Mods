using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(PuzzleManager), "OnRoundOver")]
public static class PuzzleMangerPatch
{
    private static MethodInfo _notifyPreRoundOverCutscene = AccessTools.Method(typeof(PuzzleMangerPatch), nameof(Act));

    public static void Act()
    {
        ModInterface.Events.NotifyPreRoundOverCutscene();
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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
}