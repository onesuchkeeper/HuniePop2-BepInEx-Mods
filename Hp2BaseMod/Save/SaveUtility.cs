using System.Collections.Generic;
using System.Linq;
namespace Hp2BaseMod;

public static class SaveUtility
{
    public static void MatchListLength<TSource, TTarget>(List<TSource> source, List<TTarget> target)
        where TTarget : new()
    {
        var delta = source.Count - target.Count;
        if (delta > 0)
        {
            for (var i = 0; i < delta; i++)
            {
                target.Add(new());
            }
        }
        else if (delta < 0)
        {
            target.RemoveRange(source.Count - 1, -delta);
        }
    }

    public static void HandleModSaves<TData, TModSave>(GameDataType gameDataType,
        Dictionary<RelativeId, TModSave> mods,
        List<TData> dataList,
        IEnumerable<int> dataRuntimeIds,
        string failNoun)
    where TModSave : IModSave<TData>
    {
        var modByRuntime = new Dictionary<int, TModSave>();
        foreach (var entry in mods)
        {
            if (ModInterface.Data.TryGetRuntimeDataId(gameDataType, entry.Key, out var runtime))
            {
                modByRuntime[runtime] = entry.Value;
            }
            else
            {
                ModInterface.Log.LogWarning($"Discarding {failNoun} with unregistered id {entry.Key} from save");
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
        dataList.AddRange(modByRuntime.Select(x => x.Value.Convert(x.Key)));
    }
}