using System.Collections.Generic;
using AssetStudio.Extractor;
using Hp2BaseMod;

namespace HuniePopUltimate;

public static class Girls
{
    public static RelativeId Tiffany => _tiffany;
    private static RelativeId _tiffany = new RelativeId(Plugin.ModId, 0);

    public static RelativeId Aiko => _aiko;
    private static RelativeId _aiko = new RelativeId(Plugin.ModId, 1);

    public static RelativeId Kyanna => _kyanna;
    private static RelativeId _kyanna = new RelativeId(Plugin.ModId, 2);

    public static RelativeId Audrey => _audrey;
    private static RelativeId _audrey = new RelativeId(Plugin.ModId, 3);

    public static RelativeId Nikki => _nikki;
    private static RelativeId _nikki = new RelativeId(Plugin.ModId, 4);

    public static RelativeId Beli => _beli;
    private static RelativeId _beli = new RelativeId(Plugin.ModId, 5);

    public static RelativeId Momo => _momo;
    private static RelativeId _momo = new RelativeId(Plugin.ModId, 6);

    public static RelativeId Celeste => _celeste;
    private static RelativeId _celeste = new RelativeId(Plugin.ModId, 7);

    public static RelativeId Venus => _venus;
    private static RelativeId _venus = new RelativeId(Plugin.ModId, 8);

    public static RelativeId FromHp1Id(int hp1Id)
    {
        switch (hp1Id)
        {
            case 1://tiffany
                return Girls.Tiffany;
            case 2://aiko
                return Girls.Aiko;
            case 3://kyanna
                return Girls.Kyanna;
            case 4://audrey
                return Girls.Audrey;
            case 5://lola
                return Hp2BaseMod.Girls.Lola;
            case 6://nikki
                return Girls.Nikki;
            case 7://jessie
                return Hp2BaseMod.Girls.Jessie;
            case 8://beli
                return Girls.Beli;
            case 9://kyu
                return Hp2BaseMod.Girls.Kyu;
            case 10://momo
                return Girls.Momo;
            case 11://celeste
                return Girls.Celeste;
            case 12://venus
                return Girls.Venus;
            default:
                ModInterface.Log.Error($"UNHANDLED Hp1 girl id {hp1Id}");
                return RelativeId.Default;
        }
    }

    public static bool IsSpecial(int hp1Id)
    {
        switch (hp1Id)
        {
            case 9://kyu
            case 10://momo
            case 11://celeste
            case 12://venus
                return true;
        }

        return false;
    }

    public static RelativeId FromUnityPath(UnityAssetPath path)
    {
        if (path.FileId != 0) return RelativeId.Default;

        switch (path.PathId)
        {
            case 9036: return Aiko;
            case 9037: return Audrey;
            case 9038: return Beli;
            case 9039: return Celeste;
            case 9040: return Hp2BaseMod.Girls.Jessie;
            case 9041: return Kyanna;
            case 9042: return Hp2BaseMod.Girls.Kyu;
            case 9043: return Hp2BaseMod.Girls.Lola;
            case 9044: return Momo;
            case 9045: return Nikki;
            case 9046: return Tiffany;
            case 9047: return Venus;
        }

        return RelativeId.Default;
    }

    private static readonly Dictionary<RelativeId, string> _idToName = new()
    {
        {Girls.Tiffany, "Tiffany"},
        {Girls.Aiko, "Aiko"},
        {Girls.Kyanna, "Kyanna"},
        {Girls.Audrey, "Audrey"},
        {Hp2BaseMod.Girls.Lola, "Lola"},
        {Girls.Nikki, "Nikki"},
        {Hp2BaseMod.Girls.Jessie, "Jessie"},
        {Girls.Beli, "Beli"},
        {Hp2BaseMod.Girls.Kyu, "Kyu"},
        {Girls.Momo, "Momo"},
        {Girls.Celeste, "Celeste"},
        {Girls.Venus, "Venus"}
    };

    public static string IdToName(RelativeId id) => _idToName.TryGetValue(id, out var name)
        ? name
        : string.Empty;
}
