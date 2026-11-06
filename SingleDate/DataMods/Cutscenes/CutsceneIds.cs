using Hp2BaseMod;
namespace SingleDate;

/// <summary>
/// Ids of the cutscenes this mod adds, used by the single pairs (see the *Cutscene.cs files). All ids are local to this mod.
/// </summary>
public static class CutsceneIds
{
    /// <summary>Plays when meeting the girl on a single date.</summary>
    public static RelativeId Meeting => _meeting;
    private static RelativeId _meeting = new RelativeId(State.ModId, 0);

    /// <summary>Plays when the pair becomes attracted.</summary>
    public static RelativeId Attract => _attract;
    private static RelativeId _attract = new RelativeId(State.ModId, 1);

    /// <summary>Plays before the sex date.</summary>
    public static RelativeId PreSex => _preSex;
    private static RelativeId _preSex = new RelativeId(State.ModId, 2);

    /// <summary>Plays after the sex date.</summary>
    public static RelativeId PostSex => _postSex;
    private static RelativeId _postSex = new RelativeId(State.ModId, 3);

    /// <summary>Plays when a single date is won.</summary>
    public static RelativeId Success => _success;
    private static RelativeId _success = new RelativeId(State.ModId, 4);

    /// <summary>Plays when the sex date (bonus round) is won.</summary>
    public static RelativeId BonusSuccess => _bonusSuccess;
    private static RelativeId _bonusSuccess = new RelativeId(State.ModId, 5);
}
