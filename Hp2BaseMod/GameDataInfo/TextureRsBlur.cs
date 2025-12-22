using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

public class TextureRsBlur : ITextureRenderStep
{
    private int _blurRadius { get; set; } = 3;

    public TextureRsBlur(int blurRad = 3)
    {
        _blurRadius = blurRad;
    }

    public void Apply(ref Texture2D target)
    {
        //init the final texture
        var texture = new Texture2D(target.width, target.height);
        texture.SetPixels(Enumerable.Repeat(Color.clear, texture.width * texture.height).ToArray());

        //get original pixels for sampling
        var sourcePixels = target.GetPixels();
        var width = target.width;
        var height = target.height;

        //precalculate Gaussian weights (1D kernel)
        var weights = CalculateGaussianKernel(_blurRadius);

        //create temporary buffer for horizontal pass
        var tempPixels = new Color[width * height];

        //PASS 1: Horizontal blur
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color weightedSum = Color.clear;
                float totalWeight = 0f;

                for (int k = -_blurRadius; k <= _blurRadius; k++)
                {
                    int sampleX = Mathf.Clamp(x + k, 0, width - 1);
                    float weight = weights[k + _blurRadius];

                    weightedSum += sourcePixels[y * width + sampleX] * weight;
                    totalWeight += weight;
                }

                tempPixels[y * width + x] = weightedSum / totalWeight;
            }
        }

        //PASS 2: Vertical blur (output to final array)
        var outputPixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color weightedSum = Color.clear;
                float totalWeight = 0f;

                for (int k = -_blurRadius; k <= _blurRadius; k++)
                {
                    int sampleY = Mathf.Clamp(y + k, 0, height - 1);
                    float weight = weights[k + _blurRadius];

                    weightedSum += tempPixels[sampleY * width + x] * weight;
                    totalWeight += weight;
                }

                outputPixels[y * width + x] = weightedSum / totalWeight;
            }
        }

        //set all pixels at once for better performance
        texture.SetPixels(outputPixels);

        UnityEngine.Object.Destroy(target);
        target = texture;
        target.Apply();
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        //noop
    }

    /// <summary>
    /// Precalculates 1D Gaussian kernel weights
    /// </summary>
    private float[] CalculateGaussianKernel(int radius)
    {
        int kernelSize = radius * 2 + 1;
        float[] kernel = new float[kernelSize];
        float sigma = radius / 3f;
        float twoSigmaSquared = 2f * sigma * sigma;

        for (int i = 0; i < kernelSize; i++)
        {
            int offset = i - radius;
            kernel[i] = Mathf.Exp(-(offset * offset) / twoSigmaSquared);
        }

        return kernel;
    }
}