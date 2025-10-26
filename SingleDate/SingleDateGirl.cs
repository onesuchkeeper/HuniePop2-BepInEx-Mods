using System.Collections.Generic;
using Hp2BaseMod;
using UnityEngine;

public class SingleDateGirl
{
    public List<SexPhotoData> SexPhotoIds = new();
    public List<DatePhotoData> DatePhotoIds = new();
    public RelativeId MeetingLocId;
    public RelativeId MeetingCutsceneId;
    public RelativeId AffectionCutsceneId;
    public RelativeId PreSexCutsceneId;
    public RelativeId PostSexCutsceneId;
    public Sprite CharmSprite;
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
