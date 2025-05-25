using UnityEngine;

namespace Hp2BaseMod.GameDataInfo.Interface;

public interface ITextureRenderStep
{
    void Apply(ref Texture2D target);
}