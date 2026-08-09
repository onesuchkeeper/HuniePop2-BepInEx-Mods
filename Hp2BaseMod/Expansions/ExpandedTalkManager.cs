using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

[Expansion(typeof(TalkManager))]
public partial class ExpandedTalkManager
{
    [HarmonyPatch(typeof(TalkManager))]
    private static class TalkManagerPatch
    {
        [HarmonyPatch("TalkStep")]
        [HarmonyPrefix]
        public static bool TalkStep(TalkManager __instance)
            => ExpandedTalkManager.Get(__instance).TalkStep_Prefix();

        [HarmonyPatch(nameof(TalkManager.TalkWith))]
        [HarmonyPrefix]
        public static bool TalkWith(TalkManager __instance, int dollIndex)
            => ExpandedTalkManager.Get(__instance).TalkWith_Prefix(dollIndex);

        [HarmonyPatch("ReceiveFruitFromGirl")]
        [HarmonyPrefix]
        public static bool ReceiveFruitFromGirl(TalkManager __instance, UiDoll fruitDoll, bool silent)
            => ExpandedTalkManager.Get(__instance).ReceiveFruitFromGirl_Prefix(fruitDoll, silent);
    }

    private bool ReceiveFruitFromGirl_Prefix(UiDoll fruitDoll, bool silent)
    {
        var randomFruit = fruitDoll.girlDefinition.GetExpansion().GetRandomFruit();

        if (randomFruit == null)
        {
            ModInterface.Log.Error("FRUIT WAS NULL");
            return false;
        }

        var affection = randomFruit.GetExpansion().Affection;
		Game.Persistence.playerFile.AddFruitCount(affection.Id, 1);

        if (affection == null)
        {
            ModInterface.Log.Error("AFFECTION WAS NULL");
            return false;
        }

		Object.Instantiate(_core.energyTrailPrefab).Init(EnergyTrailFormat.START_AND_END, randomFruit.energyDefinition, randomFruit, fruitDoll, "+1 " + StringUtils.Titleize(affection.Name) + " Seed");
		if (!silent)
		{
			Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Session.Gift.sfxFruitReward, fruitDoll.pauseDefinition).audioSource.pitch = 1.2f;
			Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Session.Gift.sfxFruitPop, fruitDoll.pauseDefinition).audioSource.pitch = Random.Range(0.75f, 1.5f);
			Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Session.Gift.sfxResourceFlourish, fruitDoll.pauseDefinition);
		}
        return false;
    }

    private bool TalkWith_Prefix(int dollIndex)
	{
		if (Game.Session.Location.currentGirlPair == null) return false;

		_girlPair = Game.Session.Location.currentGirlPair;
		_fileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);
        var altGirl = dollIndex > 0;
		_altGirl = altGirl;
        var statusGirl = Game.Session.Puzzle.puzzleStatus.GetStatusGirl(altGirl);
		_statusGirl = statusGirl;
		_oppositeStatusGirl = Game.Session.Puzzle.puzzleStatus.GetStatusGirl(!altGirl);
		var targetDoll = Game.Session.gameCanvas.GetDoll(altGirl);
		_targetDoll = targetDoll;
		_oppositeDoll = Game.Session.gameCanvas.GetDoll(!altGirl);
        var targetDef = targetDoll.girlDefinition;
        var targetDefExp = targetDef.GetExpansion();
        var fileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(targetDef);
		_fileGirl = fileGirl;
		_oppositeFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(_oppositeDoll.girlDefinition);
		Game.Session.Puzzle.puzzleStatus.SetGirlFocus(altGirl);

        if (statusGirl.stamina < 2)
        {
            Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Manager.Ui.sfxReject, targetDoll.pauseDefinition);
            if (Game.Manager.Windows.IsWindowActive(null, true, false))
            {
                Game.Manager.Windows.ShowWindow(Game.Session.Location.actionBubblesWindow, true);
                Game.Manager.Windows.HideWindow();
            }
            return false;
        }

        _talkType = targetDefExp.TalkHandler.SelectTalkType(fileGirl);

        _isTalking = true;
        _talkStepIndex = -1;
        if (_talkType != TalkWithType.BAGGAGE_CONVO)
        {
            Game.Persistence.playerFile.relationshipPoints += 2;
            statusGirl.playerFileGirl.relationshipPoints += 2;
        }

        Game.Session.Puzzle.puzzleStatus.AddResourceValue(PuzzleResourceType.STAMINA, -2, altGirl);
        if (Game.Session.Puzzle.puzzleStatus.movesRemaining < Game.Session.Puzzle.puzzleStatus.maxMovesRemaining)
        {
            Game.Session.Puzzle.puzzleStatus.AddResourceValue(PuzzleResourceType.MOVES, 1, altGirl);
            TokenDefinition byResourceType = Game.Data.Tokens.GetByResourceType(PuzzleResourceType.MOVES, PuzzleAffectionType.TALENT);
            UnityEngine.Object.Instantiate(_core.energyTrailPrefab).Init(EnergyTrailFormat.START_AND_END, byResourceType.energyDefinition, null, targetDoll, "+1 " + byResourceType.resourceName);
            Game.Manager.Audio.Play(AudioCategory.SOUND, _core.sfxTalkReward, targetDoll.pauseDefinition);
        }

        Game.Session.Puzzle.puzzleStatus.CheckChanges();
        Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Session.Gift.sfxResourceFlourish, targetDoll.pauseDefinition);

        if (Game.Manager.Windows.IsWindowActive(null, true, false))
        {
            Game.Manager.Windows.HideWindow();
        }
        
        TalkStep();
        return false;
	}

    private bool TalkStep_Prefix()
    {
        var nextIndex = _talkStepIndex + 1;
        switch (_talkType)
        {
            case TalkWithType.FAVORITE_QUESTION:
                ModInterface.Log.Message($"FAVORITE_QUESTION {nextIndex}");
                switch (nextIndex)
                {
                    case 1:
                        _talkStepIndex = nextIndex;
                        FavoriteQuestionShowOptions();
                        return false;
                    case 2:
                        _talkStepIndex = nextIndex;
                        FavoriteQuestionHandleSelection();
                        return false;
                    case 3:
                        _talkStepIndex = nextIndex;
                        FavoriteQuestionResponse();
                        return false;
                }
                break;
            case TalkWithType.BAGGAGE_CONVO:
                switch (nextIndex)
                {
                    case 0:
                        _talkStepIndex = nextIndex;
                        BaggageConvoStart();
                        return false;
                    case 1:
                        _talkStepIndex = nextIndex;
                        BaggageConvoCleanup();
                        return false;
                }
                break;
        }

        return true;
    }

    private void BaggageConvoStart()
    {
        _oppositeDoll.SetFocus(false, 0.5f);
        var itemDefinition = _statusGirl.girlDefinition.GetExpansion().TalkHandler.GetBaggageItem(_statusGirl.playerFileGirl);
        if (itemDefinition != null)
        {
            _statusGirl.playerFileGirl.LearnBaggage(itemDefinition);
            Game.Session.Cutscenes.CutsceneCompleteEvent += OnCutsceneComplete_Hook;
            Game.Session.Cutscenes.StartCutscene(_core.baggageWrapCutsceneDef, itemDefinition.cutsceneDefinition);
            return;
        }

		TalkStep();
    }

    private void BaggageConvoCleanup()
    {
        _oppositeDoll.SetFocus(true, 0.5f);
        TalkEnd();
    }

    private void FavoriteQuestionResponse()
    {
        var girl = _fileGirl.girlDefinition;

        var oppositeDoll = _oppositeDoll;
        var oppositeDef = oppositeDoll.girlDefinition.GetExpansion().FavQuestionIdToAnswerId;

        var selectedQuestionId = ModInterface.Data.GetDataId(GameDataType.Question, Game.Session.Dialog.selectedDialogOptionIndex);
        var selectedQuestion = ModInterface.GameData.GetQuestion(selectedQuestionId);

        var args = new TalkFavQuestionResponseArgs()
        {
            OtherGirlResponds = oppositeDef.TryGetValue(selectedQuestionId, out var oppositeAnswer)
                && oppositeAnswer == girl.GetExpansion().FavQuestionIdToAnswerId[selectedQuestionId]
        };

        ModInterface.Events.NotifyFavQuestionResponse(args);

        if (args.OtherGirlResponds)
        {
            _oppositeFileGirl.LearnFavAnswer(selectedQuestion);
            m_ReceiveFruitFromGirl.Invoke(_core, [oppositeDoll, false]);
            oppositeDoll.DialogLineCompleteEvent += OnDialogLineComplete_Hook;
            oppositeDoll.ReadDialogTrigger(_core.dtFavQuestionAgreement, DialogLineFormat.ACTIVE, selectedQuestion.GetExpansion().AnswerLookup[oppositeAnswer]);
        }
        else
        {
            TalkStep();
        }
    }

    private void FavoriteQuestionHandleSelection()
    {
        var fileGirl = _fileGirl;
        var targetDoll = _targetDoll;
        var girl = fileGirl.girlDefinition;
        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, girl.id);

        var selectedQuestionId = ModInterface.Data.GetDataId(GameDataType.Question, Game.Session.Dialog.selectedDialogOptionIndex);
        var selectedQuestion = ModInterface.GameData.GetQuestion(selectedQuestionId);

        _questionPool.Clear();

        fileGirl.LearnFavAnswer(selectedQuestion);

        if (_oppositeDoll.girlDefinition.GetExpansion().FavQuestionIdToAnswerId.TryGetValue(selectedQuestionId, out var oppositeAnswer)
            && oppositeAnswer == girl.GetExpansion().FavQuestionIdToAnswerId[selectedQuestionId])
        {
            m_ReceiveFruitFromGirl.Invoke(_core, [targetDoll, false]);
        }
        else
        {
            Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Manager.Ui.sfxReject, targetDoll.pauseDefinition);
        }

        if (!_core.dtFavQuestionResponse.GetExpansion().TryGetLineSet(_core.dtFavQuestionResponse, girlId, out var lineSet))
        {
            ModInterface.Log.Error($"Failed to find favorite question response DT set for girl {girlId}");
            m_OnDialogLineComplete.Invoke(_core, [targetDoll]);
            return;
        }

        var girlIndex = ExpandedGirlDefinition.DialogTriggerIndexes[ModInterface.Data.GetDataId(GameDataType.Girl, fileGirl.girlDefinition.id)];
        var lineIndex = ExpandedQuestionDefinition.DialogTriggerIndexes[ModInterface.Data.GetDataId(GameDataType.Question, selectedQuestion.id)];

        targetDoll.DialogLineCompleteEvent += OnDialogLineComplete_Hook;
        targetDoll.ReadDialogTrigger(_core.dtFavQuestionResponse, DialogLineFormat.ACTIVE, lineIndex);
    }

    private void FavoriteQuestionShowOptions()
    {
        ModInterface.Log.Message();
        var fileGirl = f_fileGirl.GetValue<PlayerFileGirl>(_core);
        var girl = fileGirl.girlDefinition;
        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, girl.id);
        ModInterface.Log.Message($"Target girl: {girl.girlName}");

        var questionPool = f_questionPool.GetValue<List<QuestionDefinition>>(_core);
        var targetGirlQuestions = girl.GetExpansion().FavQuestionIdToAnswerId.Keys.Select(ModInterface.GameData.GetQuestion);
        questionPool.Clear();
        questionPool.AddRange(targetGirlQuestions);

        var girlPair = f_girlPair.GetValue<GirlPairDefinition>(_core);
        var girlOneFavs = girlPair.girlDefinitionOne.GetExpansion().FavQuestionIdToAnswerId;
        var girlTwoFavs = girlPair.girlDefinitionTwo.GetExpansion().FavQuestionIdToAnswerId;

        var commonQuestions = new HashSet<int>();

        foreach (var entry in girlOneFavs)
        {
            if (girlTwoFavs.TryGetValue(entry.Key, out var value) && value == entry.Value)
            {
                commonQuestions.Add(ModInterface.Data.GetRuntimeDataId(GameDataType.Question, entry.Key));
            }
        }
        ModInterface.Log.Message("common questions: " + string.Join(", ", commonQuestions));
        questionPool.RemoveAll(x => commonQuestions.Contains(x.id));

        // remove questions from non-special chars that don't have answers
        void RemoveInvalidAnswers(GirlDefinition def)
        {
            if (def.specialCharacter) return;

            var favQuestionIdToAnswerId = def.GetExpansion().FavQuestionIdToAnswerId;

            questionPool.RemoveAll(x =>
            {
                var questionId = ModInterface.Data.GetDataId(GameDataType.Question, x.id);
                return !favQuestionIdToAnswerId.ContainsKey(questionId);
            });
        }

        RemoveInvalidAnswers(girlPair.girlDefinitionOne);
        RemoveInvalidAnswers(girlPair.girlDefinitionTwo);

        // if there aren't enough valid questions ask a 
        if (questionPool.Count < ModInterface.State.FavQuestionOptionCount)
        {
            questionPool.Clear();
            questionPool.AddRange(targetGirlQuestions);
            ModInterface.Log.Message($"Target girl questions: {string.Join(", ", targetGirlQuestions.Select(x => x.questionName))}");
        }

        ListUtils.ShuffleList(questionPool);
        ModInterface.Log.Message("final question pool: " + string.Join(", ", questionPool.Select(x => x.id)));

        if (!commonQuestions.Any())
        {
            // if the pair doesn't have common questions, just take random ones
            if (questionPool.Count > ModInterface.State.FavQuestionOptionCount)
            {
                questionPool.RemoveRange(0, questionPool.Count - ModInterface.State.FavQuestionOptionCount);
            }
        }
        else if (questionPool.Count > ModInterface.State.FavQuestionOptionCount)
        {
            // prune recent questions to ensure choice count
            var fileGirlPair = f_fileGirlPair.GetValue<PlayerFileGirlPair>(_core);
            var delta = questionPool.Count - fileGirlPair.recentFavQuestions.Count - ModInterface.State.FavQuestionOptionCount;

            if (delta < 0)
            {
                fileGirlPair.recentFavQuestions.RemoveRange(0, fileGirlPair.recentFavQuestions.Count + delta);
            }

            // remove recent questions
            if (fileGirlPair.recentFavQuestions.Any())
            {
                questionPool.RemoveAll(x => fileGirlPair.recentFavQuestions.Contains(x.id));
            }

            // see if we can add a guaranteed common answer
            var validCommons = commonQuestions.Except(fileGirlPair.recentFavQuestions).ToArray();
            if (validCommons.Any())
            {
                questionPool.RemoveRange(0, questionPool.Count - (ModInterface.State.FavQuestionOptionCount - 1));
                var randomQuestion = ModInterface.GameData.GetQuestion(validCommons.GetRandom());
                questionPool.Add(ModInterface.GameData.GetQuestion(validCommons.GetRandom()));
                fileGirlPair.AddRecentFavQuestion(randomQuestion.id);
            }
            else
            {
                questionPool.RemoveRange(0, questionPool.Count - ModInterface.State.FavQuestionOptionCount);
            }
        }

        var options = questionPool.Select(x => new DialogOptionInfo(x.questionText, x.id)).ToList();

        Game.Session.Dialog.DialogOptionSelectedEvent += OnDialogOptionSelected_Hook;
        ModInterface.Log.Message("final option count: " + options.Count);

        if (options.Any())
        {
            Game.Session.Dialog.ShowDialogOptions(options, true, true);
        }
        else
        {
            ModInterface.Log.Warning("No question options. Skipping to step 4");
            _talkStepIndex = 3;
            TalkStep();
        }
    }

    private void OnDialogOptionSelected_Hook()
    {
        Game.Session.Dialog.DialogOptionSelectedEvent -= OnDialogOptionSelected_Hook;
        TalkStep();
    }

    private void OnDialogLineComplete_Hook(UiDoll doll)
    {
        doll.DialogLineCompleteEvent -= OnDialogLineComplete_Hook;
        TalkStep();
    }

    private void OnCutsceneComplete_Hook()
    {
        Game.Session.Cutscenes.CutsceneCompleteEvent -= OnCutsceneComplete_Hook;
        TalkStep();
    }
}
