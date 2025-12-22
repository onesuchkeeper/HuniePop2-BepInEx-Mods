using System;
using AssetStudio.Extractor;
using Hp2BaseMod;

namespace HuniePopUltimate;

public static class LocationIds
{
    //date locs
    public static RelativeId BedRoom => _bedRoom;
    private static RelativeId _bedRoom = new RelativeId(Plugin.ModId, 22);

    public static RelativeId Park => _park;
    private static RelativeId _park = new RelativeId(Plugin.ModId, 2);

    public static RelativeId Cafe => _cafe;
    private static RelativeId _cafe = new RelativeId(Plugin.ModId, 3);

    public static RelativeId Beach => _beach;
    private static RelativeId _beach = new RelativeId(Plugin.ModId, 4);

    public static RelativeId Bar => _bar;
    private static RelativeId _bar = new RelativeId(Plugin.ModId, 5);

    public static RelativeId Carnival => _carnival;
    private static RelativeId _carnival = new RelativeId(Plugin.ModId, 6);

    public static RelativeId Campus => _campus;
    private static RelativeId _campus = new RelativeId(Plugin.ModId, 7);

    public static RelativeId Gym => _gym;
    private static RelativeId _gym = new RelativeId(Plugin.ModId, 8);

    public static RelativeId Mall => _mall;
    private static RelativeId _mall = new RelativeId(Plugin.ModId, 9);

    public static RelativeId FarmersMarket => _farmersMarket;
    private static RelativeId _farmersMarket = new RelativeId(Plugin.ModId, 10);

    public static RelativeId NightClub => _nightClub;
    private static RelativeId _nightClub = new RelativeId(Plugin.ModId, 11);

    public static RelativeId Casino => _casino;
    private static RelativeId _casino = new RelativeId(Plugin.ModId, 12);

    public static RelativeId OutdoorLounge => _outdoorLounge;
    private static RelativeId _outdoorLounge = new RelativeId(Plugin.ModId, 13);

    public static RelativeId BotanicalGarden => _botanicalGarden;
    private static RelativeId _botanicalGarden = new RelativeId(Plugin.ModId, 14);

    public static RelativeId HotSprings => _hotSprings;
    private static RelativeId _hotSprings = new RelativeId(Plugin.ModId, 15);

    public static RelativeId HikingTrail => _hikingTrail;
    private static RelativeId _hikingTrail = new RelativeId(Plugin.ModId, 16);

    public static RelativeId IceRink => _iceRink;
    private static RelativeId _iceRink = new RelativeId(Plugin.ModId, 17);

    public static RelativeId WaterPark => _waterPark;
    private static RelativeId _waterPark = new RelativeId(Plugin.ModId, 18);

    public static RelativeId TennisCourts => _tennisCourts;
    private static RelativeId _tennisCourts = new RelativeId(Plugin.ModId, 19);

    public static RelativeId ScenicOverlook => _scenicOverlook;
    private static RelativeId _scenicOverlook = new RelativeId(Plugin.ModId, 20);

    public static RelativeId Restaurant => _restaurant;
    private static RelativeId _restaurant = new RelativeId(Plugin.ModId, 21);

    public static RelativeId BedRoomDate => _bedRoomDate;
    private static RelativeId _bedRoomDate = new RelativeId(Plugin.ModId, 1);

    public static RelativeId FromHp1Id(int hp1LocId)
    {
        if (hp1LocId < 1 || hp1LocId > 22)
        {
            throw new ArgumentOutOfRangeException();
        }

        return new RelativeId(Plugin.ModId, hp1LocId);
    }

    public static RelativeId FromUnityPath(UnityAssetPath path)
    {
        if (path.FileId != 0) return RelativeId.Default;

        switch (path.PathId)
        {
            case 9290: return Park;
            case 9289: return NightClub;
            case 9288: return Mall;
            case 9287: return Gym;
            case 9286: return Campus;
            case 9285: return Cafe;
            case 9284: return Beach;
            case 9283: return Bar;
            case 9282: return BedRoom;
            case 9281: return WaterPark;
            case 9279: return ScenicOverlook;
            case 9278: return OutdoorLounge;
            case 9277: return IceRink;
            case 9276: return HotSprings;
            case 9275: return HikingTrail;
            case 9274: return FarmersMarket;
            case 9273: return Restaurant;
            case 9272: return Casino;
            case 9271: return Carnival;
            case 9270: return BotanicalGarden;
        }

        return RelativeId.Default;
    }
}
