using UnityEngine;

namespace Hp2BaseMod.Extension;

public static partial class GameObject_Ext
{
    public static void Destroy(this GameObject gameObject)
    {
        GameObject.Destroy(gameObject);
    }

    public static void Destroy(this Transform transform)
    {
        GameObject.Destroy(transform.gameObject);
    }

    public static void Destroy(this MonoBehaviour monoBehaviour)
    {
        GameObject.Destroy(monoBehaviour.gameObject);
    }
}