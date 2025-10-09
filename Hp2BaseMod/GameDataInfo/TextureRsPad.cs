using System.Linq;
using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

public class TextureRsPad : ITextureRenderStep
{
    private int _left;
    private int _right;
    private int _top;
    private int _bottom;
    private Color _color;

    public TextureRsPad(int allSides)
        : this(allSides, allSides, allSides, allSides, Color.clear)
    {

    }

    public TextureRsPad(int allSides, Color color)
        : this(allSides, allSides, allSides, allSides, color)
    {

    }

    public TextureRsPad(int horizontal, int vertical)
    : this(horizontal, horizontal, vertical, vertical, Color.clear)
    {

    }

    public TextureRsPad(int horizontal, int vertical, Color color)
    : this(horizontal, horizontal, vertical, vertical, color)
    {

    }

    public TextureRsPad(int left, int right, int top, int bottom)
    : this(left, right, top, bottom, Color.clear)
    {

    }

    public TextureRsPad(int left, int right, int top, int bottom, Color color)
    {
        _left = left;
        _right = right;
        _top = top;
        _bottom = bottom;
        _color = color;
    }

    public void Apply(ref Texture2D target)
    {
        var paddedTexture = new Texture2D(target.width + _left + _right, target.height + _top + _bottom);
        var paddedColors = Enumerable.Repeat(_color, paddedTexture.width * paddedTexture.height).ToArray();
        var targetColors = target.GetPixels();

        for (int i = 0; i < target.width; i++)
        {
            for (int j = 0; j < target.height; j++)
            {
                var x = i + _left;
                var y = j + _bottom;

                if (x >= 0 && y >= 0)
                {
                    paddedColors[(y * paddedTexture.width) + x] = targetColors[(j * target.width) + i];
                }
            }
        }

        paddedTexture.SetPixels(paddedColors);

        UnityEngine.Object.Destroy(target);
        target = paddedTexture;
        target.Apply();
    }
}
