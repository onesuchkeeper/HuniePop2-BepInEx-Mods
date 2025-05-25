using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

public class TextureRsTransform : ITextureRenderStep
{
    private bool _flipX;
    private bool _flipY;
    private bool _transpose;

    public TextureRsTransform(bool flipX, bool flipY, bool transpose)
    {
        _flipX = flipX;
        _flipY = flipY;
        _transpose = transpose;
    }

    public void Apply(ref Texture2D target)
    {
        if (!(_flipX || _flipY || _transpose))
        {
            return;
        }

        var transformedTexture = new Texture2D(target.width, target.height);

        if (_flipX)
        {
            if (_flipY)
            {
                if (_transpose)
                {
                    for (int i = 0; i < target.width; i++)
                    {
                        for (int j = 0; j < target.height; j++)
                        {
                            transformedTexture.SetPixel(i, j, target.GetPixel(target.height - j, target.width - i));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < target.width; i++)
                    {
                        for (int j = 0; j < target.height; j++)
                        {
                            transformedTexture.SetPixel(i, j, target.GetPixel(target.width - i, target.height - j));
                        }
                    }
                }
            }
            else
            {
                if (_transpose)
                {
                    for (int i = 0; i < target.width; i++)
                    {
                        for (int j = 0; j < target.height; j++)
                        {
                            transformedTexture.SetPixel(i, j, target.GetPixel(j, target.width - i));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < target.width; i++)
                    {
                        for (int j = 0; j < target.height; j++)
                        {
                            transformedTexture.SetPixel(i, j, target.GetPixel(target.width - i, j));
                        }
                    }
                }
            }
        }
        else
        {
            if (_flipY)
            {
                if (_transpose)
                {
                    for (int i = 0; i < target.width; i++)
                    {
                        for (int j = 0; j < target.height; j++)
                        {
                            transformedTexture.SetPixel(i, j, target.GetPixel(target.height - j, i));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < target.width; i++)
                    {
                        for (int j = 0; j < target.height; j++)
                        {
                            transformedTexture.SetPixel(i, j, target.GetPixel(i, target.height - j));
                        }
                    }
                }
            }
            else
            {
                if (_transpose)
                {
                    for (int i = 0; i < target.width; i++)
                    {
                        for (int j = 0; j < target.height; j++)
                        {
                            transformedTexture.SetPixel(i, j, target.GetPixel(j, i));
                        }
                    }
                }
            }
        }

        UnityEngine.Object.Destroy(target);
        target = transformedTexture;
        target.Apply();
    }
}
