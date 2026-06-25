using System;
using UnityEngine;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod;

namespace HuniePopUltimate;

public class TextureInfoCenterMirrored : ITextureInfo
{
    internal static Material _material;

    public TextureWrapMode WrapMode => _source.WrapMode;

    private Texture2D _texture;

    private readonly ITextureInfo _source;
    private readonly int _targetWidth;
    private readonly int _targetHeight;

    public TextureInfoCenterMirrored(
        ITextureInfo source,
        int targetWidth,
        int targetHeight)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _targetWidth = targetWidth;
        _targetHeight = targetHeight;
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        _source.RequestInternals(assetProvider);
    }

    public Texture2D GetTexture()
    {
        if (_texture != null) return _texture;

        var baseTex = _source.GetTexture();

        var rtFinal = RenderTexture.GetTemporary(
            _targetWidth,
            _targetHeight,
            0,
            RenderTextureFormat.ARGB32);

        var sourceWidth = baseTex.width;
        var targetWidth = _targetWidth;

        var centerStart = (targetWidth - sourceWidth) / (2f * targetWidth);
        var centerEnd = centerStart + (sourceWidth / (float)targetWidth);

        _material.SetFloat("_CenterStart", centerStart);
        _material.SetFloat("_CenterEnd", centerEnd);

        Graphics.Blit(baseTex, rtFinal, _material);

        RenderTexture.active = rtFinal;

        _texture = new Texture2D(
            _targetWidth,
            _targetHeight,
            TextureFormat.ARGB32,
            false);

        _texture.ReadPixels(
            new Rect(0, 0, _targetWidth, _targetHeight),
            0,
            0);

        _texture.Apply();

        RenderTexture.active = null;

        RenderTexture.ReleaseTemporary(rtFinal);

        return _texture;
    }
}