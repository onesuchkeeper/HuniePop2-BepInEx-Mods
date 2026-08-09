using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using System.Collections.Generic;
using System.IO;

namespace HuniePopUltimate;

public static class TiffanyBaggageCutscenes
{
    public static void AddDataMods()
    {
        AddMommyIssuesCutscene();
        AddPerfectionistCutscene();
        AddPeoplePleaserCutscene();
    }

    private static void AddMommyIssuesCutscene()
    {
        var audioTest = new AudioClipInfo()
        {
            IsExternal = true,
            Path = Path.Combine(Plugin.ROOT_DIR, "audio", "TiffanyTest.wav")
        };

        var startDisappointed = new DialogLineExpression() { 
            expressionType = GirlExpressionType.DISAPPOINTED, 
            eyesClosed = false, 
            percentRead = 0f
        };

        var endDisappointed = new DialogLineExpression() { 
            expressionType = GirlExpressionType.DISAPPOINTED, 
            eyesClosed = false, 
            percentRead = 1f
        };

        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(Items.Tiffany.Baggage1, InsertStyle.append)
        {
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append) {
                        DialogText = "Hey, so, I... saw my mom here earlier today...",
                        AudioClipInfo = audioTest,
                        StartExpression = startDisappointed,
                        EndExpression = endDisappointed,
                }),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append) {
                        DialogText = "She was at the hotel bar with Audrey's aunt of all people. But, I don't think she saw me.",
                        AudioClipInfo = audioTest,
                        StartExpression = startDisappointed,
                        EndExpression = endDisappointed,
                }, true),

                CutsceneStepUtility.MakeDialogOptionsInfo(
                [
                    new CutsceneDialogOptionInfo() { 
                        DialogOptionText = "Do you think she followed you here?",
                        Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>() {
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append) {
                                DialogText = "No, at least I don't think so. She used to come out here sometimes for work.",
                                AudioClipInfo = audioTest,
                                StartExpression = startDisappointed,
                                EndExpression = endDisappointed,
                            }),
                        }
                    },
                    new CutsceneDialogOptionInfo() { 
                        DialogOptionText = "Is she that nicotine-powered cougar with the legendary rack?",
                        Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>() {
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append) {
                                    DialogText = "I... guess that is one way to describe her. You've met her?",
                                    AudioClipInfo = audioTest,
                                    StartExpression = new DialogLineExpression() { 
                                        expressionType = GirlExpressionType.CONFUSED, 
                                        eyesClosed = false, 
                                        percentRead = 0f
                                    },
                                    EndExpression = endDisappointed,
                            }),
                        }
                    },
                    new CutsceneDialogOptionInfo() { 
                        DialogOptionText = "You don't sound happy about that",
                        Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>() {
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append) {
                                    DialogText = "No, I'm not. Ugh, I can't believe she's here!",
                                    AudioClipInfo = audioTest,
                                    StartExpression = new DialogLineExpression() { 
                                        expressionType = GirlExpressionType.UPSET, 
                                        eyesClosed = true, 
                                        percentRead = 0f
                                    },
                                    Expressions = new List<DialogLineExpression>() {
                                        new DialogLineExpression() { 
                                            expressionType = GirlExpressionType.UPSET, 
                                            eyesClosed = false, 
                                            percentRead = 0.4f
                                        }
                                    },
                                    EndExpression = endDisappointed,
                            }),
                        }
                    },
                ], true),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append) {
                        DialogText = "Is it okay... to not forgive someone?",
                        AudioClipInfo = audioTest,
                        StartExpression = startDisappointed,
                        EndExpression = endDisappointed,
                }),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append) {
                        DialogText = "Things didn't go that well the last time we talked...",
                        AudioClipInfo = audioTest,
                        StartExpression = startDisappointed,
                        EndExpression = endDisappointed,
                }),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append) {
                        DialogText = "She was so drunk. I doubt she even remembers.",
                        AudioClipInfo = audioTest,
                        StartExpression = startDisappointed,
                        EndExpression = endDisappointed,
                }).Proceed(),
            }
        });
    }

    private static void AddPeoplePleaserCutscene()
    {
        var audioTest = new AudioClipInfo()
        {
            IsExternal = true,
            Path = Path.Combine(Plugin.ROOT_DIR, "audio", "TiffanyTest.wav")
        };

        var startHappy = new DialogLineExpression()
        {
            expressionType = GirlExpressionType.NEUTRAL,
            eyesClosed = false,
            percentRead = 0f
        };

        var endHappy = new DialogLineExpression()
        {
            expressionType = GirlExpressionType.NEUTRAL,
            eyesClosed = false,
            percentRead = 1f
        };

        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(Items.Tiffany.Baggage3, InsertStyle.append)
        {
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Do you think Audrey and Nikki are having a good time?",
                    AudioClipInfo = audioTest,
                    StartExpression = startHappy,
                    EndExpression = endHappy,
                }, true),

                CutsceneStepUtility.MakeDialogOptionsInfo(
                [
                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "I'm not sure Audrey is capable of that...",
                        Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
                        {
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Oh be nice! She has plenty of fun, but it's usually at parties.",
                                AudioClipInfo = audioTest,
                                StartExpression = new DialogLineExpression()
                                {
                                    expressionType = GirlExpressionType.ANNOYED,
                                    eyesClosed = false,
                                    percentRead = 0f
                                },
                                EndExpression = endHappy,
                            }),
                        }
                    },

                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "Nikki is a bit of a homebody",
                        Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
                        {
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "She gets out! Sometimes... At least once a month!",
                                AudioClipInfo = audioTest,
                                StartExpression = new DialogLineExpression()
                                {
                                    expressionType = GirlExpressionType.DISAPPOINTED,
                                    eyesClosed = false,
                                    percentRead = 0f
                                },
                                EndExpression = endHappy,
                            }),
                        }
                    },

                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "It's a tropical paradise, how could they not?",
                        Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
                    },
                ], true),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "I'm just a little worried that nature isn't so much their scene.",
                    AudioClipInfo = audioTest,
                    StartExpression = new DialogLineExpression()
                    {
                        expressionType = GirlExpressionType.DISAPPOINTED,
                        eyesClosed = false,
                        percentRead = 0f
                    },
                    EndExpression = endHappy,
                }),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "I hope they didn't feel pressured into coming along.",
                    AudioClipInfo = audioTest,
                    StartExpression = new DialogLineExpression()
                    {
                        expressionType = GirlExpressionType.DISAPPOINTED,
                        eyesClosed = false,
                        percentRead = 0f
                    },
                    EndExpression = endHappy,
                }).Proceed(),
            }
        });
    }

    private static void AddPerfectionistCutscene()
    {
        var audioTest = new AudioClipInfo()
        {
            IsExternal = true,
            Path = Path.Combine(Plugin.ROOT_DIR, "audio", "TiffanyTest.wav")
        };

        var startHappy = new DialogLineExpression()
        {
            expressionType = GirlExpressionType.NEUTRAL,
            eyesClosed = false,
            percentRead = 0f
        };

        var endHappy = new DialogLineExpression()
        {
            expressionType = GirlExpressionType.NEUTRAL,
            eyesClosed = false,
            percentRead = 1f
        };

        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(Items.Tiffany.Baggage2, InsertStyle.append)
        {
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "I'm so glad to finally be here on the island!",
                    AudioClipInfo = audioTest,
                    StartExpression = startHappy,
                    EndExpression = endHappy,
                }),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "I actually started to plan this trip back when you first took me to that water park, remember?",
                    AudioClipInfo = audioTest,
                    StartExpression = startHappy,
                    EndExpression = endHappy,
                }, true),

                CutsceneStepUtility.MakeDialogOptionsInfo(
                [
                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "You've been planning for that long?",
                        Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
                        {
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Only a couple years. You should have seen my sweet 16, I started those plans when I was 10!",
                                AudioClipInfo = audioTest,
                                StartExpression = new DialogLineExpression()
                                {
                                    expressionType = GirlExpressionType.EXCITED,
                                    eyesClosed = false,
                                    percentRead = 0f
                                },
                                EndExpression = endHappy,
                            }),
                        }
                    },

                    new CutsceneDialogOptionInfo()
                    {
                        Yuri = true,
                        YuriDialogOptionText = "Couldn't get enough of my bikini huh?",
                        DialogOptionText = "Couldn't get enough of my trunks huh?",
                        Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
                        {
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                Yuri = true,
                                YuriDialogText = "*laugh* I admit, you do fill it out well.",
                                DialogText = "*laugh* I admit, you do fill them out well.",
                                AudioClipInfo = audioTest,
                                StartExpression = new DialogLineExpression()
                                {
                                    expressionType = GirlExpressionType.EXCITED,
                                    eyesClosed = false,
                                    percentRead = 0f
                                },
                                EndExpression = endHappy,
                            }),
                        }
                    },

                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "",
                        Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
                        {
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "",
                                AudioClipInfo = audioTest,
                                StartExpression = startHappy,
                                EndExpression = endHappy,
                            }),
                        }
                    },
                ], true),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "I just wanted to make sure every little thing was perfect!",
                    AudioClipInfo = audioTest,
                    StartExpression = startHappy,
                    EndExpression = endHappy,
                }).Proceed(),
            }
        });
    }
}