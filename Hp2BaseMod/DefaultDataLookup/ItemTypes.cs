// Hp2BaseMod 2025, By OneSuchKeeper

using System;

namespace Hp2BaseMod;

public static class ItemTypes
{
    public readonly static RelativeId Shoe = new RelativeId(-1, 0);
    public readonly static RelativeId Unique = new RelativeId(-1, 1);
    public readonly static RelativeId DateGift = new RelativeId(-1, 2);
    public readonly static RelativeId Food = new RelativeId(-1, 3);
    public readonly static RelativeId StaminaFood = new RelativeId(-1, 4);
    public readonly static RelativeId Smoothie = new RelativeId(-1, 5);

    internal static string GetDisplayName(ItemType itemType)
    {
        throw new NotImplementedException();
    }
}