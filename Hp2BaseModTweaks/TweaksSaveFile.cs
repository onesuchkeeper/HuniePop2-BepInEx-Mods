using System;
using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.Extension;

[Serializable]
internal class TweaksSaveFile
{
    public Dictionary<RelativeId, TweaksSaveGirl> Girls;

    public TweaksSaveGirl GetGirl(RelativeId id) => Girls.GetOrNew(id);

    public void Clean()
    {
        Girls ??= new Dictionary<RelativeId, TweaksSaveGirl>();
    }
}