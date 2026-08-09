using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace HuniePopUltimate;
public static class AudreyBaggageCutscenes
{
    public static void AddDataMods()
    {
        AddMaterialisticCutscene();
        AddMegaBitchCutscene();
        AddAddictCutscene();
    }

    private static void AddMaterialisticCutscene()
    {
        var audioTest = new AudioClipInfo()
        {
            IsExternal = true,
            Path = Path.Combine(Plugin.ROOT_DIR, "audio", "AudreyTest.wav")
        };

        var start = new DialogLineExpression()
        {
            expressionType = GirlExpressionType.NEUTRAL,
            eyesClosed = false,
            percentRead = 0f
        };

        var end = new DialogLineExpression()
        {
            expressionType = GirlExpressionType.NEUTRAL,
            eyesClosed = false,
            percentRead = 1f
        };

        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(Items.Audrey.Baggage2, InsertStyle.append)
        {
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "So... what'd you get me?",
                    AudioClipInfo = audioTest,
                    StartExpression = start,
                    EndExpression = end,
                }, true),

                CutsceneStepUtility.MakeDialogOptionsInfo(
                [
                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "Uh... my charming personality?",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "I know you didn't just try and pull that shit.",
                                AudioClipInfo = audioTest,
                                StartExpression = start,
                                EndExpression = end,
                            })
                        ]
                    },

                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "I didn't know I was supposed to bring something.",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Don't give me that shit, you've known me long enough to get how this works.",
                                AudioClipInfo = audioTest,
                                StartExpression = start,
                                EndExpression = end,
                            })
                        ]
                    },

                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "Oh! The hotel rooms all have free lotion and tissues!",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Oh fantastic, the virgin special.",
                                AudioClipInfo = audioTest,
                                StartExpression = new DialogLineExpression()
                                {
                                    expressionType = GirlExpressionType.UPSET,
                                    eyesClosed = false,
                                    percentRead = 0f
                                },
                                EndExpression = end,
                            })
                        ]
                    }
                ], true),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Listen, I don't do broke.",
                    AudioClipInfo = audioTest,
                    StartExpression = start,
                    EndExpression = end,
                }),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Either you get with the goods or... Just don't fuck with me got it!",
                    AudioClipInfo = audioTest,
                    StartExpression = start,
                    EndExpression = end,
                }).Proceed(),
            }
        });
    }

    private static void AddAddictCutscene()
    {
        var audioTest = new AudioClipInfo()
        {
            IsExternal = true,
            Path = Path.Combine(Plugin.ROOT_DIR, "audio", "AudreyTest.wav")
        };

        var start = new DialogLineExpression()
        {
            expressionType = GirlExpressionType.ANNOYED,
            eyesClosed = false,
            percentRead = 0f
        };

        var end = new DialogLineExpression()
        {
            expressionType = GirlExpressionType.ANNOYED,
            eyesClosed = false,
            percentRead = 1f
        };

        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(Items.Audrey.Baggage3, InsertStyle.append)
        {
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Fuck, I'm out of coke. How much you got on you?",
                    AudioClipInfo = audioTest,
                    StartExpression = start,
                    EndExpression = end,
                }, true),

                CutsceneStepUtility.MakeDialogOptionsInfo(
                [
                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "You can't bring that shit on a plane.",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Ha, only if you're a pussy you can't.",
                                AudioClipInfo = audioTest,
                                StartExpression = new DialogLineExpression()
                                {
                                    expressionType = GirlExpressionType.EXCITED,
                                    eyesClosed = false,
                                    percentRead = 0f
                                },
                                EndExpression = end,
                            })
                        ]
                    },

                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "I think there's a vending machine back that way.",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Not soda, dumbass.",
                                AudioClipInfo = audioTest,
                                StartExpression = start,
                                EndExpression = end,
                            })
                        ]
                    },

                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "I know this cleaning lady, Nora, who could probably get you some.",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Really? You better not be fucking with me.",
                                AudioClipInfo = audioTest,
                                StartExpression = new DialogLineExpression()
                                {
                                    expressionType = GirlExpressionType.INQUISITIVE,
                                    eyesClosed = false,
                                    percentRead = 0f
                                },
                                EndExpression = end,
                            })
                        ]
                    }
                ], true),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "I ran out this morning. I'm fucking dying over here.",
                    AudioClipInfo = audioTest,
                    StartExpression = start,
                    EndExpression = end,
                }),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Won't be much of a vacation if I can't get totally wired.",
                    AudioClipInfo = audioTest,
                    StartExpression = start,
                    EndExpression = end,
                }).Proceed(),
            }
        });
    }

    private static void AddMegaBitchCutscene()
    {
        var audioTest = new AudioClipInfo()
        {
            IsExternal = true,
            Path = Path.Combine(Plugin.ROOT_DIR, "audio", "AudreyTest.wav")
        };

        var start = new DialogLineExpression()
        {
            expressionType = GirlExpressionType.ANNOYED,
            eyesClosed = false,
            percentRead = 0f
        };

        var end = new DialogLineExpression()
        {
            expressionType = GirlExpressionType.ANNOYED,
            eyesClosed = false,
            percentRead = 1f
        };

        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(Items.Audrey.Baggage1, InsertStyle.append)
        {
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Well, say something.",
                    AudioClipInfo = audioTest,
                    StartExpression = start,
                    EndExpression = end,
                }, true),

                CutsceneStepUtility.MakeDialogOptionsInfo(
                [
                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "\"something\"",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Oh, way to go smart ass.",
                                AudioClipInfo = audioTest,
                                StartExpression = new DialogLineExpression()
                                {
                                    expressionType = GirlExpressionType.UPSET,
                                    eyesClosed = false,
                                    percentRead = 0f
                                },
                                EndExpression = end,
                            })
                        ]
                    },

                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "What?",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "\"What?\"",
                                AudioClipInfo = audioTest,
                                StartExpression = new DialogLineExpression()
                                {
                                    expressionType = GirlExpressionType.UPSET,
                                    eyesClosed = false,
                                    percentRead = 0f
                                },
                                Expressions =
                                [
                                    new DialogLineExpression()
                                    {
                                        expressionType = GirlExpressionType.ANNOYED,
                                        eyesClosed = false,
                                        percentRead = 0.35f
                                    }
                                ],
                                EndExpression = end,
                            }),

                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Fuck, you're dense.",
                                AudioClipInfo = audioTest,
                                StartExpression = start,
                                EndExpression = end,
                            })
                        ]
                    },

                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "Uhh, fuck these birds, am I right?",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Was that supposed to be funny? The fuck does that even mean?",
                                AudioClipInfo = audioTest,
                                StartExpression = start,
                                EndExpression = end,
                            })
                        ]
                    }
                ], true),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    Yuri = true,
                    YuriDialogText = "You spend so much time sucking cunt you forget how to form a damn sentence?",
                    DialogText = "You spend so much time sucking dick you forget how to form a damn sentence?",
                    AudioClipInfo = audioTest,
                    StartExpression = new DialogLineExpression()
                    {
                        expressionType = GirlExpressionType.UPSET,
                        eyesClosed = false,
                        percentRead = 0f
                    },
                    EndExpression = new DialogLineExpression()
                    {
                        expressionType = GirlExpressionType.UPSET,
                        eyesClosed = false,
                        percentRead = 1f
                    },
                }),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "God you're pathetic.",
                    AudioClipInfo = audioTest,
                    StartExpression = new DialogLineExpression()
                    {
                        expressionType = GirlExpressionType.UPSET,
                        eyesClosed = false,
                        percentRead = 0f
                    },
                    EndExpression = end,
                }).Proceed(),
            }
        });
    }
}