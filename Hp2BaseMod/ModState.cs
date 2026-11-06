namespace Hp2BaseMod;

/// <summary>
/// Values for controlling some basic game functions.
/// </summary>
public class ModState
{
    /// <summary>
    /// The number of favorite question options displayed when asking.
    /// </summary>
    public int FavQuestionOptionCount = 3;

    /// <summary>
    /// If the cellphone ui should appear on the left, same position as in the hub.
    /// </summary>
    public bool CellphoneOnLeft;

    /// <summary>
    /// Multiplies the speed at which you get rewards after the date (the fruit)
    /// </summary>
    public float RewardSpeedMult = 1f;
}
