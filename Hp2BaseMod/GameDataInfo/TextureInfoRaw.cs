using System;
using System.Collections.Generic;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

public class TextureInfoRaw : ITextureInfo
{
    private byte[] _data;
    private TextureFormat _format = TextureFormat.ARGB32;
    private int _width;
    private int _height;
    private IEnumerable<ITextureRenderStep> _renderSteps;
    private FilterMode _filterMode;
    private TextureWrapMode _wrapMode;

    private Texture2D _texture;

    public TextureInfoRaw(int width, int height, byte[] data, TextureFormat format, FilterMode filterMode, TextureWrapMode wrapMode, IEnumerable<ITextureRenderStep> renderSteps)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _width = width;
        _height = height;
        _format = format;
        _filterMode = filterMode;
        _wrapMode = wrapMode;
        _renderSteps = renderSteps;
    }

    public Texture2D GetTexture()
    {
        if (_texture == null)
        {
            _texture = TextureUtility.LoadFromRaw(_data, _width, _height, _format);
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