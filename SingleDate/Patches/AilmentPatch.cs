using HarmonyLib;
using Hp2BaseMod;

namespace SingleDate;

[HarmonyPatch(typeof(Ailment))]
internal static class AilmentPatch
{
    private static readonly RelativeId[] _singleDateDisabledAilments = [
        new RelativeId(-1, 28),//Ashley - Commitment Issues - disabled because after 4 turns all matches turn negative
        new RelativeId(-1, 31)//Abia - Self Effacing - disables because doesn't allow gifts at all on single dates
    ];

    private static readonly RelativeId _commitmentIssuesId = new RelativeId(-1, 28);

    [HarmonyPatch(nameof(Ailment.Enable))]
    [HarmonyPrefix]
    public static bool Enable(Ailment __instance)
    {
        //prevent incompatible ailments from being enabled during single dates
        if (!State.IsSingleDate)
        {
            return true;
        }

        if (!Plugin.SingleDateBaggage)
        {
            return false;
        }

        foreach (var id in _singleDateDisabledAilments)
        {
            if (ModInterface.Data.TryGetRuntimeDataId(GameDataType.Ailment, id, out var runtimeId)
                && __instance.definition.id == runtimeId)
            {
                ModInterface.Log.LogInfo($"Preventing ailment {id} \"{__instance.definition.name}\" from being enabled during single date");
                return false;
            }
        }

        return true;
    }
}
