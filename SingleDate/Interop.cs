using System;
using System.Collections.Generic;
using Hp2BaseMod;
using UnityEngine;

namespace SingleDate;

/// <summary>
/// All Inter mod value registrations are placed here for easy reference. 
/// <see cref="ModInterface.RegisterInterModValue"/> <see cref="ModInterface.TryGetInterModValue"/>
/// </summary>
internal static class Interop
{
    internal static void RegisterInterModValues()
    {
        //Swaps charms between the girls with the given ids
        ModInterface.RegisterInterModValue(State.ModId, nameof(UiPrefabs.SwapCharms), (Action<RelativeId, RelativeId>)UiPrefabs.SwapCharms);

        //Swaps photos between the girls with the given ids
        ModInterface.RegisterInterModValue(State.ModId, nameof(Plugin.SwapPhotos), (Action<RelativeId, RelativeId>)Plugin.Instance.SwapPhotos);

        //Sets the charm for the girl with the given id
        ModInterface.RegisterInterModValue(State.ModId, nameof(UiPrefabs.SetCharmSprite), (Action<RelativeId, Sprite>)UiPrefabs.SetCharmSprite);

        //Adds date photos paired with their min affection level percentage (float 0-1) to be added for a girl
        ModInterface.RegisterInterModValue(State.ModId, nameof(Plugin.AddGirlDatePhotos), (Action<RelativeId, IEnumerable<(RelativeId, float)>>)Plugin.Instance.AddGirlDatePhotos);

        //Adds sex photos paired with their location id
        ModInterface.RegisterInterModValue(State.ModId, nameof(Plugin.AddGirlSexPhotos), (Action<RelativeId, IEnumerable<RelativeId>>)Plugin.Instance.AddGirlSexPhotos);
    }
}
