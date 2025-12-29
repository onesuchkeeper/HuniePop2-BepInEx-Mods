using Hp2BaseMod;

namespace HuniePopUltimate;

public static class Items
{
    public static RelativeId WeirdThing => _weirdThing;
    private static readonly RelativeId _weirdThing = new RelativeId(Plugin.ModId, 0);

    public static RelativeId Goldfish => _goldfish;
    private static readonly RelativeId _goldfish = new RelativeId(Plugin.ModId, 1);
}
