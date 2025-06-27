// Hp2BaseMod 2025, By OneSuchKeeper

namespace Hp2BaseMod;
public static class FruitTypes
{
    public readonly static RelativeId Talent = new RelativeId(-1, 0);
    public readonly static RelativeId Flirtation = new RelativeId(-1, 1);
    public readonly static RelativeId Romance = new RelativeId(-1, 2);
    public readonly static RelativeId Sexuality = new RelativeId(-1, 3);
    public static bool IsDefaultAffection(RelativeId relativeId)
        => relativeId == Talent || relativeId == Flirtation || relativeId == Romance || relativeId == Sexuality;
}