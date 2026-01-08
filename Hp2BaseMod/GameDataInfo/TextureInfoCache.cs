using System;
using System.IO;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

public class TextureInfoCache : ITextureInfo
{
    private readonly string _path;
    private readonly ITextureInfo _decorated;
    private readonly bool _readOnly;

    private Texture2D _texture;

    public TextureInfoCache(string path, ITextureInfo decorated, bool readOnly = true)
    {
        // using to throw on invalid input
        _path = Path.GetFullPath(path);
        _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
        _readOnly = readOnly;
    }

    public Texture2D GetTexture()
    {
        if (_texture != null)
        {
            return _texture;
        }

        if (File.Exists(_path))
        {
            _texture = TextureUtility.LoadFromPng(_path, _readOnly);
            return _texture;
        }
        else
        {
            _texture = _decorated.GetTexture();
            File.WriteAllBytes(_path, _texture.EncodeToPNG());

            // According to the class structure I should duplicate the texture
            // rather than directly modifying it here, but the Cache should
            // really only be used as the owner of the decorated instance
            // and always used in its place
            _texture.Apply(false, _readOnly);
            return _texture;
        }
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        //noop
    }
}
