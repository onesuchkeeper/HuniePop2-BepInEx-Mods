using System;
using System.Collections.Generic;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

public class TextureInfoExternal : ITextureInfo
{
    private string _path;
    private FilterMode _filter = FilterMode.Bilinear;
    private IEnumerable<ITextureRenderStep> _renderSteps;
    private Texture2D _texture;

    public TextureInfoExternal(string filePath, FilterMode filter = FilterMode.Bilinear, IEnumerable<ITextureRenderStep> renderSteps = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(nameof(filePath));
        }

        _path = filePath;
        _filter = filter;
        _renderSteps = renderSteps;
    }

    public Texture2D GetTexture()
    {
        if (_texture == null)
        {
            _texture = TextureUtility.LoadFromPath(_path);
            _texture.filterMode = _filter;

            if (_renderSteps != null)
            {
                _renderSteps?.ForEach(x => x.Apply(ref _texture));
            }
        }

        return _texture;
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        //noop
    }
}