namespace Hp2BaseMod.Utility;

public static partial class GameDataLogUtility
{
    public static void Log (this GirlExpressionSubDefinition expression, ILogger logger = null) => LogExpression(expression, logger);

    public static void LogExpression(GirlExpressionSubDefinition expression, ILogger logger = null)
    {
        logger ??= ModInterface.Log;
        if (expression == null)
        {
            logger.Message("null");
            return;
        }

        using (logger.MakeIndent($"Expression Type: {expression.expressionType}"))
        {
            logger.Message($"Eyebrows Index: {expression.partIndexEyebrows}");

            logger.Message($"Eyes Index: {expression.partIndexEyes}, "
                + $"Eyes Glow Index: {expression.partIndexEyesGlow}, "
                + $"Eyes Closed: {expression.eyesClosed} ");

            logger.Message($"Mouth Open: {expression.mouthOpen}"
                + $"Mouth Closed Index: {expression.partIndexMouthClosed}");
        }
    }

    public static void Log (this GirlOutfitSubDefinition outfit) => LogOutfit(outfit);

    public static void LogOutfit(GirlOutfitSubDefinition outfit, ILogger logger = null)
    {
        logger ??= ModInterface.Log;

        if (outfit == null)
        {
            logger.Message("null");
            return;
        }

        using (logger.MakeIndent($"Outfit: {outfit.outfitName}"))
        {
            logger.Message($"Part index: {outfit.partIndexOutfit}, Hide Nipples: {outfit.hideNipples}");
            if (outfit.tightlyPaired) logger.Message($"Paired hairstyle index: {outfit.pairHairstyleIndex}");
        }
    }

    public static void Log (this GirlHairstyleSubDefinition hairstyle) => LogHairstyle(hairstyle);

    public static void LogHairstyle(GirlHairstyleSubDefinition hairstyle, ILogger logger = null)
    {
        logger ??= ModInterface.Log;
        
        if (hairstyle == null)
        {
            logger.Message("null");
            return;
        }

        using (logger.MakeIndent($"Hairstyle: {hairstyle.hairstyleName}"))
        {
            logger.Message($"Part index front: {hairstyle.partIndexFronthair}, "
                + "Part index back: {hairstyle.partIndexFronthair}");

            if (hairstyle.tightlyPaired) logger.Message($"Paired outfit index: {hairstyle.pairOutfitIndex}");

            logger.Message($"Hide Specials: {hairstyle.hideSpecials}");
        }
    }
}