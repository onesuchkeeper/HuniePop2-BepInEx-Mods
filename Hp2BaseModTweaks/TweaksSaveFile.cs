using System;
using System.Collections.Generic;
using Hp2BaseMod;

[Serializable]
internal class TweaksSaveFile
{
    public Dictionary<RelativeId, TweaksSaveGirl> Girls;

    public TweaksSaveGirl GetGirl(RelativeId id)
    {
        if (!Girls.TryGetValue(id, out var girl))
        {
            girl = new TweaksSaveGirl();
            Girls[id] = girl;
        }

        return girl;
    }

    public void Clean()
    {
        Girls ??= new Dictionary<RelativeId, TweaksSaveGirl>();
    }
}