using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace HuniePopUltimate;

/// <summary>
/// Supplies the hand-authored, girl-specific data needed to turn a girl extracted
/// from HuniePop 1 into a complete HuniePop 2 girl.
/// </summary>
/// <remarks>
/// Extraction fills a girl's <see cref="GirlDataMod"/> with everything that can be read
/// from the HP1 data. A configurator describes the rest: the details HP1 doesn't
/// contain or encodes differently, and the choices that have to be made by hand.
/// There is one implementation per girl.
/// </remarks>
public interface IGirlConfigurator
{
    /// <summary>
    /// The mod object for the girl this configurator handles. Extraction populates it,
    /// and <see cref="ConfigureGirl"/> can then adjust it.
    /// </summary>
    GirlDataMod Mod {get;}

    /// <summary>
    /// Decides whether the lines for an HP1 dialog trigger should be imported for this
    /// girl, and which HP2 trigger they should be stored under.
    /// </summary>
    /// <param name="dialogTriggerId">The trigger the lines belong to.</param>
    /// <param name="cleanedDialogTriggerId">
    /// The trigger the lines should be stored under. Only meaningful when this method
    /// returns true. Return <paramref name="dialogTriggerId"/> unchanged to keep it as is.
    /// </param>
    /// <returns>True to import the lines, false to skip them for this girl.</returns>
    bool CleanDialogTrigger(RelativeId dialogTriggerId, out RelativeId cleanedDialogTriggerId);

    /// <summary>
    /// The favorite-question topics in the order HP1 encodes this girl's answers to them.
    /// </summary>
    /// <remarks>
    /// Her extracted answer lines are matched to topics by position, so this must follow
    /// the HP1 order, which varies between girls.
    /// </remarks>
    IEnumerable<RelativeId> FavQuestionOrder {get;}

    /// <summary>
    /// The HP1 asset ids of the items to extract for this girl, each paired with the mod
    /// item id it will be stored under.
    /// </summary>
    /// <remarks>
    /// The extracted items are handed to <see cref="ConfigureGirl"/> under those mod ids.
    /// </remarks>
    IEnumerable<(RelativeId, int)> ExtractItemIds {get;}

    /// <summary>
    /// The display name of this girl's underwear outfit.
    /// </summary>
    string UnderwearName {get;}

    /// <summary>
    /// The positions within each HP1 photo's list of images that hold its censored,
    /// nude and wet versions. Use -1 for a version the photos don't have.
    /// </summary>
    (int censoredIndex, int nudeIndex, int wetIndex) PhotoIndexes {get;}

    /// <summary>
    /// The outfit and hairstyle for each character shown in this girl's meeting cutscene,
    /// in the order the cutscene shows them. Null uses the Activity outfit and hairstyle
    /// for every character.
    /// </summary>
    /// <remarks>
    /// This covers every character the cutscene loads, not only this girl, so the length
    /// and order depend on how the HP1 scene is built. Characters beyond the end of the
    /// sequence also use Activity.
    /// </remarks>
    (RelativeId outfit, RelativeId hairstyle)[] MeetingCutsceneStyleSequence {get;}

    /// <summary>
    /// Whether a photo the girl sends by text is NSFW.
    /// </summary>
    /// <remarks>
    /// Photos shown after a bonus round are always NSFW, so implementations don't need
    /// to account for them.
    /// </remarks>
    /// <param name="photoIndex">The 0-based position of the photo in the girl's photo list.</param>
    bool IsTextPhotoIndexNsfw(int photoIndex);

    /// <summary>
    /// Applies this girl's hand-authored configuration once extraction has populated
    /// her body.
    /// </summary>
    /// <param name="hpBody">The body that was extracted from HP1.</param>
    /// <param name="assetBundle">The mod's asset bundle, for loading hand-made assets.</param>
    /// <param name="sprites">Sprite cache for building sprite infos from HP1 atlases.</param>
    /// <param name="audio">Audio cache for building audio from HP1 clips.</param>
    /// <param name="items">The items extracted for <see cref="ExtractItemIds"/>, by mod id.</param>
    void ConfigureGirl(
        GirlBodyDataMod hpBody,
        AssetBundle assetBundle,
        HpSpriteCache sprites,
        HpAudioCache audio,
        HpItemCache items);
}