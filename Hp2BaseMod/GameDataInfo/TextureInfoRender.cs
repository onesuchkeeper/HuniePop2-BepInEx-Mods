using System;
using System.Collections.Generic;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

public class TextureInfoRender : ITextureInfo
{
    private ITextureInfo _decorated;
    private IEnumerable<ITextureRenderStep> _renderSteps;
    private bool _readOnly;

    private Texture2D _texture;

    public TextureInfoRender(ITextureInfo decorated, bool readOnly, IEnumerable<ITextureRenderStep> renderSteps)
    {
        _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
        _renderSteps = renderSteps ?? throw new ArgumentNullException(nameof(renderSteps));
        _readOnly = readOnly;
    }

    public Texture2D GetTexture()
    {
        if (_texture == null)
        {
            _texture = TextureUtility.Duplicate(_decorated.GetTexture());
            _renderSteps.ForEach(x => x.Apply(ref _texture));
            _texture.Apply(false, _readOnly);
        }

        return _texture;
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        //noop
    }
}