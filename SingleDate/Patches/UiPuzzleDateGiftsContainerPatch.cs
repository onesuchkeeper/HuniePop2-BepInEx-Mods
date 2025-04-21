using System.Collections.Generic;
using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(UiPuzzleDateGiftsContainer))]
public static class UiPuzzleDateGiftsContainerPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiPuzzleDateGiftsContainer __instance)
        => ExpandedUiPuzzleDateGiftsContainer.Get(__instance).Start();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPrefix]
    public static void OnDestroy(UiPuzzleDateGiftsContainer __instance)
        => ExpandedUiPuzzleDateGiftsContainer.Get(__instance).OnDestroy();
}

public class ExpandedUiPuzzleDateGiftsContainer
{
    private static readonly Dictionary<UiPuzzleDateGiftsContainer, ExpandedUiPuzzleDateGiftsContainer> _expansions
        = new Dictionary<UiPuzzleDateGiftsContainer, ExpandedUiPuzzleDateGiftsContainer>();

    public static ExpandedUiPuzzleDateGiftsContainer Get(UiPuzzleDateGiftsContainer uiPuzzleDateGiftsContainer)
    {
        if (!_expansions.TryGetValue(uiPuzzleDateGiftsContainer, out var expansion))
        {
            expansion = new ExpandedUiPuzzleDateGiftsContainer(uiPuzzleDateGiftsContainer);
            _expansions[uiPuzzleDateGiftsContainer] = expansion;
        }

        return expansion;
    }

    private readonly UiPuzzleDateGiftsContainer _uiPuzzleDateGiftsContainer;
    public ExpandedUiPuzzleDateGiftsContainer(UiPuzzleDateGiftsContainer uiPuzzleDateGiftsContainer)
    {
        _uiPuzzleDateGiftsContainer = uiPuzzleDateGiftsContainer;
    }

    public void Start()
    {
        foreach (var slot in _uiPuzzleDateGiftsContainer.slotsRight)
        {
            slot.itemSlot.eastOnHub = true;
        }
    }

    public void OnDestroy()
    {
        _expansions.Remove(_uiPuzzleDateGiftsContainer);
    }
}