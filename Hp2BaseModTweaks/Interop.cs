using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;

namespace Hp2BaseModTweaks;

/// <summary>
/// All Inter mod value registrations are placed here for easy reference. 
/// <see cref="ModInterface.RegisterInterModValue"/> <see cref="ModInterface.TryGetInterModValue"/>
/// </summary>
internal static class Interop
{
    internal static void RegisterInterModValues()
    {
        ModInterface.RegisterInterModValue(Plugin.ModId, "AddModCredit",
            (string logoPath, IEnumerable<(string creditButtonPath, string creditButtonOverPath, string redirectLink)> creditEntries)
                => Plugin.ModCredits.Add(new CreditEntry(logoPath, creditEntries.Select(x => new CreditMember(x.creditButtonPath, x.creditButtonOverPath, x.redirectLink)))));

        ModInterface.RegisterInterModValue(Plugin.ModId, "AddLogoPath", (Action<string>)Plugin.LogoPaths.Add);
    }
}
