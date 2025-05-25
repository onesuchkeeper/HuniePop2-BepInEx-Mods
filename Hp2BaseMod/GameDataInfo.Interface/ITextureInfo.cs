using UnityEngine;

namespace Hp2BaseMod.GameDataInfo.Interface;

public interface ITextureInfo
{
    Texture2D GetTexture();

    /// <summary>
    /// Allows the mod an opportunity to request internal assets from the assetProvider 
    /// which will be available during <see cref="SetData"/> via <see cref="AssetProvider.GetInternalAsset"/>
    /// </summary>
    void RequestInternals(AssetProvider assetProvider);
}