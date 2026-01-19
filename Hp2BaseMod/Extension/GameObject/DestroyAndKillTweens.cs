using DG.Tweening;
using UnityEngine;

namespace Hp2BaseMod.Extension;

public static partial class GameObject_Ext
{
    public static void DestroyAndKillTweens(this GameObject gameObject)
    {
        DOTween.KillAll(gameObject);
        GameObject.Destroy(gameObject);
    }

    public static void DestroyAndKillTweens(this Transform transform)
    {
        DOTween.KillAll(transform.gameObject);
        GameObject.Destroy(transform.gameObject);
    }

    public static void DestroyAndKillTweens(this MonoBehaviour monoBehaviour)
    {
        DOTween.KillAll(monoBehaviour.gameObject);
        GameObject.Destroy(monoBehaviour.gameObject);
    }
}