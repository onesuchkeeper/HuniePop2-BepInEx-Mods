using UnityEngine;

namespace Hp2BaseMod.GameDataInfo.Interface;

public interface IExpInfo
{
    /// <summary>
    /// Percentage to exp out of max
    /// </summary>
    public float Percentage { get; }

    /// <summary>
    /// Current level of the stat
    /// </summary>
    public int CurrentLevel { get; }

    /// <summary>
    /// Maximum level of the stat
    /// </summary>
    public int MaxLevel { get; }

    /// <summary>
    /// Item used to display exp information to the user. It's "ItemDescription" will be ignored in favor or <see cref="ExpDesc"/>
    /// </summary>
    public ItemDefinition ExpItemDef { get; }

    /// <summary>
    /// Item used to display level information to the user. It's "ItemDescription" will be ignored in favor or <see cref="PlateDesc"/>
    /// </summary>
    public ItemDefinition LevelPlateItemDef { get; }

    /// <summary>
    /// Used for populating the shop with smoothies if not at max level
    /// </summary>
    public ItemDefinition SmoothieItemDef { get; }

    /// <summary>
    /// Name used for level plate. I.E ""
    /// </summary>
    public string PlateTitle { get; }

    /// <summary>
    /// Level description taking place of <see cref="LevelPlateItemDef"/>'s ItemDescription property
    /// </summary>
    public string PlateDesc { get; }

    /// <summary>
    /// 
    /// </summary>
    public string ExpTitle { get; }

    /// <summary>
    /// Level description taking place of <see cref="ExpItemDef"/>'s ItemDescription property
    /// </summary>
    public string ExpDesc { get; }

    /// <summary>
    /// Icon image used for the level plate
    /// </summary>
    public Sprite IconImage { get; }

    public Sprite MeterFront { get; }
    public Sprite PlateImage { get; }

    /// <summary>
    /// Id of the affection type this exp is for, or default if not associated with an affection type.
    /// </summary>
    RelativeId AffectionType { get; }

    Color32 OutlineColor { get; }
    Color32 TextColor { get; }
}
