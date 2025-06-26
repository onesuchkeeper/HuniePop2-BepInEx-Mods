using Hp2BaseMod;

namespace ExpandedWardrobe;

internal static class Ids
{
    public static RelativeId Style1 => _style1;
    private static RelativeId _style1;

    public static RelativeId Style2 => _style2;
    private static RelativeId _style2;

    public static RelativeId Style3 => _style3;
    private static RelativeId _style3;

    public static RelativeId Style4 => _style4;
    private static RelativeId _style4;

    public static RelativeId OutfitPart1 => _outfitPart1;
    private static RelativeId _outfitPart1;

    public static RelativeId OutfitPart1Mirror => _outfitPart1Mirror;
    private static RelativeId _outfitPart1Mirror;

    public static RelativeId OutfitPart2 => _outfitPart2;
    private static RelativeId _outfitPart2;

    public static RelativeId OutfitPart3 => _outfitPart3;
    private static RelativeId _outfitPart3;

    public static RelativeId OutfitPart4 => _outfitPart4;
    private static RelativeId _outfitPart4;

    public static RelativeId FronthairPart1 => _fronthairPart1;
    private static RelativeId _fronthairPart1;

    public static RelativeId BackhairPart1 => _backhairPart1;
    private static RelativeId _backhairPart1;

    public static RelativeId FronthairPart2 => _fronthairPart2;
    private static RelativeId _fronthairPart2;

    public static RelativeId BackhairPart2 => _backhairPart2;
    private static RelativeId _backhairPart2;

    public static RelativeId FronthairPart3 => _fronthairPart3;
    private static RelativeId _fronthairPart3;

    public static RelativeId BackhairPart3 => _backhairPart3;
    private static RelativeId _backhairPart3;

    public static RelativeId FronthairPart4 => _fronthairPart4;
    private static RelativeId _fronthairPart4;

    public static RelativeId BackhairPart4 => _backhairPart4;
    private static RelativeId _backhairPart4;

    public static int ModId => _modId;
    private static int _modId;

    public static void Init()
    {
        var styleIndex = 0;
        var partIndex = 0;

        _modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);

        _style1 = new RelativeId(_modId, styleIndex++);
        _style2 = new RelativeId(_modId, styleIndex++);
        _style3 = new RelativeId(_modId, styleIndex++);
        _style4 = new RelativeId(_modId, styleIndex++);

        _outfitPart1 = new RelativeId(_modId, partIndex++);
        _outfitPart1Mirror = new RelativeId(_modId, partIndex++);
        _outfitPart2 = new RelativeId(_modId, partIndex++);
        _outfitPart3 = new RelativeId(_modId, partIndex++);
        _outfitPart4 = new RelativeId(_modId, partIndex++);

        _fronthairPart1 = new RelativeId(_modId, partIndex++);
        _fronthairPart2 = new RelativeId(_modId, partIndex++);
        _fronthairPart3 = new RelativeId(_modId, partIndex++);
        _fronthairPart4 = new RelativeId(_modId, partIndex++);

        _backhairPart1 = new RelativeId(_modId, partIndex++);
        _backhairPart2 = new RelativeId(_modId, partIndex++);
        _backhairPart3 = new RelativeId(_modId, partIndex++);
        _backhairPart4 = new RelativeId(_modId, partIndex++);
    }
}