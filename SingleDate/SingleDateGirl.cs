using System.Collections.Generic;
using Hp2BaseMod;
using UnityEngine;

namespace SingleDate;

public class SingleDateGirl
{
    public List<SexPhotoData> SexPhotos = new();

    public List<DatePhotoData> DatePhotos = new();

    public Sprite CharmSprite;
    //charm behaviors

    public HashSet<RelativeId> SexLocBlackList = new();

    public RelativeId CutsceneSuccessAttracted = RelativeId.Default;
    public RelativeId CutsceneSuccessBonus = RelativeId.Default;
}

public struct DatePhotoData
{
    public RelativeId PhotoId;
    public float RelationshipPercentage;
}

public struct SexPhotoData
{
    public RelativeId PhotoId;
    public RelativeId LocationId;
}
