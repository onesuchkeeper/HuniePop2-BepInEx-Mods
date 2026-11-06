using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace SingleDate;

/// <summary>
/// Describes sensitivity to the base mod's EXP display (the meter in the cellphone's pair app). Levels are shown
/// starting from 1, so they are one more than <see cref="State.GetSensitivityLevel"/>.
/// </summary>
internal class SensitivityExp : IExpInfo
{
    /// <summary>Meter fill, from 0 to 1.</summary>
    public float Percentage => State.SensitivityPercentage;

    public int CurrentLevel => State.GetSensitivityLevel() + 1;

    public int MaxLevel => Plugin.MaxSensitivityLevel.Value + 1;

    public ItemDefinition ExpItemDef => ModInterface.GameData.GetItem(ItemSensitivitySmoothie.Exp);

    public ItemDefinition LevelPlateItemDef => ModInterface.GameData.GetItem(ItemSensitivitySmoothie.Level);

    public string ExpTitle => $"Sensitivity Level {State.GetSensitivityLevel() + 1}";

    /// <summary>Text for the EXP tooltip, how many EXP until the next level or that sensitivity is maxed.</summary>
    public string ExpDesc
    {
        get
        {
            var sensitivityLevel = State.GetSensitivityLevel();
            return sensitivityLevel == Plugin.MaxSensitivityLevel.Value
                ? "Maximum Sensitivity Level!"
                : $"Misc • +{State.ExpPerLevel - (State.SensitivityExp % State.ExpPerLevel)} EXP until Level {sensitivityLevel + 2}";
        }
    }

    /// <summary>Title shown on the sensitivity plate.</summary>
    public string PlateTitle => "SENSTIV. LVL";

    /// <summary>Plate description, the affection removed per broken heart at the current level.</summary>
    public string PlateDesc => $"On single dates, each <c=#5E2782FF><q=hp_token_broken>Broken Heart</c> token matched will remove <c=#5E2782FF>{(State.GetBrokenMult() * 100).ToString("0.00")}%</c> Affection.";

    public Sprite IconImage => UiPrefabs.SensitivityIcon;

    public Sprite MeterFront => UiPrefabs.SensitivityMeter;
    public Sprite PlateImage => UiPrefabs.SensitivityPlate;

    public RelativeId AffectionType => RelativeId.Default;

    // Purple, matching the broken heart token.
    public Color32 OutlineColor => new Color32(80, 57, 157, 255);
    public Color32 TextColor => new Color32(221, 217, 255, 255);

    public ItemDefinition SmoothieItemDef => ModInterface.GameData.GetItem(ItemSensitivitySmoothie.SmoothieId);
}
