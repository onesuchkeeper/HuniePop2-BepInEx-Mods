using System;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

public class SpriteInfoInternal : IGameDefinitionInfo<Sprite>
{
    private string _spriteName;

    public SpriteInfoInternal(Sprite def, AssetProvider assetProvider)
    {
        if (def == null) { return; }

        assetProvider.NameAndAddAsset(ref _spriteName, def);
    }

    public SpriteInfoInternal(string spriteName)
    {
        if (string.IsNullOrWhiteSpace(spriteName))
        {
            throw new ArgumentException(nameof(spriteName));
        }

        _spriteName = spriteName;
    }

    public void SetData(ref Sprite def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle)
    {
        def = assetProvider.GetInternalAsset<Sprite>(_spriteName);
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        assetProvider.RequestInternal(typeof(Sprite), _spriteName);
    }
}
