using System;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace SingleDate;

public class SensitivityExp : IExpInfo
{
    public static RelativeId Id => _id;
    internal static RelativeId _id;

    public static int ExpPerLvl => 6;

    private int Exp => Game.Persistence.playerFile.GetAffectionLevelExp(SensitivityExp.Id);

    public float Percentage => Exp / ((float)(Plugin.Instance.MaxSensitivityLevel * ExpPerLvl));

    public int CurrentLevel => 1 + State.GetSensitivityLevel();

    public int MaxLevel => Plugin.Instance.MaxSensitivityLevel + 1;

    public ItemDefinition ExpItemDef => ModInterface.GameData.GetItem(ItemSensitivitySmoothie.Exp);

    public ItemDefinition LevelPlateItemDef => ModInterface.GameData.GetItem(ItemSensitivitySmoothie.Level);

    public string ExpTitle => $"Sensitivity Level {CurrentLevel}";

    public string ExpDesc
    {
        get
        {
            var sensitivityLevel = State.GetSensitivityLevel();
            return sensitivityLevel == Plugin.Instance.MaxSensitivityLevel
                ? "Maximum Sensitivity Level!"
                : $"Misc • +{ExpPerLvl - (Exp % ExpPerLvl)} EXP until Level {sensitivityLevel + 2}";
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

    public bool GiveSmoothieItemTo(ItemDefinition itemDef, UiDoll doll, PuzzleStatusGirl puzzleStatusGirl)
    {
        var smoothieType = itemDef.Expansion().AffectionType;
        var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(doll.girlDefinition);
        var girlExpansion = doll.girlDefinition.Expansion();

        if (smoothieType == girlExpansion.LeastFavAffectionType)
        {
            Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Session.Gift.sfxGiftFailure, doll.pauseDefinition);
            doll.ReadDialogTrigger(Game.Session.Gift.dtSmoothieReject, DialogLineFormat.PASSIVE, -1);
            return false;
        }

        if (CurrentLevel == MaxLevel)
        {
            Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Session.Gift.sfxGiftFailure, doll.pauseDefinition);
            doll.ReadDialogTrigger(Game.Session.Gift.dtSmoothieFull, DialogLineFormat.PASSIVE, -1);
            return false;
        }

        var expGain = ModInterface.Data.GetDataId(GameDataType.Girl, doll.girlDefinition.id) == Girls.LillianId
            ? 3
            : 1;

        expGain = Math.Min(expGain, (MaxLevel * ExpPerLvl) - Exp);

        var lastLevel = CurrentLevel;
        Game.Persistence.playerFile.AddAffectionLevelExp(smoothieType, expGain);

        if (lastLevel != CurrentLevel)
        {
            doll.notificationBox.Show($"Level {CurrentLevel} achieved!");
        }

        Game.Persistence.playerFile.relationshipPoints++;
        playerFileGirl.relationshipPoints++;

        if (!itemDef.noStaminaCost)
        {
            puzzleStatusGirl.stamina--;
        }

        if (itemDef.energyDefinition != null)
        {
            GameObject.Instantiate(Game.Session.Gift.energyTrailPrefab)
                .Init(EnergyTrailFormat.START_AND_END, itemDef.energyDefinition, Game.Manager.gameCamera.GetMousePosition(), doll, $"+{expGain} Sensitivity EXP", null);
        }

        doll.ReadDialogTrigger(Game.Session.Gift.dtSmoothieAccept, DialogLineFormat.PASSIVE, -1);
        return true;
    }
}
