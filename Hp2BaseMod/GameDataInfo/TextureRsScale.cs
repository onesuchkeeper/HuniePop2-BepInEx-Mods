using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

public class TextureRsScale : ITextureRenderStep
{
    private Vector2 _scale;

    public TextureRsScale(float scale)
    : this(new Vector2(scale, scale))
    {
    }

    public TextureRsScale(Vector2 scale)
    {
        _scale = scale;
    }

    public void Apply(ref Texture2D target)
    {
        int initW = target.width;
        int initH = target.height;
        int targetW;
        int targetH;
        RenderTexture renderTexture;

        //scaling down by more than 0.5 causes quality issues, so do it in steps
        int preScaleRounds = 0;
        while (_scale.x < 0.5f || _scale.y < 0.5f)
        {
            _scale = _scale * 2f;
            preScaleRounds++;
        }

        for (var i = 0; i < preScaleRounds; i++)
        {
            targetW = (int)(initW * _scale.x);
            targetH = (int)(initH * _scale.y);

            renderTexture = RenderTexture.GetTemporary(targetW, targetH);

            Graphics.Blit(target, renderTexture);

            UnityEngine.Object.Destroy(target);

            target = new Texture2D(targetW, targetH);
            target.ReadPixels(new Rect(0, 0, targetW, targetH), 0, 0);
            target.Apply();

            RenderTexture.ReleaseTemporary(renderTexture);

            _scale = _scale * 0.5f;
        }

        targetW = (int)(initW * _scale.x);
        targetH = (int)(initH * _scale.y);

        renderTexture = RenderTexture.GetTemporary(targetW, targetH);

        Graphics.Blit(target, renderTexture);

        UnityEngine.Object.Destroy(target);

        target = new Texture2D(targetW, targetH);
        target.ReadPixels(new Rect(0, 0, targetW, targetH), 0, 0);
        target.Apply();

        RenderTexture.ReleaseTemporary(renderTexture);
    }
}