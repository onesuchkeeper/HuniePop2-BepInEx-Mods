using System;

namespace Hp2BaseMod.GameDataInfo.Interface;

public interface IHerQuestionDataInfo
{
    public void SetData(GirlQuestionSubDefinition def,
        DialogLine questionLine,
        IdIndexMap answerIdToIndex,
        Func<(DialogLine responseLine, int responseIndex)> getGoodResponse,
        Func<RelativeId, (DialogLine responseLine, int responseIndex)> getBadResponse,
        GameDefinitionProvider gameDefinitionProvider,
        AssetProvider assetProvider);
}
