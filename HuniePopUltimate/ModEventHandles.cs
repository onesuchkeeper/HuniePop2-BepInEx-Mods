using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace HuniePopUltimate;

public static class ModEventHandles
{
    internal static void On_PreDataMods()
    {
        var nudeOutfitPart = Chainloader.PluginInfos.ContainsKey(Plugin.REPEAT_THREESOME_GUID)
            ? new GirlPartDataMod(new RelativeId(ModInterface.GetSourceId(Plugin.REPEAT_THREESOME_GUID), 0), InsertStyle.replace)
            {
                PartType = GirlPartType.OUTFIT,
                PartName = "nudeOutfit",
                X = 0,
                Y = 0,
                SpriteInfo = new SpriteInfoInternal("EmptySprite")
            }
            : null;

        var singleDateId = ModInterface.GetSourceId(Plugin.SINGLE_DATE_GUID);

        if (!(ModInterface.TryGetInterModValue(singleDateId, "AddGirlDatePhotos", out Action<RelativeId, IEnumerable<(RelativeId, float)>> m_AddGirlDatePhotos)
                && ModInterface.TryGetInterModValue(singleDateId, "AddGirlSexPhotos", out Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> m_AddGirlSexPhotos)
                && ModInterface.TryGetInterModValue(singleDateId, "SetGirlCharm", out Action<RelativeId, Sprite> m_SetCharmSprite)))
        {
            m_AddGirlDatePhotos = null;
            m_AddGirlSexPhotos = null;
            m_SetCharmSprite = null;
        }

        ModInterface.Log.Message("Loading HuniePop assembly (this may take a bit)");

        HpExtraction hpExtraction = null;
        try
        {
            hpExtraction = new HpExtraction(Plugin.HuniePopDir.Value, m_AddGirlDatePhotos, m_AddGirlSexPhotos, m_SetCharmSprite, nudeOutfitPart);
            using (ModInterface.Log.MakeIndent("HuniePop assembly loaded successfully, beginning import:"))
            {
                hpExtraction.Extract();
            }
        }
        catch (Exception e)
        {
            ModInterface.Log.Error(e.Message + Environment.NewLine + e.StackTrace);
            return;
        }

        if (ModInterface.TryGetInterModValue(singleDateId, "AddSexLocationBlackList", out Action<RelativeId, IEnumerable<RelativeId>> m_AddSexLocationBlackList)
            && ModInterface.TryGetInterModValue(singleDateId, "SetCutsceneSuccessAttracted", out Action<RelativeId, RelativeId> m_SetCutsceneSuccessAttracted)
            && ModInterface.TryGetInterModValue(singleDateId, "SetBonusRoundSuccessCutscene", out Action<RelativeId, RelativeId> m_SetBonusRoundSuccessCutscene)
            && ModInterface.TryGetInterModValue(singleDateId, "MakeSexPhotoCutsceneStep", out Func<IGameDefinitionInfo<CutsceneStepSubDefinition>> m_MakeSexPhotoCutsceneStep))
        {
            Plugin.HasSingleDate = true;

            PreSexCutscene.AddDataMods();
            PostSexCutscene.AddDataMods();
            SuccessAttractedCutscene.AddDataMods();
            BonusRoundSuccessCutscene.AddDataMods(m_MakeSexPhotoCutsceneStep.Invoke());

            var defaultPhoto = new RelativeId(singleDateId, 0);
            var singleDateNobodyId = new RelativeId(singleDateId, 0);

            void AddPairMod(ClockDaytimeType sexTime, RelativeId girlId, RelativeId pairId)
            {
                var meetingLoc = new RelativeId(-1, 1 + (girlId.LocalId % 8));
                var introCutsceneId = new RelativeId(singleDateId, 0);
                if (hpExtraction.SingleDatePairData.TryGetValue(girlId, out var pairData))
                {
                    meetingLoc = pairData.MeetingLocation;
                    introCutsceneId = pairData.MeetingCutscene.Id;
                    ModInterface.AddDataMod(pairData.MeetingCutscene);
                }
                else
                {
                    ModInterface.Log.Error($"failed to find intro for {girlId}");
                }

                ModInterface.AddDataMod(new GirlPairDataMod(pairId, InsertStyle.assignNull, 1)
                {
                    GirlDefinitionOneID = singleDateNobodyId,
                    GirlDefinitionTwoID = girlId,
                    SpecialPair = false,
                    PhotoDefinitionID = defaultPhoto,
                    IntroductionPair = false,
                    IntroSidesFlipped = false,
                    HasMeetingStyleOne = false,
                    HasMeetingStyleTwo = false,
                    MeetingLocationDefinitionID = meetingLoc,
                    SexDayTime = sexTime,
                    SexLocationDefinitionID = null,

                    BonusSuccessCutsceneDefinitionID = Cutscenes.BonusRoundSuccess,
                    AttractSuccessCutsceneDefinitionID = Cutscenes.SuccessAttracted,
                    SuccessCutsceneDefinitionID = new RelativeId(singleDateId, 4),

                    IntroRelationshipCutsceneDefinitionID = introCutsceneId,
                    AttractRelationshipCutsceneDefinitionID = new RelativeId(singleDateId, 1),
                    PreSexRelationshipCutsceneDefinitionID = Cutscenes.PreSex,
                    PostSexRelationshipCutsceneDefinitionID = Cutscenes.PostSex,
                });

                m_AddSexLocationBlackList(girlId, Plugin.SexLocs);
                m_SetCutsceneSuccessAttracted(girlId, Cutscenes.SuccessAttracted);
                m_SetBonusRoundSuccessCutscene(girlId, Cutscenes.BonusRoundSuccess);
            }

            foreach (var girl in new RelativeId[]
            {
                Girls.Tiffany,
                Girls.Aiko,
                Girls.Kyanna,
                Girls.Audrey,
                Girls.Nikki,
                Girls.Beli,
                Girls.Celeste,
                Girls.Venus
            })
            {
                AddPairMod(ClockDaytimeType.NIGHT, girl, girl);
            }

            AddPairMod(ClockDaytimeType.EVENING, Girls.Momo, Girls.Momo);

            Pairs._kyuSingleDate = new RelativeId(Plugin.ModId, Hp2BaseMod.Girls.Kyu.LocalId);
            Pairs._jessieSingleDate = new RelativeId(singleDateId, Hp2BaseMod.Girls.Jessie.LocalId);
            Pairs._lolaSingleDate = new RelativeId(singleDateId, Hp2BaseMod.Girls.Lola.LocalId);

            AddPairMod(ClockDaytimeType.NIGHT, Hp2BaseMod.Girls.Lola, Pairs.LolaSingleDate);
            AddPairMod(ClockDaytimeType.NIGHT, Hp2BaseMod.Girls.Jessie, Pairs.JessieSingleDate);
            AddPairMod(ClockDaytimeType.NIGHT, Hp2BaseMod.Girls.Kyu, Pairs.KyuSingleDate);
        }
    }

    /// <summary>
    /// Adds "Weird Thing" for Celeste to shop
    /// </summary>
    internal static void On_PopulateStoreProducts(StoreProductsPopulateArgs args)
    {
        if (UnityEngine.Random.Range(0, 5) > 1) return;

        var category = new Hp2BaseMod.Elements.Category<ItemDefinition>()
        {
            Priority = -1,
            TargetCount = 1,
            Pool = new()
        };

        var playerFile = Game.Persistence.playerFile;

        // using goldfish plush instead
        // var momoDef = ModInterface.GameData.GetGirl(Girls.Momo);
        // var momoSave = playerFile.GetPlayerFileGirl(momoDef);
        // if (!momoSave.playerMet)
        // {
        //     category.Pool.Add(new Hp2BaseMod.Elements.Category<ItemDefinition>.Entry(ModInterface.GameData.GetItem(Items.Goldfish), 1));
        // }

        var celesteDef = ModInterface.GameData.GetGirl(Girls.Celeste);
        var celesteSave = playerFile.GetPlayerFileGirl(celesteDef);
        if (!celesteSave.playerMet
            && !playerFile.IsItemInInventory(ModInterface.GameData.GetItem(Items.WeirdThing), false))
        {
            category.Pool.Add(new Hp2BaseMod.Elements.Category<ItemDefinition>.Entry(ModInterface.GameData.GetItem(Items.WeirdThing), 1));
        }

        args.ItemCategories[new RelativeId(Plugin.ModId, 0)] = category;
    }

    internal static void On_FinderSlotsPopulate(FinderSlotPopulateEventArgs args)
    {
        var time = (ClockDaytimeType)(Game.Persistence.playerFile.daytimeElapsed % 4);

        switch (time)
        {
            case ClockDaytimeType.MORNING:
                args.RemoveGirlFromAllPools(Girls.Celeste);
                break;
            case ClockDaytimeType.AFTERNOON:
                if (UnityEngine.Random.Range(0, 10) > 2) args.RemoveGirlFromAllPools(Girls.Momo);
                break;
            case ClockDaytimeType.EVENING:
                if (UnityEngine.Random.Range(0, 10) > 2) args.RemoveGirlFromAllPools(Girls.Momo);
                break;
            case ClockDaytimeType.NIGHT:
                args.RemoveGirlFromAllPools(Girls.Celeste);
                if (UnityEngine.Random.Range(0, 10) > 2) args.RemoveGirlFromAllPools(Girls.Audrey);
                break;
        }
    }

    internal static void On_RequestUnlockedPhotos(RequestUnlockedPhotosEventArgs args)
    {
        if (!Plugin.UnlockPhotos.Value) return;

        args.UnlockedPhotos ??= new();

        for (int i = 0; i < Photos.Count; i++)
        {
            args.UnlockedPhotos.Add(ModInterface.GameData.GetPhoto(new RelativeId(Plugin.ModId, i)));
        }
    }

    internal static void On_PreLoadPlayerFile(PlayerFile file)
    {
        Plugin.GameStarted = true;

        if (Plugin.UnlockStyles.Value)
        {
            using (ModInterface.Log.MakeIndent())
            {
                foreach (var fileGirl in file.girls)
                {
                    var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, fileGirl.girlDefinition.id);
                    var expansion = ExpandedGirlDefinition.Get(girlId);

                    foreach (var outfitId in expansion.OutfitLookup.Ids.Where(x => x.SourceId == Plugin.ModId))
                    {
                        fileGirl.UnlockOutfit(expansion.OutfitLookup[outfitId]);
                    }

                    foreach (var hairstyleId in expansion.HairstyleLookup.Ids.Where(x => x.SourceId == Plugin.ModId))
                    {
                        fileGirl.UnlockHairstyle(expansion.HairstyleLookup[hairstyleId]);
                    }
                }
            }
        }

        // if the player has already met Lola from single date, then the meeting loop won't ever get started
        // ideally I'd make a backup cutscene for this case, but I don't have voice acting...
        // so just un-meet her I guess. She will keep her exp, you just gotta do her cutscene again
        var nikkiSaveFile = file.girls.FirstOrDefault(x
            => ModInterface.Data.GetDataId(GameDataType.Girl, x.girlDefinition.id) == Girls.Nikki);

        if (nikkiSaveFile != null
            && !nikkiSaveFile.playerMet)
        {
            file.metGirlPairs.Remove(ModInterface.GameData.GetGirlPair(Pairs.LolaSingleDate));
        }
    }
}
