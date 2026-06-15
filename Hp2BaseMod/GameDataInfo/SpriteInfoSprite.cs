using System;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

public class SpriteInfoSprite : IGameDefinitionInfo<Sprite>
{
    private readonly Sprite _sprite;
    public SpriteInfoSprite(Sprite sprite)
    {
        _sprite = sprite ?? throw new ArgumentNullException(nameof(sprite));
    }

    public void SetData(ref Sprite def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle)
    {
        def = _sprite;
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        //noop
    }
}