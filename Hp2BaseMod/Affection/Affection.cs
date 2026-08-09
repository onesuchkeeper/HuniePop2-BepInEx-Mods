using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

public class Affection : IAffection
{
    ///<inheritdoc/>
    public RelativeId Id => _id;
    private readonly RelativeId _id;

    ///<inheritdoc/>
    public string Name => _name;
    private string _name;

    ///<inheritdoc/>
    public bool HasFruit => _fruitIds.Length > 0;

    private readonly RelativeId[] _fruitIds;
    public Affection(RelativeId id, string name, RelativeId[] fruitIds)
    {
        _id = id;
        _name = name;
        _fruitIds = fruitIds;
    }

    public ItemDefinition GetRandomFruit() => ModInterface.GameData.GetItem(_fruitIds.GetRandom());
}
