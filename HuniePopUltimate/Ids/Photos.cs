using Hp2BaseMod;

namespace HuniePopUltimate;

public static class Photos
{
    public static RelativeId ThankYou => _thankYou;
    private static readonly RelativeId _thankYou = new RelativeId(Plugin.ModId, -1);

    public static RelativeId KyuOld => _kyuOld;
    private static readonly RelativeId _kyuOld = new RelativeId(Plugin.ModId, 0);

    public static RelativeId Audrey10th => _audrey10th;
    private static readonly RelativeId _audrey10th = new RelativeId(Plugin.ModId, 1);

    internal static int Count = 2;

    internal const int HpDacPromoPhotoBase = 100_000;
    internal static int HpDacPromoPhotoCount = 0;

    internal const int HpDacProfilePhotoBase = 101_000;
    internal static int HpDacProfilePhotoCount = 0;

    internal const int HpDacArtTestPhotoBase = 102_000;
    internal static int HpDacArtTestPhotoCount = 0;

    internal const int HpDacGuestPiecePhotoBase = 103_000;
    internal static int HpDacGuestPiecePhotoCount = 0;

    internal const int HpDacThankYouPhotoBase = 104_000;
    internal static int HpDacThankYouPhotoCount = 0;

    internal const int HpDacKsRewardPhotoBase = 105_000;
    internal static int HpDacKsRewardPhotoCount = 0;

    internal static (int,int)[] BonusIdRanges => [
        (HpDacPromoPhotoBase, HpDacPromoPhotoCount),
        (HpDacProfilePhotoBase, HpDacProfilePhotoCount),
        (HpDacArtTestPhotoBase, HpDacArtTestPhotoCount),
        (HpDacGuestPiecePhotoBase, HpDacGuestPiecePhotoCount),
        (HpDacThankYouPhotoBase, HpDacThankYouPhotoCount),
        (HpDacKsRewardPhotoBase, HpDacKsRewardPhotoCount),
    ];
}
