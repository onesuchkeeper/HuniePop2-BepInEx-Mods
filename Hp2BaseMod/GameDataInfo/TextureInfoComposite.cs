using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace Hp2BaseMod;

public class TextureRsComposite : ITextureInfo
{
    private IEnumerable<(ITextureInfo texture, Vector2 offset)> _textures;
    private Vector2Int _size;
    private Texture2D _texture;

    public Texture2D GetTexture()
    {
        if (_texture != null)
        {
            return _texture;
        }

        _texture = new Texture2D(_size.x, _size.y);
        _texture.SetPixels(Enumerable.Repeat(Color.clear, _texture.width * _texture.height).ToArray());

        var renderTexture = RenderTexture.GetTemporary(_size.x, _size.y);

        foreach (var texture in _textures)
        {
            Graphics.Blit(texture.texture.GetTexture(), renderTexture, Vector2.one, texture.offset);
        }
        _texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

        RenderTexture.ReleaseTemporary(renderTexture);

        _texture.Apply();
        return _texture;
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        _textures.ForEach(x => x.texture.RequestInternals(assetProvider));
    }
}