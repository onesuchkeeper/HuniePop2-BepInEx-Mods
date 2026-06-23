using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;

namespace HuniePopUltimate;

public class MomentumAilment : IScriptedAilment
{
    public static void AddMods()
    {
        var mod = new AilmentDataMod(Items.Audrey.Baggage1, InsertStyle.append)
        {
            ItemDefinitionID = Items.Audrey.Baggage1,
            ScriptedAilmentFactory = (Ailment) => new MomentumAilment(Ailment),
        };
        ModInterface.AddDataMod(mod);
    }

    private const string StreakKey = "momentum_streak";

    // The ailment instance this behaviour belongs to.
    // Captured at construction so we don't need to receive it on every call.
    private readonly Ailment _ailment;

    public MomentumAilment(Ailment ailment)
    {
        _ailment = ailment;
    }

    public void OnEnable(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl)
    {
        // Reset the streak when the ailment becomes active.
        _ailment.flags[StreakKey] = 0;
    }

    public void OnDisable(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl)
    {
        // Clear streak on disable so a re-enable starts fresh.
        // (Only relevant if persistentFlags is true on the definition.)
        if (_ailment.flags.ContainsKey(StreakKey))
        {
            _ailment.flags[StreakKey] = 0;
        }
    }

    public bool OnTrigger(
        AilmentTriggerType triggerType,
        Ailment ailment,
        PuzzleStatusGirl girl,
        bool unfocused,
        MoveModifier moveModifier,
        MatchModifier matchModifier,
        GiftModifier giftModifier)
    {
        switch (triggerType)
        {
            case AilmentTriggerType.PRE_MOVE:
            {
                if (unfocused) return false;

                if (!_ailment.flags.ContainsKey(StreakKey))
                {
                    _ailment.flags[StreakKey] = 0;
                }

                _ailment.flags[StreakKey]++;

                int streak = _ailment.flags[StreakKey];

                // Drain stamina equal to the current streak depth.
                // Clamped so a very long streak cannot one-shot a girl with max stamina.
                int drain = UnityEngine.Mathf.Clamp(streak, 1, girl.stamina - 1);
                girl.stamina -= drain;

                return streak > 1; // Verbalize only once the streak has actually escalated.
            }

            case AilmentTriggerType.ON_FOCUS:
            {
                ModInterface.Log.Message("Momentum Focus Switch!");

                // A focus switch means this girl is no longer on a streak, reset.
                if (_ailment.flags.ContainsKey(StreakKey))
                {
                    _ailment.flags[StreakKey] = 0;
                }

                return false;
            }

            default:
                return false;
        }
    }
}