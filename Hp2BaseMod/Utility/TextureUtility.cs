// Hp2BaseMod 2022, By OneSuchKeeper

using System.IO;
using System.Linq;
using UnityEngine;

namespace Hp2BaseMod.Utility
{
    public static class TextureUtility
    {
        public static Sprite SpriteFromPng(string path, bool readOnly)
        {
            var texture = LoadFromPng(path, readOnly);
            return texture == null
                ? null
                : TextureToSprite(texture, Vector2.zero);
        }

        public static Texture2D LoadFromPng(string path, bool readOnly)
        {
            if (!File.Exists(path))
            {
                ModInterface.Log.Warning($"Failed to load png file {path}");
                return null;
            }

            return LoadFromBytes(File.ReadAllBytes(path), readOnly);
        }

        public static Texture2D LoadFromBytes(byte[] bytes, bool readOnly)
        {
            var texture = new Texture2D(2, 2);
            texture.LoadImage(bytes, readOnly);

            return texture;
        }

        public static Texture2D LoadFromRaw(byte[] bytes, int width, int height, TextureFormat format, bool readOnly)
        {
            var texture = new Texture2D(width, height, format, false);
            texture.LoadRawTextureData(bytes);
            texture.Apply(false, readOnly);
            return texture;
        }

        public static Texture2D Empty() => new Texture2D(1, 1, TextureFormat.ARGB32, false);

        public static Sprite TextureToSprite(Texture2D texture, Vector2 pivot) => Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height),
            pivot);

        private static Material _uiDefaultMat
        {
            get
            {
                if (x_uiDefaultMat == null)
                {
                    x_uiDefaultMat = ModInterface.Assets.GetInternalAsset<Material>("UIDefault");
                }
                return x_uiDefaultMat;
            }
        }
        private static Material x_uiDefaultMat;

        public static Texture2D RenderSpriteToTexture(Sprite sprite, bool readOnly)
        {
            var mesh = new Mesh
            {
                bounds = sprite.bounds,
                vertices = sprite.vertices.Select(v => new Vector3(v.x, v.y, 0)).ToArray(),
                normals = Enumerable.Repeat(Vector3.back, sprite.vertices.Length).ToArray(),
                uv = sprite.uv,
                triangles = sprite.triangles.Select(t => (int)t).ToArray()
            };

            int width = (int)sprite.rect.width;
            int height = (int)sprite.rect.height;

            var renderTexture = RenderTexture.GetTemporary(width, height);
            var old = RenderTexture.active;
            RenderTexture.active = renderTexture;

            GL.Clear(true, true, Color.clear);

            var mat = _uiDefaultMat;
            mat.mainTexture = sprite.texture;

            // Center the sprite based on its pivot within texture space
            var translation = new Vector3(
                width * 0.5f - sprite.pivot.x,
                height * 0.5f - sprite.pivot.y,
                0f
            );
            var matrix = Matrix4x4.TRS(translation, Quaternion.identity, Vector3.one);

            Graphics.DrawMeshNow(mesh, matrix);

            var texture = new Texture2D(width, height, sprite.texture.format, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply(false, readOnly);

            RenderTexture.active = old;
            RenderTexture.ReleaseTemporary(renderTexture);

            return texture;
        }

        public static Texture2D Duplicate(Texture2D source)
        {
            var dupe = new Texture2D(source.width, source.height, source.format, false);
            dupe.SetPixels(source.GetPixels());
            dupe.wrapMode = source.wrapMode;
            dupe.filterMode = source.filterMode;
            dupe.Apply();
            return dupe;
        }
    }
}
