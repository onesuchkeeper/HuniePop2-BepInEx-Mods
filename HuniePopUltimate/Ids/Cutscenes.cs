using Hp2BaseMod;

namespace HuniePopUltimate;

public static class Cutscenes
{
    public class PairCutscenes(int baseId)
    {
        public RelativeId Compatible => _compatible;
        private readonly RelativeId _compatible = new RelativeId(Plugin.ModId, baseId + 0);

        public RelativeId Attracted => _attracted;
        private readonly RelativeId _attracted = new RelativeId(Plugin.ModId, baseId + 1);

        public RelativeId Lovers => _lovers;
        private readonly RelativeId _lovers = new RelativeId(Plugin.ModId, baseId + 2);

        public RelativeId PostBonusRound => _postSex;
        private readonly RelativeId _postSex = new RelativeId(Plugin.ModId, baseId + 3);
    }

    public static RelativeId PreSex => _preSex;
    private static readonly RelativeId _preSex = new RelativeId(Plugin.ModId, 0);

    public static RelativeId PostSex => _postSex;
    private static readonly RelativeId _postSex = new RelativeId(Plugin.ModId, 1);

    public static RelativeId SuccessAttracted => _successAttracted;
    private static readonly RelativeId _successAttracted = new RelativeId(Plugin.ModId, 2);

    public static RelativeId BonusRoundSuccess => _bonusRoundSuccess;
    private static readonly RelativeId _bonusRoundSuccess = new RelativeId(Plugin.ModId, 3);

    public static RelativeId AudreyTestBaggage => _ashleyTestBaggage;
    private static readonly RelativeId _ashleyTestBaggage = new RelativeId(Plugin.ModId, 4);

    public static RelativeId TiffanyBaggageMommyIssues => _tiffanyBaggageMommyIssues;
    private static readonly RelativeId _tiffanyBaggageMommyIssues = new RelativeId(Plugin.ModId, 5);

    internal static int NextCutsceneId = 6;
    private static int NextDialogLineLocal = 0;
    internal static RelativeId NextDialogLineId => new RelativeId(Plugin.ModId, NextDialogLineLocal++);

    public static PairCutscenes TiffanyAudrey = new PairCutscenes(100_000);
}
