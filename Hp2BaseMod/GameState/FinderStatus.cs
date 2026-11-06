// Hp2BaseMod 2026, By OneSuchKeeper

namespace Hp2BaseMod;

/// <summary>
/// What the cellphone's finder app shows, as decided by the current <see cref="IGameState"/>
/// (see <see cref="IGameState.GetFinderStatus"/>).
/// </summary>
public readonly struct FinderStatus
{
    /// <summary>False when the finder is greyed out and none of its slots exist.</summary>
    public bool Available { get; }

    /// <summary>Whether the slots can be picked right now. When false the app shows the message at <see cref="MessageIndex"/>.</summary>
    public bool SlotsReady { get; }

    /// <summary>Which of the finder app's error messages explains why the slots aren't ready. Negative shows none.</summary>
    public int MessageIndex { get; }

    private FinderStatus(bool available, bool slotsReady, int messageIndex)
    {
        Available = available;
        SlotsReady = slotsReady;
        MessageIndex = messageIndex;
    }

    /// <summary>The finder is greyed out and shows its message <paramref name="messageIndex"/>.</summary>
    public static FinderStatus Unavailable(int messageIndex) => new(false, false, messageIndex);

    /// <summary>The finder is there. The slots are usable when <paramref name="slotsReady"/>, else <paramref name="messageIndex"/> is shown.</summary>
    public static FinderStatus Present(bool slotsReady, int messageIndex) => new(true, slotsReady, messageIndex);
}