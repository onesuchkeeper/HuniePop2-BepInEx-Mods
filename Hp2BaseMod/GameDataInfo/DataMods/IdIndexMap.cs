using System;
using System.Collections.Generic;

namespace Hp2BaseMod.GameDataInfo;

/// <summary>
/// Manages assigning ids to collection indexes
/// </summary>
public class IdIndexMap
{
    public IEnumerable<RelativeId> Ids => _idToIndex.Keys;
    public IEnumerable<int> Indexes => _indexToId.Keys;

    private int _offset;
    private Dictionary<RelativeId, int> _idToIndex = new();
    private Dictionary<int, RelativeId> _indexToId = new();

    public IdIndexMap()
    {
        _offset = 0;
    }

    /// <param name="offset">Starting index for new ids.</param>
    /// <exception cref="ArgumentException">If offset is less than zero.</exception>
    public IdIndexMap(int offset)
    {
        if (offset < 0) throw new ArgumentException(nameof(offset));
        _offset = offset;
    }

    public int this[RelativeId id] => GetIndex(id);
    public int GetIndex(RelativeId id)
    {
        if (id == RelativeId.Default) return -1;

        if (!_idToIndex.TryGetValue(id, out var index))
        {
            index = _idToIndex.Count + _offset;
            _idToIndex[id] = index;
            _indexToId[index] = id;
        }

        return index;
    }

    public bool TryGetIndex(RelativeId id, out int index) => _idToIndex.TryGetValue(id, out index);

    public RelativeId this[int index] => GetId(index);
    public RelativeId GetId(int index) => index < _offset
        ? RelativeId.Default
        : _indexToId[index];

    public bool TryGetId(int index, out RelativeId id) => _indexToId.TryGetValue(index, out id);

    /// <summary>
    /// Initializes the map with a range of id-index pairs.
    /// </summary>
    /// <param name="count">The number of entries</param>
    /// <param name="startingIndex">First index</param>
    /// <param name="idOffset">Offset from the index used for the local id</param>
    internal void MapRelativeIdRange(int count, int startingIndex = 0, int idOffset = 0)
    {
        for (int i = startingIndex; i < startingIndex + count; i++)
        {
            var id = new RelativeId(-1, i + idOffset);
            _idToIndex[id] = i;
            _indexToId[i] = id;
        }
    }

    /// <summary>
    /// Force-adds a specific id index mapping
    /// </summary>
    internal void ForceMap(RelativeId id, int index)
    {
        _idToIndex[id] = index;
        _indexToId[index] = id;
    }
}
