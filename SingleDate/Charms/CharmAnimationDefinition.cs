using System;
using System.Collections.Generic;
using DG.Tweening;
using Hp2BaseMod;

namespace SingleDate;

public class CharmAnimationDefinition
{
    public float Weight;
    public float Cost;

    // null = applies to all characters
    public HashSet<RelativeId> AllowedCharacters;

    public Action<Sequence, CharmAnimationContext> Build;
}