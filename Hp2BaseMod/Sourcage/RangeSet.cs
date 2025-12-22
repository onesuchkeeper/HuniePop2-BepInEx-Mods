using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sourceage.Element.Interface;

namespace Sourceage.Element
{
    /// <summary>
    /// A set of unique values. Takes advantage of discrete set properties
    /// to efficiently handle elements. Enumerates in min to max order.
    /// </summary>
    public class RangeSet<T> : IEnumerable<T>
    {
        private class InclusiveRange
        {
            public T Min;

            public T Max;

            public InclusiveRange(T min, T max)
            {
                Min = min;
                Max = max;
            }

            public override string ToString()
            {
                return $"[{Min}, {Max}]";
            }
        }

        private IDiscreteValueOperator<T> _operator;
        private List<InclusiveRange> _ranges = new List<InclusiveRange>();

        public RangeSet(IDiscreteValueOperator<T> opHandler, IEnumerable<T> values = null)
        {
            _operator = opHandler ?? throw new ArgumentNullException(nameof(opHandler));
            Add(values);
        }

        /// <returns>If the set contains the value.</returns>
        public bool Contains(T value)
        {
            if (_ranges.Count == 0)
            {
                return false;
            }

            int left = 0;
            int right = _ranges.Count - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                var range = _ranges[mid];

                // value is less than the range's minimum
                if (_operator.IsLessThan(value, range.Min))
                {
                    right = mid - 1;
                }
                // value is greater than the range's maximum
                else if (_operator.IsGreaterThan(value, range.Max))
                {
                    left = mid + 1;
                }
                // value is within the range (Min <= value <= Max)
                else
                {
                    return true;
                }
            }

            return false;
        }

        /// <returns>If the set contains all possible values.</returns>
        public bool ContainsAllValues()
        {
            if (_ranges.Count == 1)
            {
                var range = _ranges[0];
                return _operator.IsMin(range.Min) && _operator.IsMax(range.Max);
            }

            return false;
        }

        /// <summary>
        /// Adds the given item to the set.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            switch (_ranges.Count)
            {
                case 0:
                    _ranges.Add(new InclusiveRange(item, item));
                    break;
                case 1:
                    var range = _ranges[0];

                    // before the range
                    if (_operator.IsLessThan(item, range.Min))
                    {
                        // 1 before the range
                        if (_operator.AreEqual(item, _operator.Decrement(range.Min)))
                        {
                            range.Min = item;
                        }
                        else
                        {
                            _ranges.Insert(0, new InclusiveRange(item, item));
                        }
                    }
                    // after the range
                    else if (_operator.IsGreaterThan(item, range.Max))
                    {
                        // 1 after the range
                        if (_operator.AreEqual(item, _operator.Increment(range.Max)))
                        {
                            range.Max = item;
                        }
                        else
                        {
                            _ranges.Insert(1, new InclusiveRange(item, item));
                        }
                    }
                    // already in the range
                    break;
                default:
                    using (var it = _ranges.GetEnumerator())
                    {
                        it.MoveNext();

                        // item is before all existing
                        if (_operator.IsLessThan(item, it.Current.Min))
                        {
                            // If it's just 1 less, expand the range to include it
                            if (_operator.AreEqual(item, _operator.Decrement(it.Current.Min)))
                            {
                                it.Current.Min = item;
                            }
                            // Otherwise add a new range
                            else
                            {
                                _ranges.Insert(0, new InclusiveRange(item, item));
                            }

                            return;
                        }

                        // item is in the first range
                        if (!_operator.IsLessThan(it.Current.Max, item))
                        {
                            return;
                        }

                        // item is in the middle of ranges, possibly in a gap
                        var previous = it.Current;
                        var index = 0;

                        // there are more than 1 entries, so this will run at lease once
                        while (it.MoveNext())
                        {
                            index++;

                            // one away from previous
                            if (_operator.AreEqual(item, _operator.Increment(previous.Max)))
                            {
                                // also one away from next so combine them
                                if (_operator.AreEqual(item, _operator.Decrement(it.Current.Min)))
                                {
                                    previous.Max = it.Current.Max;
                                    _ranges.Remove(it.Current);
                                }
                                // otherwise just expand previous to include it
                                else
                                {
                                    previous.Max = item;
                                }

                                return;
                            }

                            // behind current
                            if (_operator.IsLessThan(item, it.Current.Min))
                            {
                                // one less
                                if (_operator.AreEqual(item, _operator.Decrement(it.Current.Min)))
                                {
                                    it.Current.Min = item;
                                }
                                // otherwise add a new range
                                else
                                {
                                    _ranges.Insert(index, new InclusiveRange(item, item));
                                }

                                return;
                            }

                            // inside current
                            if (!_operator.IsLessThan(it.Current.Max, item))
                            {
                                return;
                            }

                            // iterate
                            previous = it.Current;
                        }

                        // item is one after largest value
                        if (_operator.AreEqual(item, _operator.Increment(previous.Max)))
                        {
                            previous.Max = item;
                        }
                        // otherwise it's off on its own
                        else
                        {
                            _ranges.Add(new InclusiveRange(item, item));
                        }
                    }

                    return;
            }
        }

        /// <summary>
        /// Adds multiple values to the set
        /// </summary>
        public void Add(IEnumerable<T> items)
        {
            if (items == null || !items.Any())
            {
                return;
            }

            // Sort and remove duplicates to identify sequential runs
            var sortedItems = items.Distinct().OrderBy(x => x, _operator).ToList();

            // Process items, detecting sequential runs
            var rangeStart = sortedItems[0];
            var rangeEnd = sortedItems[0];

            for (int i = 1; i < sortedItems.Count; i++)
            {
                var current = sortedItems[i];

                // Check if current is sequential (next value after rangeEnd)
                if (_operator.AreEqual(current, _operator.Increment(rangeEnd)))
                {
                    // Extend the current range
                    rangeEnd = current;
                }
                else
                {
                    // Non-sequential, so add the current range and start a new one
                    if (_operator.AreEqual(rangeStart, rangeEnd))
                    {
                        Add(rangeStart);
                    }
                    else
                    {
                        AddRange(rangeStart, rangeEnd);
                    }

                    rangeStart = current;
                    rangeEnd = current;
                }
            }

            // Add the final range
            if (_operator.AreEqual(rangeStart, rangeEnd))
            {
                Add(rangeStart);
            }
            else
            {
                AddRange(rangeStart, rangeEnd);
            }
        }

        /// <summary>
        /// Adds all values between and including max and min to the set
        /// </summary>
        public void AddRange(T min, T max)
        {
            if (_operator.IsGreaterThan(min, max))
            {
                throw new ArgumentException("min was greater than max");
            }

            if (_operator.AreEqual(min, max))
            {
                Add(min);
                return;
            }

            if (_ranges.Count == 0)
            {
                _ranges.Add(new InclusiveRange(min, max));
                return;
            }

            int index = 0;
            using (var it = _ranges.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var current = it.Current;

                    // new range ends before current starts
                    if (_operator.IsLessThan(max, MinOrDecrement(current.Min)))
                    {
                        _ranges.Insert(index, new InclusiveRange(min, max));
                        return;
                    }

                    // new range starts after current ends
                    if (_operator.IsGreaterThan(min, MaxOrIncrement(current.Max)))
                    {
                        index++;
                        continue;
                    }

                    // overlap or adjacency found
                    MergeOverlappingRanges(index, min, max);
                    return;
                }
            }

            // new range comes after all existing
            _ranges.Add(new InclusiveRange(min, max));
        }

        private void MergeOverlappingRanges(int startIndex, T min, T max)
        {
            var index = startIndex;

            using (var it = _ranges.Skip(index).GetEnumerator())
            {
                it.MoveNext();
                var workingRange = it.Current;

                if (_operator.IsLessThan(min, workingRange.Min))
                {
                    workingRange.Min = min;
                }

                // expand the working range to encompass the max
                var maxAndMergeLimit = MaxOrIncrement(max);
                while (it.MoveNext())
                {
                    // no longer touching or overlapping
                    if (_operator.IsGreaterThan(it.Current.Min, maxAndMergeLimit))
                    {
                        break;
                    }

                    workingRange.Max = it.Current.Max;
                    index++;
                }

                if (_operator.IsGreaterThan(max, workingRange.Max))
                {
                    workingRange.Max = max;
                }
            }

            // Remove subsequent overlapping ranges (if any)
            var removeCount = index - startIndex;
            if (removeCount > 0)
            {
                _ranges.RemoveRange(startIndex + 1, removeCount);
            }
        }

        private T MinOrDecrement(T value)
        {
            var foo = _operator.IsMin(value)
            ? value
            : _operator.Decrement(value);
            return foo;
        }

        private T MaxOrIncrement(T value) => _operator.IsMax(value)
            ? value
            : _operator.Increment(value);

        /// <summary>
        /// Removes the given item from the set.
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            using (var it = _ranges.GetEnumerator())
            {
                int index = 0;
                while (it.MoveNext())
                {
                    if (_operator.IsLessThan(item, it.Current.Min))
                    {
                        // If it's less than a ranges min, it's not in it or any of the following
                        return;
                    }

                    if (!_operator.IsGreaterThan(item, it.Current.Max))
                    {
                        // It's inside the range, we gotta take it out bud
                        if (_operator.AreEqual(it.Current.Min, it.Current.Max))
                        {
                            _ranges.RemoveAt(index);
                        }
                        else if (_operator.AreEqual(item, it.Current.Min))
                        {
                            it.Current.Min = _operator.Increment(it.Current.Min);
                        }
                        else if (_operator.AreEqual(item, it.Current.Max))
                        {
                            it.Current.Max = _operator.Decrement(it.Current.Max);
                        }
                        else
                        {
                            // split the range
                            _ranges.Insert(index, new InclusiveRange(it.Current.Min, _operator.Decrement(item)));
                            it.Current.Min = _operator.Increment(item);
                        }

                        return;
                    }

                    index++;
                }
            }
        }

        /// <summary>
        /// Adds the next unused value to the set and returns it.
        /// </summary>
        /// <returns>The next unused item.</returns>
        public T AddUnused()
        {
            switch (_ranges.Count)
            {
                case 0:
                    var d = _operator.Default();
                    _ranges.Add(new InclusiveRange(d, d));
                    return d;
                case 1:
                    var first = _ranges[0];
                    if (_operator.IsMax(first.Max))
                    {
                        if (_operator.IsMin(first.Min))
                        {
                            throw new Exception("Set contains all possible unique values and is unable to add another.");
                        }

                        first.Min = _operator.Decrement(first.Min);
                        return first.Min;
                    }

                    first.Max = _operator.Increment(first.Max);
                    return first.Max;
                default:
                    // We're fine with adding any unused value, so we can just use the first available ranges. 
                    var a = _ranges[0];
                    var b = _ranges[1];

                    // if they are 1 apart, combine them and return the middle
                    if (_operator.AreEqual(a.Max, _operator.Decrement(_operator.Decrement(b.Min))))
                    {
                        _ranges.RemoveAt(1);
                        a.Max = b.Max;
                        return _operator.Decrement(b.Min);
                    }

                    // otherwise just increase a.Max by 1 and return that.
                    a.Max = _operator.Increment(a.Max);
                    return a.Max;
            }
        }

        /// <returns>An enumerator that enumerates the set.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var range in _ranges)
            {
                if (_operator.AreEqual(range.Min, range.Max))
                {
                    yield return range.Min;
                }
                else
                {
                    foreach (var value in _operator.Range(range.Min, range.Max))
                    {
                        yield return value;
                    }
                }
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Join(", ", _ranges);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _ranges.Clear();
        }
    }
}
