using System.Collections.Generic;
using Hp2BaseMod;
using UnityEngine;

namespace SingleDate;

/// <summary>
/// The single date configuration of one girl, which other mods fill in through the interop methods in
/// <see cref="Plugin"/>. Not saved, it is rebuilt every launch.
/// </summary>
public class SingleDateGirl
{
    /// <summary>Photos shown for the sex date, and where each one takes place.</summary>
    public List<SexPhotoData> SexPhotos = new();

    /// <summary>Photos shown after successful dates, unlocked as her level increases.</summary>
    public List<DatePhotoData> DatePhotos = new();

    /// <summary>The charm shown with the girl in the single date status app, null to use the default charm.</summary>
    public Sprite CharmSprite;
    // (planned: per girl charm behaviors, for now animations are chosen by CharmAnimationRegistry)

    /// <summary>Locations that can't be randomly picked for her sex date.</summary>
    public HashSet<RelativeId> SexLocBlackList = new();

    /// <summary>Cutscene for a successful date that levels her up to attracted. Not read by anything yet.</summary>
    public RelativeId CutsceneSuccessAttracted = RelativeId.Default;
    /// <summary>Cutscene for a successful sex date. Not read by anything yet.</summary>
    public RelativeId CutsceneSuccessBonus = RelativeId.Default;
}

/// <summary>A photo that can be shown after a date once the girl has reached a level.</summary>
public struct DatePhotoData
{
    public RelativeId PhotoId;
    /// <summary>Her level as a fraction (0 to 1) of the max single date level that unlocks the photo.</summary>
    public float RelationshipPercentage;
}

/// <summary>A sex date photo and the location it belongs to.</summary>
public struct SexPhotoData
{
    public RelativeId PhotoId;
    /// <summary>Where the photo's date takes place, or <see cref="RelativeId.Default"/> to use the pair's sex location.</summary>
    public RelativeId LocationId;
}
