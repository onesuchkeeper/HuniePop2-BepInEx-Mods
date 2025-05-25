using System;
using System.IO;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

public class TextureInfoCache : ITextureInfo
{
    private string _path;
    private ITextureInfo _decorated;
    private Texture2D _texture;

    public TextureInfoCache(string path, ITextureInfo decorated)
    {
        //using to throw on invalid input
        _path = Path.GetFullPath(path);
        _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
    }

    public Texture2D GetTexture()
    {
        if (_texture != null)
        {
            return _texture;
        }

        if (File.Exists(_path))
        {
            _texture = TextureUtility.LoadFromPath(_path);
            return _texture;
        }

        _texture = _decorated.GetTexture();
        File.WriteAllBytes(_path, _texture.EncodeToPNG());

        return _texture;
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        //noop
    }
}
