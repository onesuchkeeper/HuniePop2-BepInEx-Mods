using System;
using System.Collections.Generic;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

public class TextureInfoExternal : ITextureInfo
{
    private readonly string _path;
    private readonly FilterMode _filter = FilterMode.Bilinear;
    private readonly IEnumerable<ITextureRenderStep> _renderSteps;
    private readonly bool _readOnly;

    private Texture2D _texture;

    public TextureInfoExternal(string filePath, bool readOnly, FilterMode filter = FilterMode.Bilinear, IEnumerable<ITextureRenderStep> renderSteps = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(nameof(filePath));
        }

        _path = filePath;
        _filter = filter;
        _renderSteps = renderSteps;
        _readOnly = readOnly;
    }

    public Texture2D GetTexture()
    {
        if (_texture == null)
        {
            _texture = TextureUtility.LoadFromPng(_path, _readOnly);
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