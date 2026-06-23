using Hp2BaseMod;

namespace HuniePopUltimate;

/// <summary>
/// Holds all item ids in one place to ensure they are unique. Defined statically
/// so they don't accidentally change between versions
/// </summary>
public static class Items
{
    public sealed class GirlItemIds(int baseId)
    {
        public RelativeId Baggage1 => _baggage1;
        private RelativeId _baggage1 = new RelativeId(Plugin.ModId, baseId);
        public RelativeId Baggage2 => _baggage2;
        private RelativeId _baggage2 = new RelativeId(Plugin.ModId, baseId + 1);
        public RelativeId Baggage3 => _baggage3;
        private RelativeId _baggage3 = new RelativeId(Plugin.ModId, baseId + 2);

        public bool IsBaggageItem(RelativeId id) => id == _baggage1 
            || id == _baggage2
            || id == _baggage3;
        
        public RelativeId Shoe1 => _shoe1;
        private RelativeId _shoe1 = new RelativeId(Plugin.ModId, baseId + 3);
        public RelativeId Shoe2 => _shoe2;
        private RelativeId _shoe2 = new RelativeId(Plugin.ModId, baseId + 4);
        public RelativeId Shoe3 => _shoe3;
        private RelativeId _shoe3 = new RelativeId(Plugin.ModId, baseId + 5);
        public RelativeId Shoe4 => _shoe4;
        private RelativeId _shoe4 = new RelativeId(Plugin.ModId, baseId + 6);

        public bool IsShoeItem(RelativeId id) => id == _shoe1 
            || id == _shoe2
            || id == _shoe3
            || id == _shoe4;

        public RelativeId Unique1 => _unique1;
        private RelativeId _unique1 = new RelativeId(Plugin.ModId, baseId + 7);
        public RelativeId Unique2 => _unique2;
        private RelativeId _unique2 = new RelativeId(Plugin.ModId, baseId + 8);
        public RelativeId Unique3 => _unique3;
        private RelativeId _unique3 = new RelativeId(Plugin.ModId, baseId + 9);
        public RelativeId Unique4 => _unique4;
        private RelativeId _unique4 = new RelativeId(Plugin.ModId, baseId + 10);

        public bool IsUniqueItem(RelativeId id) => id == _unique1 
            || id == _unique2
            || id == _unique3
            || id == _unique4;
    }

    public static RelativeId WeirdThing => _weirdThing;
    private static readonly RelativeId _weirdThing = new RelativeId(Plugin.ModId, 0);

    public static readonly GirlItemIds Aiko = new(100_000);
    public static readonly GirlItemIds Audrey = new(101_000);
    public static readonly GirlItemIds Beli = new(102_000);
    public static readonly GirlItemIds Celeste = new(103_000);
    public static readonly GirlItemIds Kyanna = new(104_000);
    public static readonly GirlItemIds Kyu = new(105_000);
    public static readonly GirlItemIds Momo = new(106_000);
    public static readonly GirlItemIds Nikki = new(107_000);
    public static readonly GirlItemIds Tiffany = new(108_000);
    public static readonly GirlItemIds Venus = new(109_000);
}
