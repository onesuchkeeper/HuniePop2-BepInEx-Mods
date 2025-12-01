using Hp2BaseMod;

public static partial class GameDataLogUtility
{
    public static void LogExpression(GirlExpressionSubDefinition expression)
    {
        if (expression == null)
        {
            ModInterface.Log.LogInfo("null");
            return;
        }

        using (ModInterface.Log.MakeIndent($"Expression Type: {expression.expressionType}"))
        {
            ModInterface.Log.LogInfo($"Eyebrows Index: {expression.partIndexEyebrows}");

            ModInterface.Log.LogInfo($"Eyes Index: {expression.partIndexEyes}, "
                + $"Eyes Glow Index: {expression.partIndexEyesGlow}, "
                + $"Eyes Closed: {expression.eyesClosed} ");

            ModInterface.Log.LogInfo($"Mouth Open: {expression.mouthOpen}"
                + $"Mouth Closed Index: {expression.partIndexMouthClosed}");
        }
    }

    public static void LogOutfit(GirlOutfitSubDefinition outfit)
    {
        if (outfit == null)
        {
            ModInterface.Log.LogInfo("null");
            return;
        }

        using (ModInterface.Log.MakeIndent($"Outfit: {outfit.outfitName}"))
        {
            ModInterface.Log.LogInfo($"Part index: {outfit.partIndexOutfit}, Hide Nipples: {outfit.hideNipples}");
            if (outfit.tightlyPaired) ModInterface.Log.LogInfo($"Paired hairstyle index: {outfit.pairHairstyleIndex}");
        }
    }

    public static void LogHairstyle(GirlHairstyleSubDefinition hairstyle)
    {
        if (hairstyle == null)
        {
            ModInterface.Log.LogInfo("null");
            return;
        }

        using (ModInterface.Log.MakeIndent($"Hairstyle: {hairstyle.hairstyleName}"))
        {
            ModInterface.Log.LogInfo($"Part index front: {hairstyle.partIndexFronthair}, "
                + "Part index back: {hairstyle.partIndexFronthair}");

            if (hairstyle.tightlyPaired) ModInterface.Log.LogInfo($"Paired outfit index: {hairstyle.pairOutfitIndex}");

            ModInterface.Log.LogInfo($"Hide Specials: {hairstyle.hideSpecials}");
        }
    }
}