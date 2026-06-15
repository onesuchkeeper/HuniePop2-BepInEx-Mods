using System;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace HuniePopUltimate;

public class TextureInfoRasterized : ITextureInfo
{
    public TextureWrapMode WrapMode => _wrapMode;
    private readonly TextureWrapMode _wrapMode;

    internal static Shader _shader;
    private readonly ITextureInfo _source;
    private readonly Vector2[] _verts;
    private readonly Vector2[] _uvs;
    private readonly int[] _triangles;

    private readonly int _width;
    private readonly int _height;

    private readonly FilterMode _filterMode;
    private Texture2D _texture;

    public TextureInfoRasterized(
        ITextureInfo source,
        int width,
        int height,
        Vector2[] verts,
        Vector2[] uvs,
        int[] triangles,
        FilterMode filterMode = FilterMode.Bilinear,
        TextureWrapMode wrapMode = TextureWrapMode.Clamp)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _verts = verts ?? throw new ArgumentNullException(nameof(verts));
        _uvs = uvs ?? throw new ArgumentNullException(nameof(uvs));
        _triangles = triangles ?? throw new ArgumentNullException(nameof(triangles));

        _width = width;
        _height = height;

        _filterMode = filterMode;
        _wrapMode = wrapMode;
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        _source.RequestInternals(assetProvider);
    }

    public Texture2D GetTexture()
    {
        if (_texture != null)
            return _texture;

        BuildTexture();
        return _texture;
    }

    private void BuildTexture()
    {
        var material = new Material(_shader);
        var sourceTex = _source.GetTexture();

        // sourceTex.wrapMode = TextureWrapMode.Clamp;
        // sourceTex.filterMode = FilterMode.Bilinear;

        var rt = new RenderTexture(_width, _height, 0, RenderTextureFormat.ARGB32)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = _filterMode,
            useMipMap = false
        };
        rt.Create();

        RenderTexture.active = rt;

        GL.Clear(true, true, Color.clear);

        material.SetTexture("_MainTex", sourceTex);
        material.SetPass(0);

        GL.PushMatrix();
        GL.LoadOrtho(); // normalized 0–1 space

        GL.Begin(GL.TRIANGLES);

        for (int i = 0; i < _triangles.Length; i++)
        {
            int idx = _triangles[i];

            var pos = _verts[idx];
            var uv = _uvs[idx];

            // Convert pixel space → normalized 0–1
            float nx = pos.x / _width;
            float ny = pos.y / _height;

            GL.TexCoord2(uv.x, uv.y);
            GL.Vertex3(nx, ny, 0);
        }

        GL.End();

        GL.PopMatrix();

        _texture = new Texture2D(_width, _height, TextureFormat.ARGB32, false);
        _texture.ReadPixels(new Rect(0, 0, _width, _height), 0, 0);
        _texture.Apply();

        _texture.wrapMode = _wrapMode;
        _texture.filterMode = _filterMode;

        RenderTexture.active = null;
        rt.Release();
    }
}