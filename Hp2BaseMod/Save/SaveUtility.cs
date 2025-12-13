using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Hp2BaseMod;

public static class SaveUtility
{
    /// <summary>
    /// Creates or removes from target to match the length of source
    /// </summary>
    public static void MatchListLength<TTarget>(IList source, List<TTarget> target)
        where TTarget : new()
        => MatchListLength(source.Count, target);

    /// <summary>
    /// Adds or removes from target to match len
    /// </summary>
    public static void MatchListLength<TTarget>(int len, List<TTarget> target)
        where TTarget : new()
    {
        var delta = len - target.Count;
        if (delta > 0)
        {
            for (var i = 0; i < delta; i++)
            {
                target.Add(new());
            }
        }
        else if (delta < 0)
        {
            target.RemoveRange(len - 1, -delta);
        }
    }

    public static void HandleModSaves<TData, TModSave>(GameDataType gameDataType,
        Dictionary<RelativeId, TModSave> mods,
        List<TData> dataList,
        IEnumerable<int> dataRuntimeIds)
    where TModSave : IModSave<TData>
    {
        var modByRuntime = new Dictionary<int, TModSave>();
        foreach (var entry in mods)
        {
            if (ModInterface.Data.TryGetRuntimeDataId(gameDataType, entry.Key, out var runtime))
            {
                modByRuntime[runtime] = entry.Value;
            }
        }

        foreach (var (data, runtime) in dataList.Zip(dataRuntimeIds, (data, runtime) => (data, runtime)))
        {
            if (modByRuntime.TryGetValue(runtime, out var mod))
            {
                mod.SetData(data);
                modByRuntime.Remove(runtime);
            }
        }

        //these saves don't go by index, so we can just add the extra ones
        dataList.AddRange(modByRuntime.Select(x => x.Value.Convert(x.Key)));
    }
}