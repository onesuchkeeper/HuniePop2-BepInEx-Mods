// Hp2BaseMod 2022, By OneSuchKeeper

using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hp2BaseMod.Utility
{
    public static class TextureUtility
    {
        public static Sprite SpriteFromPath(string path) => TextureToSprite(LoadFromPath(path), Vector2.zero);

        public static Texture2D LoadFromPath(string path) => LoadFromBytes(File.ReadAllBytes(path), TextureFormat.ARGB32);

        public static Texture2D LoadFromBytes(byte[] bytes, TextureFormat format)
        {
            var texture = new Texture2D(2, 2, format, false);
            texture.LoadImage(bytes);

            return texture;
        }

        public static Texture2D LoadFromRaw(byte[] bytes, int width, int height, TextureFormat format)
        {
            var texture = new Texture2D(width, height, format, false);
            texture.LoadRawTextureData(bytes);
            texture.Apply();

            return texture;
        }

        public static Texture2D Empty() => new Texture2D(0, 0, TextureFormat.ARGB32, false);

        public static Sprite TextureToSprite(Texture2D texture, Vector2 pivot) => Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height),
            pivot);

        private static Material _uiDefaultMat
        {
            get
            {
                if (x_uiDefaultMat == null)
                {
                    foreach (var mat in Resources.FindObjectsOfTypeAll<Material>())
                    {
                        if (mat.name == "UIDefault")
                        {
                            x_uiDefaultMat = mat;
                        }
                    }
                }
                return x_uiDefaultMat;
            }
        }
        private static Material x_uiDefaultMat;

        public static Texture2D RenderSpriteToTexture(Sprite sprite)
        {
            var mesh = new Mesh();
            mesh.bounds = sprite.bounds;
            mesh.vertices = sprite.vertices.Select(x => new Vector3(x.x, x.y, 0)).ToArray();
            mesh.normals = Enumerable.Repeat(Vector3.back, mesh.vertices.Length).ToArray();
            mesh.uv = sprite.uv;
            mesh.triangles = sprite.triangles.Select(x => (int)x).ToArray();

            var texWidth = (int)sprite.rect.width;
            var texHeight = (int)sprite.rect.height;

            var radW = sprite.rect.width / 2;
            var radH = sprite.rect.height / 2;

            var renderTexture = RenderTexture.GetTemporary(texWidth, texHeight);

            var buffer = new CommandBuffer();
            buffer.SetRenderTarget(renderTexture);
            buffer.DrawMesh(mesh, Matrix4x4.Ortho(-radW, radW, -radH, radH, -1, 1), _uiDefaultMat, -1, 0);

            var texture = new Texture2D(texWidth, texHeight);
            texture.ReadPixels(new Rect(0, 0, texWidth, texHeight), 0, 0);
            texture.Apply();

            RenderTexture.ReleaseTemporary(renderTexture);

            return texture;
        }

        private static WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

        public static IEnumerable Foo()
        {
            yield return endOfFrame;


        }

        public static Texture2D Duplicate(Texture2D texture2D)
        {
            var dupe = new Texture2D(texture2D.width, texture2D.height);
            dupe.SetPixels(texture2D.GetPixels());
            dupe.Apply();
            return dupe;
        }
    }
}
