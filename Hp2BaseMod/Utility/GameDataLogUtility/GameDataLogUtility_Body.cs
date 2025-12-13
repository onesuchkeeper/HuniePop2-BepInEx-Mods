namespace Hp2BaseMod.Utility;

public static partial class GameDataLogUtility
{
    public static void LogExpression(GirlExpressionSubDefinition expression)
    {
        if (expression == null)
        {
            ModInterface.Log.Message("null");
            return;
        }

        using (ModInterface.Log.MakeIndent($"Expression Type: {expression.expressionType}"))
        {
            ModInterface.Log.Message($"Eyebrows Index: {expression.partIndexEyebrows}");

            ModInterface.Log.Message($"Eyes Index: {expression.partIndexEyes}, "
                + $"Eyes Glow Index: {expression.partIndexEyesGlow}, "
                + $"Eyes Closed: {expression.eyesClosed} ");

            ModInterface.Log.Message($"Mouth Open: {expression.mouthOpen}"
                + $"Mouth Closed Index: {expression.partIndexMouthClosed}");
        }
    }

    public static void LogOutfit(GirlOutfitSubDefinition outfit)
    {
        if (outfit == null)
        {
            ModInterface.Log.Message("null");
            return;
        }

        using (ModInterface.Log.MakeIndent($"Outfit: {outfit.outfitName}"))
        {
            ModInterface.Log.Message($"Part index: {outfit.partIndexOutfit}, Hide Nipples: {outfit.hideNipples}");
            if (outfit.tightlyPaired) ModInterface.Log.Message($"Paired hairstyle index: {outfit.pairHairstyleIndex}");
        }
    }

    public static void LogHairstyle(GirlHairstyleSubDefinition hairstyle)
    {
        if (hairstyle == null)
        {
            ModInterface.Log.Message("null");
            return;
        }

        using (ModInterface.Log.MakeIndent($"Hairstyle: {hairstyle.hairstyleName}"))
        {
            ModInterface.Log.Message($"Part index front: {hairstyle.partIndexFronthair}, "
                + "Part index back: {hairstyle.partIndexFronthair}");

            if (hairstyle.tightlyPaired) ModInterface.Log.Message($"Paired outfit index: {hairstyle.pairOutfitIndex}");

            ModInterface.Log.Message($"Hide Specials: {hairstyle.hideSpecials}");
        }
    }
}