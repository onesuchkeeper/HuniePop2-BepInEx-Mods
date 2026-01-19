using System;

namespace Hp2BaseMod.GameDataInfo;

public class HerQuestionAnswerInfo
{
    public string text;
    public string altText;
    public DialogLineDataMod Response;

    internal void SetData(GirlQuestionAnswerSubDefinition def,
        Func<(DialogLine responseLine, int responseIndex)> getResponse,
        GameDefinitionProvider definitionProvider,
        AssetProvider assetProvider)
    {
        if (text != null)
        {
            def.answerText = text;
        }

        if (altText != null)
        {
            def.answerTextAlt = altText;
            def.hasAlt = true;
        }

        if (Response != null)
        {
            (var responseLine, var responseIndex) = getResponse();
            Response.SetData(responseLine, definitionProvider, assetProvider);
            def.responseIndex = responseIndex;
        }
    }
}
