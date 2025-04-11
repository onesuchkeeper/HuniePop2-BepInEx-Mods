using System;
using System.Collections.Generic;

namespace Hp2BaseModTweaks
{
    [Serializable]
    public class ModConfig
    {
        public string ModImagePath;
        public List<CreditsEntry> CreditsEntries;
        public List<string> LogoImages;
    }

    [Serializable]
    public class CreditsEntry
    {
        public string CreditButtonImagePath;
        public string CreditButtonImageOverPath;
        public string RedirectLink;
    }
}
