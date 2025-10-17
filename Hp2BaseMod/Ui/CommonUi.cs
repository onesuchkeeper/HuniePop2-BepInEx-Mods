using UnityEngine;

namespace Hp2BaseMod.Ui;

public static class CommonUi
{
    public static RectTransform MakeFullRect()
    {
        var go = new GameObject();
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);

        return rt;
    }
}