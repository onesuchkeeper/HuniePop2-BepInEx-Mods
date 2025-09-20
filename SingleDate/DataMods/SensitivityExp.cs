using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace SingleDate;

internal class SensitivityExp : IExpInfo
{
    public float Percentage => State.SensitivityPercentage;

    public int CurrentLevel => State.GetSensitivityLevel() + 1;

    public int MaxLevel => Plugin.Instance.MaxSensitivityLevel + 1;

    public ItemDefinition ExpItemDef => ModInterface.GameData.GetItem(ItemSensitivitySmoothie.Exp);

    public ItemDefinition LevelPlateItemDef => ModInterface.GameData.GetItem(ItemSensitivitySmoothie.Level);

    public string ExpTitle => $"Sensitivity Level {State.GetSensitivityLevel() + 1}";

    public string ExpDesc
    {
        get
        {
            var sensitivityLevel = State.GetSensitivityLevel();
            return sensitivityLevel == Plugin.Instance.MaxSensitivityLevel
                ? "Maximum Sensitivity Level!"
                : $"Misc • +{6 - (State.SensitivityExp % 6)} EXP until Level {sensitivityLevel + 2}";
        }
    }

    public string PlateTitle => "SENSTIV. LVL";

    public string PlateDesc => $"On single dates, each <c=#5E2782FF><q=hp_token_broken>Broken Heart</c> token matched will remove <c=#5E2782FF>{State.GetBrokenMult() * 100}%</c> Affection.";

    public Sprite IconImage => UiPrefabs.SensitivityIcon;

    public Sprite MeterFront => UiPrefabs.SensitivityMeter;
    public Sprite PlateImage => UiPrefabs.SensitivityPlate;

    public RelativeId AffectionType => RelativeId.Default;

    public Color32 OutlineColor => new Color32(80, 57, 157, 255);
    public Color32 TextColor => new Color32(221, 217, 255, 255);

    public ItemDefinition SmoothieItemDef => ModInterface.GameData.GetItem(ItemSensitivitySmoothie.SmoothieId);
}
