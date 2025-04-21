using UnityEngine;

/// <summary>
/// Holds unity engine objects to use as prefabs for alternate single date ui
/// </summary>
public static class UiPrefabs
{
    public static UiWindow SingleDateBubbles => _singleBubbles;
    private static UiWindowActionBubbles _singleBubbles;

    public static UiWindow DefaultDateBubbles => _defaultBubbles;
    private static UiWindow _defaultBubbles;

    public static void InitActionBubbles(UiWindow actionBubblesWindow)
    {
        _defaultBubbles = actionBubblesWindow;

        var delta = Game.Session.gameCanvas.header.xValues.y - Game.Session.gameCanvas.header.xValues.x;
        _singleBubbles = (UiWindowActionBubbles)Object.Instantiate(actionBubblesWindow);

        UiActionBubble badTalkBubble = null;
        UiActionBubble dateBubble = null;
        UiActionBubble goodTalkBubble = null;
        foreach (var bubble in _singleBubbles.actionBubbles)
        {
            switch (bubble.actionBubbleType)
            {
                case ActionBubbleType.TALK:
                    if (bubble.actionBubbleValue == 0)
                    {
                        badTalkBubble = bubble;
                    }
                    else
                    {
                        goodTalkBubble = bubble;
                    }
                    break;
                case ActionBubbleType.DATE:
                    dateBubble = bubble;
                    break;
            }

            bubble.transform.position = new Vector3(bubble.transform.position.x + delta, bubble.transform.position.y, bubble.transform.position.z);
        }

        if (badTalkBubble != null && dateBubble != null)
        {
            dateBubble.transform.position = new Vector3(badTalkBubble.transform.position.x + 48,
                badTalkBubble.transform.position.y + 32f);

            var padding_GO = new GameObject($"{dateBubble.name}Padding");
            padding_GO.transform.SetParent(dateBubble.transform.parent, false);
            var padding_RectTransform = padding_GO.AddComponent<RectTransform>();
            var ratio = 29f / 33f;
            padding_RectTransform.localScale = new Vector3(ratio, ratio, 1f);

            dateBubble.transform.SetParent(padding_RectTransform, true);
        }

        if (goodTalkBubble != null)
        {
            goodTalkBubble.transform.position = new Vector3(goodTalkBubble.transform.position.x - 48,
                goodTalkBubble.transform.position.y + 32f);
        }

        if (badTalkBubble != null)
        {
            badTalkBubble.transform.SetParent(null);
            UnityEngine.Object.Destroy(badTalkBubble);
            _singleBubbles.actionBubbles.Remove(badTalkBubble);
        }
    }
}
