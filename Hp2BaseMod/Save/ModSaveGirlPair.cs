using Hp2BaseMod.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hp2BaseMod.Save
{
    [Serializable]
    public class ModSaveGirlPair
    {
        public int RelationshipLevel;
        public List<RelativeId> LearnedFavs = new List<RelativeId>();
        public List<RelativeId> RecentFavQuestions = new List<RelativeId>();

        public void Strip(SaveFileGirlPair save)
        {
            RelationshipLevel = save.relationshipLevel;

            foreach (var fav in save.learnedFavs)
            {
                var id = ModInterface.Data.GetDataId(GameDataType.Question, fav);
                LearnedFavs.Add(id);
            }
            save.learnedFavs = LearnedFavs.Where(x => x.SourceId == -1).Select(x => x.LocalId).ToList();

            foreach (var question in save.recentFavQuestions)
            {
                var id = ModInterface.Data.GetDataId(GameDataType.Question, question);
                RecentFavQuestions.Add(id);
            }
            save.recentFavQuestions = LearnedFavs.Where(x => x.SourceId == -1).Select(x => x.LocalId).ToList();
        }

        public void SetData(SaveFileGirlPair save)
        {
            Inject(save);
        }

        public SaveFileGirlPair Convert(int runtimeId)
        {
            var save = new SaveFileGirlPair(runtimeId);
            save.relationshipLevel = RelationshipLevel;
            Inject(save);
            return save;
        }

        private void Inject(SaveFileGirlPair save)
        {
            foreach (var fav in LearnedFavs)
            {
                if (ModInterface.Data.TryGetRuntimeDataId(GameDataType.Question, fav, out var runtimeId))
                {
                    save.learnedFavs.Add(runtimeId);
                }
            }
            save.learnedFavs = save.learnedFavs.Distinct().ToList();

            foreach (var questions in RecentFavQuestions)
            {
                if (ModInterface.Data.TryGetRuntimeDataId(GameDataType.Question, questions, out var runtimeId))
                {
                    save.recentFavQuestions.Add(runtimeId);
                }
            }
            save.recentFavQuestions = save.recentFavQuestions.Distinct().ToList();
        }
    }
}
