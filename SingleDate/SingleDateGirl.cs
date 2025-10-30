using System.Collections.Generic;
using Hp2BaseMod;
using SingleDate;
using UnityEngine;

public class SingleDateGirl
{
    public List<SexPhotoData> SexPhotos = new();

    public List<DatePhotoData> DatePhotos = new();

    public Sprite CharmSprite;
    //charm behaviors
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
