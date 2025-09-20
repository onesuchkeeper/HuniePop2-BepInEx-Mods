using System;
using Hp2BaseMod;

namespace SingleDate;

/// <summary>
/// All Inter mod value registrations are placed here for easy reference. 
/// <see cref="ModInterface.RegisterInterModValue"/> <see cref="ModInterface.TryGetInterModValue"/>
/// </summary>
internal static class Interop
{
    internal static void RegisterInterModValues()
    {
        ModInterface.RegisterInterModValue(State.ModId, nameof(UiPrefabs.SwapCharms), (Action<RelativeId, RelativeId>)UiPrefabs.SwapCharms);
    }
}
