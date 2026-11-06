// Hp2BaseMod 2026, By OneSuchKeeper

namespace Hp2BaseMod;

/// <summary>
/// Used as resource value in puzzles and as a preferance for girls
/// </summary>
public interface IAffection
{
    /// <summary>
    /// Serializable id of the affection resource instance.
    /// </summary>
    RelativeId Id {get;}

    /// <summary>
    /// Name of the affection displayed to the user.
    /// </summary>
    string Name {get;}
    
    /// <summary>
    /// If this affection has fruits associated with it.
    /// </summary>
    bool HasFruit { get; }

    /// <summary>
    /// Gets a random fruit item associated with this affection.
    /// </summary>
    /// <returns></returns>
    ItemDefinition GetRandomFruit();
}