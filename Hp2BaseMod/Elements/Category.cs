using System;
using System.Collections.Generic;
using System.Linq;

namespace Hp2BaseMod.Elements;

public class Category<T>
{
    public class Entry
    {
        /// <summary>
        /// Weight for the value to be chosen from its pool
        /// </summary>
        public int Weight;

        public T Value;

        public Entry(T def, int weight)
        {
            Value = def;
            Weight = weight;
        }
    }

    /// <summary>
    /// Items in the category
    /// </summary>
    public List<Entry> Pool;

    /// <summary>
    /// The max number of items to select from the category
    /// </summary>
    public int TargetCount = 0;

    /// <summary>
    /// Priority of which category to fulfil first
    /// </summary>
    public int Priority = 0;

    /// <summary>
    /// Special logic for when an entry is chosen. Is called after the
    /// entry is removed from the pool and before 
    /// </summary>
    public Action<Entry> OnEntryChosen;

    /// <summary>
    /// Randomly chooses an entry based on the weights of the entries.
    /// </summary>
    /// <param name="entries"></param>
    /// <param name="totalWeight"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Entry GetWeighted(List<Entry> entries, int totalWeight)
    {
        if (entries == null || entries.Count == 0)
            throw new ArgumentException("Entries list is empty");

        if (entries.Any(e => e.Weight < 0))
            throw new ArgumentException("Negative weights are not allowed");

        var selectedWeight = UnityEngine.Random.Range(0, totalWeight);
        var currentWeight = 0;

        foreach (var weightedValue in entries)
        {
            currentWeight += weightedValue.Weight;

            if (currentWeight > selectedWeight)
            {
                return weightedValue;
            }
        }

        throw new Exception("Failed to get weighted");
    }

    public static Entry GetWeighted(List<Entry> entries)
        => GetWeighted(entries, entries.Sum(x => x.Weight));

    public static Entry PopWeighted(List<Entry> entries, int totalWeight)
    {
        if (entries == null || entries.Count == 0)
            throw new ArgumentException("Entries list is empty");

        if (entries.Any(e => e.Weight < 0))
            throw new ArgumentException("Negative weights are not allowed");

        var selectedWeight = UnityEngine.Random.Range(0, totalWeight);
        var currentWeight = 0;

        foreach (var weightedValue in entries)
        {
            currentWeight += weightedValue.Weight;

            if (currentWeight > selectedWeight)
            {
                entries.Remove(weightedValue);
                return weightedValue;
            }
        }

        throw new Exception("Failed to pop weighted");
    }

    public static Entry PopWeighted(List<Entry> values)
        => PopWeighted(values, values.Sum(x => x.Weight));
}
