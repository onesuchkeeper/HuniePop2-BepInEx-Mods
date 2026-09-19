using HarmonyLib;

namespace Hp2BaseMod
{
    [HarmonyPatch(typeof(GameSession))]
    internal static class GameSessionPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void Awake_Postfix(GameSession __instance) => ModInterface.Events.NotifyGameSessionBegan();
    }
}
