// Hp2BaseMod 2026, By OneSuchKeeper

using HarmonyLib;

namespace Hp2BaseMod;

/// <summary>
/// Simple hook patch to notify when the game session begins
/// </summary>
[HarmonyPatch(typeof(GameSession))]
internal static class GameSessionPatch
{
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    private static void Awake_Postfix(GameSession __instance) => ModInterface.Events.NotifyGameSessionBegan();
}
