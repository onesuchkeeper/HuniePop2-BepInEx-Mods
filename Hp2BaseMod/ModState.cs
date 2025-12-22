namespace Hp2BaseMod;

/// <summary>
/// Values for controlling some basic game functions.
/// </summary>
public class ModState
{
    /// <summary>
    /// The number of smoothies to attempt to populate in the hub.
    /// </summary>
    public int MaxStoreSmoothies = 4;

    /// <summary>
    /// The number of favorite question options displayed when asking.
    /// </summary>
    public int FavQuestionOptionCount = 3;

    /// <summary>
    /// If the cellphone ui should appear on the left, same position as in the hub.
    /// </summary>
    public bool CellphoneOnLeft;

    /// <summary>
    /// The prefab for the photos window
    /// </summary>
    public UiWindow UiWindowPhotos;

    /// <summary>
    /// The prefab for the photos window
    /// </summary>
    public UiWindow KyuButtWindow;

    /// <summary>
    /// The prefab for the photos window
    /// </summary>
    public UiWindow ItemNotifierWindow;
}
