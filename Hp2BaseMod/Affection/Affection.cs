// Hp2BaseMod 2026, By OneSuchKeeper

using System;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

/// <summary>
/// Basic implementation of <see cref="IAffection"/>
/// </summary>
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

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id">The affection's Id</param>
    /// <param name="name">The name of the affection</param>
    /// <param name="fruitIds">Ids of fruit items, used for <see cref="GetRandomFruit"/> .</param>
    public Affection(RelativeId id, string name, RelativeId[] fruitIds)
    {
        _id = id;
        _name = name;
        _fruitIds = fruitIds ?? [];
    }

    ///<inheritdoc/>
    public ItemDefinition GetRandomFruit() => HasFruit 
        ? ModInterface.GameData.GetItem(_fruitIds.GetRandom()) 
        : throw new Exception($"Affection {_name} does not have any fruits");
}
