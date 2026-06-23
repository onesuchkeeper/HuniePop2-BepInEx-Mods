using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace HuniePopUltimate;

/// <summary>
/// Applies hand-authored, girl-specific configuration to an already-constructed
/// <see cref="GirlDataMod"/> things like age, favorites, location style
/// overrides, dialog remaps, and girl-specific items/ailments that cannot be
/// derived from the extracted HP1 asset data.
/// </summary>
public interface IGirlConfigurator
{
    bool ExtractUniqueAcceptDialogLines {get;}

    IEnumerable<RelativeId> FavQuestionOrder {get;}

    (int censoredIndex, int nudeIndex, int wetIndex) PhotoIndexes {get;}

    /// <summary>
    /// The girl mod this configurator handles
    /// </summary>
    GirlDataMod Mod {get;}

    /// <summary>
    /// Ids of items that need to be extracted and passed to the configurator for configuring
    /// </summary>
    IEnumerable<(RelativeId, int)> ExtractItemIds {get;}

    /// <summary>
    /// The name of the girl's underwear outfit
    /// </summary>
    string UnderwearName {get;}

    /// <summary>
    /// Styles for each step of the meeting cutscene, or null to always use 'Active'
    /// </summary>
    (RelativeId outfit, RelativeId hairstyle)[] MeetingCutsceneStyleSequence {get;}

    /// <param name="girlMod">
    /// The mod object populated by extraction. The first body is guaranteed to
    /// exist and all extracted dialog lines are already present.
    /// </param>
    /// <param name="assetBundle">The mod's asset bundle for loading sprites.</param>
    /// <param name="sprites">Sprite cache for building item sprite infos from HP1 atlases.</param>
    /// <param name="audio">Audio cache for building item/ailment audio from HP1 clips.</param>
    void ConfigureGirl(
        GirlBodyDataMod hpBody,
        AssetBundle assetBundle,
        HpSpriteCache sprites,
        HpAudioCache audio,
        HpItemCache items);

    /// <summary>
    /// Checks if the text-photo at this index is nsfw and 
    /// must be censored
    /// </summary>
    bool IsPhotoIndexNsfw(int photoIndex);
}