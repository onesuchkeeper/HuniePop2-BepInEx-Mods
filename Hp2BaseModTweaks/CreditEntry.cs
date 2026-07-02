using System.Collections.Generic;
using UnityEngine;

namespace Hp2BaseModTweaks;

public class CreditEntry
{
    public Sprite LogoSprite { get; private set; }
    public IEnumerable<CreditMember> Members { get; private set; }

    public CreditEntry(Sprite logoSprite, IEnumerable<CreditMember> members)
    {
        LogoSprite = logoSprite;
        Members = members;
    }
}

public class CreditMember
{
    public Sprite ButtonSprite { get; private set; }
    public Sprite ButtonOverSprite { get; private set; }
    public string ButtonLink { get; private set; }

    public CreditMember(Sprite buttonSprite, Sprite buttonOverSprite, string buttonLink)
    {
        ButtonSprite = buttonSprite;
        ButtonOverSprite = buttonOverSprite;
        ButtonLink = buttonLink;
    }
}