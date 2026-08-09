namespace Hp2BaseMod;

public interface IAffection
{
    /// <summary>
    /// Serializable id of the affection resource instance
    /// </summary>
    RelativeId Id {get;}

    /// <summary>
    /// Name of the affection displayed to the user.
    /// </summary>
    string Name {get;}
    
    bool HasFruit { get; }

    ItemDefinition GetRandomFruit();
}