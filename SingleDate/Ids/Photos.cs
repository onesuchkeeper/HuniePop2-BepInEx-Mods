using Hp2BaseMod;

namespace SingleDate;

public static class Photos
{
    public static RelativeId DefaultSex => _defaultSex;
    private static readonly RelativeId _defaultSex = new RelativeId(Plugin.ModId, -1);

    public static RelativeId AbiaSex => _abiaSex;
    private static readonly RelativeId _abiaSex = new RelativeId(Plugin.ModId, 0);
    
    public static RelativeId BrookeSex => _brookeSex;
    private static readonly RelativeId _brookeSex = new RelativeId(Plugin.ModId, 1);

    public static RelativeId CandaceSex => _candaceSex;
    private static readonly RelativeId _candaceSex = new RelativeId(Plugin.ModId, 2);

    public static RelativeId LailaniSex => _lailaniSex;
    private static readonly RelativeId _lailaniSex = new RelativeId(Plugin.ModId, 3);

    public static RelativeId LillianSex => _lillianSex;
    private static readonly RelativeId _lillianSex = new RelativeId(Plugin.ModId, 4);

    public static RelativeId NoraSex => _noraSex;
    private static readonly RelativeId _noraSex = new RelativeId(Plugin.ModId, 5);

    public static RelativeId PollySex => _pollySex;
    private static readonly RelativeId _pollySex = new RelativeId(Plugin.ModId, 6);

    public static RelativeId SarahSex => _sarahSex;
    private static readonly RelativeId _sarahSex = new RelativeId(Plugin.ModId, 7);

    public static RelativeId ZoeySex => _zoeySex;
    private static readonly RelativeId _zoeySex = new RelativeId(Plugin.ModId, 8);
}
