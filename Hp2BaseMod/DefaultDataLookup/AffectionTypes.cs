namespace Hp2BaseMod;
public static class AffectionTypes
{
    public readonly static RelativeId Talent = new RelativeId(-1, 0);
    public readonly static RelativeId Flirtation = new RelativeId(-1, 1);
    public readonly static RelativeId Romance = new RelativeId(-1, 2);
    public readonly static RelativeId Sexuality = new RelativeId(-1, 3);
    public readonly static RelativeId Broken = new RelativeId(-1, 6);
    public static bool IsDefaultAffection(RelativeId relativeId)
        => relativeId == Talent || relativeId == Flirtation || relativeId == Romance || relativeId == Sexuality;
}