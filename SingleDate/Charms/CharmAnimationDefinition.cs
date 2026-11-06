using System;
using System.Collections.Generic;
using DG.Tweening;
using Hp2BaseMod;

namespace SingleDate;

/// <summary>One idle animation the charm can play, and how it is chosen.</summary>
public class CharmAnimationDefinition
{
    /// <summary>How likely this is picked compared to the other valid animations.</summary>
    public float Weight;
    /// <summary>How much of the sequence's time budget this uses. Tuned by hand to roughly match the animation's length.</summary>
    public float Cost;

    /// <summary>The girls this can be used for, or null for all girls.</summary>
    public HashSet<RelativeId> AllowedCharacters;

    /// <summary>Appends the animation to the sequence.</summary>
    public Action<Sequence, CharmAnimationContext> Build;
}
