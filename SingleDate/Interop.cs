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
        //Swaps data between the girls with the given ids
        ModInterface.RegisterInterModValue(State.ModId, nameof(Plugin.SwapGirls), (Action<RelativeId, RelativeId>)Plugin.Instance.SwapGirls);

        //Adds date photos paired with their min affection level percentage (float 0-1) to be added for a girl
        ModInterface.RegisterInterModValue(State.ModId, nameof(Plugin.AddGirlDatePhotos), (Action<RelativeId, IEnumerable<(RelativeId, float)>>)Plugin.Instance.AddGirlDatePhotos);

        //Sets the charm sprite for the girl with the given id
        ModInterface.RegisterInterModValue(State.ModId, nameof(Plugin.SetGirlCharm), (Action<RelativeId, Sprite>)Plugin.Instance.SetGirlCharm);

        //Adds sex photos paired with their location id
        ModInterface.RegisterInterModValue(State.ModId, nameof(Plugin.AddGirlSexPhotos), (Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>>)Plugin.Instance.AddGirlSexPhotos);

        /// Please note, any pair with <see cref="GirlNobody"/> (LocalId 0) as girl 1 will be treated as a single date pair.
        /// All Data mods are added with a priority of zero
    }
}
