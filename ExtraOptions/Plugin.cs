using System;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2ExtraOptions;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        var modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);

        Constants.FemaleJizzToggleCodeID = new RelativeId(modId, 0);
        Constants.SlowAffectionDrainToggleCodeID = new RelativeId(modId, 1);
        Constants.HubStyleChangeRateUpCodeId = new RelativeId(modId, 2);
        Constants.UnpairStylesCodeId = new RelativeId(modId, 3);
        Constants.RandomStylesCodeId = new RelativeId(modId, 4);
        Constants.RunInBackgroundCodeId = new RelativeId(modId, 5);
        Constants.FairyWingsCodeId = new RelativeId(modId, 6);

        ModInterface.AddDataMod(new CodeDataMod(Constants.FemaleJizzToggleCodeID, InsertStyle.replace)
        {
            CodeHash = MD5Utility.Encrypt("JIZZ FOR ALL"),
            CodeType = CodeType.TOGGLE,
            OnMessage = "Female 'wet' photos enabled.",
            OffMessage = "Female 'wet' photos disabled."
        });
        ModInterface.AddDataMod(new CodeDataMod(Constants.HubStyleChangeRateUpCodeId, InsertStyle.replace)
        {
            CodeHash = MD5Utility.Encrypt("HOT N COLD"),
            CodeType = CodeType.TOGGLE,
            OnMessage = "Hub girl style change rate up.",
            OffMessage = "Hub girl style change rate normal."
        });
        ModInterface.AddDataMod(new CodeDataMod(Constants.RandomStylesCodeId, InsertStyle.replace)
        {
            CodeHash = MD5Utility.Encrypt("YES N NO"),
            CodeType = CodeType.TOGGLE,
            OnMessage = "Girl sim styles randomization on.",
            OffMessage = "Girl sim styles randomization off."
        });
        ModInterface.AddDataMod(new CodeDataMod(Constants.RunInBackgroundCodeId, InsertStyle.replace)
        {
            CodeHash = MD5Utility.Encrypt("STAY FOCUSED"),
            CodeType = CodeType.TOGGLE,
            OnMessage = "The game will continue running while unfocused.",
            OffMessage = "The game will pause when unfocused."
        });
        ModInterface.AddDataMod(new CodeDataMod(Constants.UnpairStylesCodeId, InsertStyle.replace)
        {
            CodeHash = MD5Utility.Encrypt("ZOEY APPROVED"),
            CodeType = CodeType.TOGGLE,
            OnMessage = "Random girl styles unpaired.",
            OffMessage = "Random girl styles paired."
        });
        ModInterface.AddDataMod(new CodeDataMod(Constants.FairyWingsCodeId, InsertStyle.replace)
        {
            CodeHash = MD5Utility.Encrypt("PINK BITCH!"),
            CodeType = CodeType.TOGGLE,
            OnMessage = "Awh yeah! She's unstoppable! [The game must be restarted in order to take effect]",
            OffMessage = "Lack of hunies rivets us firmly to the ground, ones wings are clipped."
        });
        ModInterface.AddCommand(new SetIconCommand());

        ModInterface.Events.RequestStyleChange += RandomizeStyles.On_RequestStyleChange;
        ModInterface.Events.PostDataMods += On_PostDataMods;
        ModInterface.Events.PostCodeSubmitted += On_PostCodeSubmitted;

        // add toggle for slow drain on bonus round? TODO

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    private void On_PostCodeSubmitted()
    {
        Application.runInBackground = ModInterface.GameData.IsCodeUnlocked(Constants.RunInBackgroundCodeId);
    }

    private void On_PostDataMods()
    {
        Application.runInBackground = ModInterface.GameData.IsCodeUnlocked(Constants.RunInBackgroundCodeId);

        if (!ModInterface.GameData.IsCodeUnlocked(Constants.FairyWingsCodeId)) { return; }

        var kyu = ModInterface.GameData.GetGirl(Girls.KyuId);

        ModInterface.Log.LogInfo("Applying wings");
        if (kyu == null)
        {
            ModInterface.Log.LogWarning("Unable to find Kyu, \"PINK BITCH!\" wings not applied D:");
            return;
        }

        foreach (var girl in Game.Data.Girls.GetAll())
        {
            girl.specialEffectPrefab = kyu.specialEffectPrefab;
            girl.specialEffectOffset = kyu.specialEffectOffset;
        }
    }
}