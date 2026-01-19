using HarmonyLib;
using Hp2BaseMod;

namespace HuniePopUltimate;

[HarmonyPatch(typeof(UiCellphoneTrashZone))]
public static class UiCellphoneTrashZonePatch
{
    private static readonly RelativeId _goldFishPlushId = new RelativeId(-1, 45);

    [HarmonyPatch("OnDrop")]
    [HarmonyPrefix]
    public static void OnDrop(UiCellphoneTrashZone __instance, Draggable draggable)
    {
        var id = draggable.GetItemDefinition()?.ModId();
        if (id == _goldFishPlushId)
        {
            ModInterface.Log.Message("Threw out the gold fish!");
            Plugin.ThrewOutGoldfish = true;
        }
    }
}
