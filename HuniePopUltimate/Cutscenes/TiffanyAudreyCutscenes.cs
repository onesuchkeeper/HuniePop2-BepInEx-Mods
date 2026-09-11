using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;

namespace HuniePopUltimate;

public static class TiffanyAudreyCutscenes
{
    public static void AddDataMods()
    {
        AddCompatibleCutscene();
        AddAttractedCutscene();
        AddLoversCutscene();
        AddPostBonusRoundCutscene();
    }

    private static void AddCompatibleCutscene()
    {
        var tiffanyAudio = new AudioClipInfo()
        {
            IsExternal = true,
            Path = Path.Combine(Plugin.ROOT_DIR, "audio", "TiffanyTest.wav")
        };

        var audreyAudio = new AudioClipInfo()
        {
            IsExternal = true,
            Path = Path.Combine(Plugin.ROOT_DIR, "audio", "AudreyTest.wav")
        };

        var happy = new DialogLineExpression()
        {
            expressionType = GirlExpressionType.NEUTRAL,
            eyesClosed = false,
            percentRead = 0f
        };

        var annoyed = new DialogLineExpression()
        {
            expressionType = GirlExpressionType.ANNOYED,
            eyesClosed = false,
            percentRead = 0f
        };

        var endHappy = new DialogLineExpression()
        {
            expressionType = GirlExpressionType.NEUTRAL,
            eyesClosed = false,
            percentRead = 1f
        };

        var endAnnoyed = new DialogLineExpression()
        {
            expressionType = GirlExpressionType.ANNOYED,
            eyesClosed = false,
            percentRead = 1f
        };

        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(Cutscenes.TiffanyAudrey.Compatible, InsertStyle.append)
        {
            Steps =
            [
                CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.INNER).TargetOrientation(DollOrientationType.LEFT),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Aw sweet! Audrey, we're finally here!",
                    AudioClipInfo = tiffanyAudio,
                    StartExpression = happy,
                    EndExpression = endHappy,
                }).TargetGirl(Girls.Tiffany),

                CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.INNER).TargetOrientation(DollOrientationType.RIGHT),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Holy. Shit. It is SO damn hot out.",
                    AudioClipInfo = audreyAudio,
                    StartExpression = annoyed,
                    EndExpression = endAnnoyed,
                }).TargetGirl(Girls.Audrey),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Come on it\'s not that bad, and I\'m sure there\'s AC at the hotel",
                    AudioClipInfo = tiffanyAudio,
                    StartExpression = happy,
                    EndExpression = endHappy,
                }).TargetGirl(Girls.Tiffany),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Nikki\'s flight got in first so she\'s probably checked us in already",
                    AudioClipInfo = tiffanyAudio,
                    StartExpression = happy,
                    EndExpression = endHappy,
                }).TargetGirl(Girls.Tiffany),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Ugh, she\'s probably holed up in there with the head on.",
                    AudioClipInfo = audreyAudio,
                    StartExpression = annoyed,
                    EndExpression = endAnnoyed,
                }).TargetGirl(Girls.Audrey),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Did you see that bitch brought like five hoodies with her? I'm out here sweating my tits off and she-",
                    AudioClipInfo = audreyAudio,
                    StartExpression = annoyed,
                    EndExpression = endAnnoyed,
                }, true).TargetGirl(Girls.Audrey),

                CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.OUTER, 0.5f, CutsceneStepProceedType.INSTANT).TargetOrientation(DollOrientationType.LEFT),
                CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.OUTER, 0.5f).TargetOrientation(DollOrientationType.RIGHT),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "What the HELL are you doing here?",
                    AudioClipInfo = audreyAudio,
                    StartExpression = annoyed,
                    EndExpression = endAnnoyed,
                }, true).TargetGirl(Girls.Audrey),

                CutsceneStepUtility.MakeDialogOptionsInfo(
                [
                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "Hey Tiffany, Audrey, long time no see!",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Well, at least there\'s one other person on this island who knows how to have a good time.",
                                AudioClipInfo = audreyAudio,
                                StartExpression = annoyed,
                                EndExpression = endAnnoyed,
                            }).TargetGirl(Girls.Audrey)
                        ]
                    },

                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "Tiffany! I like the new hat.",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Excuse me, I'm right here.",
                                AudioClipInfo = audreyAudio,
                                StartExpression = annoyed,
                                EndExpression = endAnnoyed,
                            }).TargetGirl(Girls.Audrey)
                        ]
                    },

                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "Tiffany is this person bothering you? I can go get security.",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Hey, I\'m not the one perving around an airport for no reason!",
                                AudioClipInfo = audreyAudio,
                                StartExpression = annoyed,
                                EndExpression = endAnnoyed,
                            }).TargetGirl(Girls.Audrey)
                        ]
                    }
                ], true),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "And you know Tiffany how?",
                    AudioClipInfo = audreyAudio,
                    StartExpression = annoyed,
                    EndExpression = endAnnoyed,
                }).TargetGirl(Girls.Audrey),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Oh, we went on a couple dates a while back. It\'s cool Audrey, it was just a one time thing.",
                    AudioClipInfo = tiffanyAudio,
                    StartExpression = happy,
                    EndExpression = endHappy,
                }).TargetGirl(Girls.Tiffany),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "We\'re here for the rave this weekend, we should all totally hang out sometime since you\'re here!",
                    AudioClipInfo = tiffanyAudio,
                    StartExpression = happy,
                    EndExpression = endHappy,
                }).TargetGirl(Girls.Tiffany).Proceed(),

                CutsceneStepUtility.MakeDialogOptionsInfo(
                [
                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "At that club near Love Lei? Count me in!",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "That\'s the one! I hear the music is awesome!",
                                AudioClipInfo = tiffanyAudio,
                                StartExpression = annoyed,
                                EndExpression = endAnnoyed,
                            }).TargetGirl(Girls.Tiffany)
                        ]
                    },

                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "Yeah, and we should go to this sick grotto I found too!",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Really? I didn\'t see that on the website, that sounds so cool!",
                                AudioClipInfo = tiffanyAudio,
                                StartExpression = annoyed,
                                EndExpression = endAnnoyed,
                            }).TargetGirl(Girls.Tiffany)
                        ]
                    },

                    new CutsceneDialogOptionInfo()
                    {
                        DialogOptionText = "Me, and two lovely ladies? Golf. We need to play golf.",
                        Steps =
                        [
                            CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                            {
                                DialogText = "Oh, they have golf here? Uh, Sure!",
                                AudioClipInfo = tiffanyAudio,
                                StartExpression = annoyed,
                                EndExpression = endAnnoyed,
                            }).TargetGirl(Girls.Tiffany)
                        ]
                    }
                ], true),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Wouldn\'t that be so much fun Audrey?",
                    AudioClipInfo = tiffanyAudio,
                    StartExpression = happy,
                    EndExpression = endHappy,
                }).TargetGirl(Girls.Tiffany),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Um, yeah, fine, whatever...",
                    AudioClipInfo = audreyAudio,
                    StartExpression = annoyed,
                    EndExpression = endAnnoyed,
                }).TargetGirl(Girls.Audrey),
            ]
        });
    }

    private static void AddAttractedCutscene()
    {
        var tiffanyAudio = new AudioClipInfo()
        {
            IsExternal = true,
            Path = Path.Combine(Plugin.ROOT_DIR, "audio", "TiffanyTest.wav")
        };

        var audreyAudio = new AudioClipInfo()
        {
            IsExternal = true,
            Path = Path.Combine(Plugin.ROOT_DIR, "audio", "AudreyTest.wav")
        };

        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(Cutscenes.TiffanyAudrey.Attracted, InsertStyle.append)
        {
            Steps =
            [
                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Not so bad. Now I remember how you managed to get in my pants the first time.",
                    AudioClipInfo = audreyAudio,
                }).TargetGirl(Girls.Audrey),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Wait, what!? You and Audrey were… you two had",
                    AudioClipInfo = tiffanyAudio,
                }).TargetGirl(Girls.Tiffany),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    Yuri = true,
                    YuriDialogText = "Relax, it was a one time thing. *scoff* I mean, It\'s not like she was dating us at the same time or anything.",
                    DialogText = "Relax, it was a one time thing. *scoff* I mean, It\'s not like he was dating us at the same time or anything.",
                    AudioClipInfo = audreyAudio,
                }).TargetGirl(Girls.Audrey).Proceed()
            ]
        });
    }

    private static void AddLoversCutscene()
    {
        var tiffanyAudio = new AudioClipInfo()
        {
            IsExternal = true,
            Path = Path.Combine(Plugin.ROOT_DIR, "audio", "TiffanyTest.wav")
        };

        var audreyAudio = new AudioClipInfo()
        {
            IsExternal = true,
            Path = Path.Combine(Plugin.ROOT_DIR, "audio", "AudreyTest.wav")
        };

        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(Cutscenes.TiffanyAudrey.Lovers, InsertStyle.append)
        {
            Steps =
            [
                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Tiff, is it just me or is it getting hot as shit? I,▸▸ might need some help over here…",
                    AudioClipInfo = audreyAudio,
                }).TargetGirl(Girls.Audrey),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Oh yeah? I think I can help out with that…",
                    AudioClipInfo = tiffanyAudio,
                }).TargetGirl(Girls.Tiffany).Proceed()
            ]
        });
    }

    private static void AddPostBonusRoundCutscene()
    {
        var tiffanyAudio = new AudioClipInfo()
        {
            IsExternal = true,
            Path = Path.Combine(Plugin.ROOT_DIR, "audio", "TiffanyTest.wav")
        };

        var audreyAudio = new AudioClipInfo()
        {
            IsExternal = true,
            Path = Path.Combine(Plugin.ROOT_DIR, "audio", "AudreyTest.wav")
        };

        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(Cutscenes.TiffanyAudrey.PostBonusRound, InsertStyle.append)
        {
            Steps =
            [
                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Audrey… you\'re so cute when you\'re like that.▸▸ I love you…",
                    AudioClipInfo = tiffanyAudio,
                }).TargetGirl(Girls.Tiffany),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "[aa]Are you gay?! G-get off me!",
                    AudioClipInfo = audreyAudio,
                }).TargetGirl(Girls.Audrey),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "[aa aa aa]▸▸What???▸[aa]▸ Like you're not?",
                    AudioClipInfo = tiffanyAudio,
                }).TargetGirl(Girls.Tiffany),

                CutsceneStepUtility.MakeDialogLineInfo(new DialogLineDataMod(Cutscenes.NextDialogLineId, InsertStyle.append)
                {
                    DialogText = "Shut up!",
                    AudioClipInfo = audreyAudio,
                }).TargetGirl(Girls.Audrey).Proceed()
            ]
        });
    }
}
