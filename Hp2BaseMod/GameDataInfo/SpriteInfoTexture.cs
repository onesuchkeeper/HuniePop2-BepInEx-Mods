using System;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using Unity.Collections;
using UnityEngine;
using UnityEngine.U2D;

namespace Hp2BaseMod.GameDataInfo
{
    public class SpriteInfoTexture : IGameDefinitionInfo<Sprite>
    {
        private ITextureInfo _textureInfo;
        public Rect? _rect;
        private Vector2[] _vertices;
        private Vector2[] _uvs;
        private ushort[] _triangles;

        private Sprite _sprite;

        public SpriteInfoTexture(ITextureInfo texture,
            Rect? spriteRect = null,
            Vector2[] vertices = null,
            Vector2[] uvs = null,
            ushort[] triangles = null)
        {
            _textureInfo = texture ?? throw new ArgumentNullException(nameof(texture));
            _rect = spriteRect;
            _vertices = vertices;
            _uvs = uvs;
            _triangles = triangles;
        }

        public void SetData(ref Sprite def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            def = GetSprite();
        }

        public Sprite GetSprite()
        {
            if (_sprite == null)
            {
                var overrideGeometry = _vertices != null || _triangles != null;
                var texture = _textureInfo.GetTexture();

                _sprite = Sprite.Create(texture,
                    _rect ?? new Rect(0, 0, texture.width, texture.height),
                    Vector2.zero,
                    100,
                    0,
                    overrideGeometry ? SpriteMeshType.Tight : SpriteMeshType.FullRect,
                    Vector4.zero,
                    false);

                if (overrideGeometry)
                {
                    _sprite.OverrideGeometry(_vertices ?? _sprite.vertices, _triangles ?? _sprite.triangles);
                }

                if (_uvs != null)
                {
                    using (var uvs = new NativeArray<Vector2>(_uvs, Allocator.Temp))
                    {
                        _sprite.SetVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord0, uvs);
                    }
                }
            }

            return _sprite;
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            _textureInfo.RequestInternals(assetProvider);
        }
    }
}
