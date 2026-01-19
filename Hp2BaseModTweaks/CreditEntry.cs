using System.Collections.Generic;

namespace Hp2BaseModTweaks;

public class CreditEntry
{
    public string LogoPath { get; private set; }
    public IEnumerable<CreditMember> Members { get; private set; }

    public CreditEntry(string logoPath, IEnumerable<CreditMember> members)
    {
        LogoPath = logoPath;
        Members = members;
    }
}

public class CreditMember
{
    public string ButtonPath { get; private set; }
    public string ButtonOverPath { get; private set; }
    public string ButtonLink { get; private set; }

    public CreditMember(string buttonPath, string buttonOverPath, string buttonLink)
    {
        ButtonPath = buttonPath;
        ButtonOverPath = buttonOverPath;
        ButtonLink = buttonLink;
    }
}