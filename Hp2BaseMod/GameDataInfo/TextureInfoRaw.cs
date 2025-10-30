using System;
using System.Collections.Generic;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

public class TextureInfoRaw : ITextureInfo
{
    private readonly byte[] _data;
    private readonly TextureFormat _format = TextureFormat.ARGB32;
    private readonly int _width;
    private readonly int _height;
    private readonly IEnumerable<ITextureRenderStep> _renderSteps;
    private readonly FilterMode _filterMode;
    private readonly TextureWrapMode _wrapMode;
    private readonly bool _readOnly;

    private Texture2D _texture;

    public TextureInfoRaw(int width, int height, byte[] data, TextureFormat format, FilterMode filterMode, TextureWrapMode wrapMode, bool readOnly, IEnumerable<ITextureRenderStep> renderSteps = null)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _width = width;
        _height = height;
        _format = format;
        _filterMode = filterMode;
        _wrapMode = wrapMode;
        _renderSteps = renderSteps;
        _readOnly = readOnly;
    }

    public Texture2D GetTexture()
    {
        if (_texture == null)
        {
            _texture = TextureUtility.LoadFromRaw(_data, _width, _height, _format, _readOnly);
            _texture.filterMode = _filterMode;
            _texture.wrapMode = _wrapMode;

            if (_renderSteps != null)
            {
                _renderSteps.ForEach(x => x.Apply(ref _texture));
            }
        }

        return _texture;
    }

    public Vector2 GetSize() => new Vector2(_width, _height);

    public void RequestInternals(AssetProvider assetProvider)
    {
        //noop
    }
}