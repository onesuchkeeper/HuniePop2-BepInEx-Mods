using Hp2BaseMod;
using UnityEngine;

namespace SingleDate;

/// <summary>The charm being animated, passed to each animation as it adds itself to the sequence.</summary>
public class CharmAnimationContext
{
    /// <summary>The charm's transform.</summary>
    public RectTransform Transform;
    /// <summary>The direction the charm is facing, 1 or -1. Used to multiply the x scale, flipping the charm. Animations may flip it.</summary>
    public float Dir;
    /// <summary>The girl the charm belongs to.</summary>
    public RelativeId GirlId;
}
