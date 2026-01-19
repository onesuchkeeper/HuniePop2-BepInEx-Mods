// Hp2BaseMod 2025, By OneSuchKeeper

using System;
using System.Collections.Generic;

namespace Hp2BaseMod;

/// <summary>
/// Args used in <see cref="ModEvents.FinderSlotsPopulate"/>.
/// </summary>
public class FinderSlotPopulateEventArgs : EventArgs
{
    /// <summary>
    /// 1st priority, will be populated at their defined sex location
    /// </summary>
    public List<PlayerFileGirlPair> SexPool;

    /// <summary>
    /// 2nd priority, will be populated at their defined meeting location
    /// </summary>
    public List<PlayerFileGirlPair> IntroPool;

    /// <summary>
    /// 3rd priority, will be populated at their defined meeting location
    /// </summary>
    public List<PlayerFileGirlPair> MeetingPool;

    /// <summary>
    /// 4th priority
    /// </summary>
    public List<PlayerFileGirlPair> CompatiblePool;

    /// <summary>
    /// 5th priority
    /// </summary>
    public List<PlayerFileGirlPair> LoversPool;

    /// <summary>
    /// 6th priority
    /// </summary>
    public List<PlayerFileGirlPair> AttractedPool;

    /// <summary>
    /// Locations to populate at
    /// </summary>
    public List<LocationDefinition> LocationPool;

    /// <summary>
    /// Removes all pairs with the given girl from the pools
    /// </summary>
    public void RemoveGirlFromAllPools(RelativeId girlId)
    {
        var runtimeId = ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId);

        bool PairHasGirl(PlayerFileGirlPair pair) => pair.girlPairDefinition.girlDefinitionOne.id == runtimeId || pair.girlPairDefinition.girlDefinitionTwo.id == runtimeId;

        SexPool.RemoveAll(PairHasGirl);
        IntroPool.RemoveAll(PairHasGirl);
        MeetingPool.RemoveAll(PairHasGirl);
        CompatiblePool.RemoveAll(PairHasGirl);
        LoversPool.RemoveAll(PairHasGirl);
        AttractedPool.RemoveAll(PairHasGirl);
    }
}
