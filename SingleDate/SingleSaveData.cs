using System;
using System.Collections.Generic;
using UnityEngine;

namespace SingleDate;

[Serializable]
public class SingleSaveData
{
    public List<SingleSaveFile> SaveFiles;

    /// <summary>
    /// If upset hint is shown on single dates
    /// </summary>
    public bool SingleUpsetHint = false;

    /// <summary>
    /// If baggage is used on single dates
    /// </summary>
    public bool SingleDateBaggage = true;

    public bool RequireLoversBeforeThreesome = true;

    /// <summary>
    /// Relationship stages for single dates
    /// </summary>
    public int MaxSingleGirlRelationshipLevel = 3;

    public void Clean()
    {
        SaveFiles ??= new List<SingleSaveFile>();
        SaveFiles.ForEach(x => x.Clean());

        MaxSingleGirlRelationshipLevel = Mathf.Max(1, MaxSingleGirlRelationshipLevel);
    }
}
