using System;
using System.Collections;
using System.Collections.Generic;
using Sourceage.Element.Interface;

namespace Sourceage.Element
{
    /// <summary>
    /// A set of unique values
    /// </summary>
    public class SetManager<T> : IEnumerable<T>
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
        }

        private ISimpleOpHandler<T> _opHandler;

        private List<InclusiveRange> _ranges = new List<InclusiveRange>();

        public SetManager(ISimpleOpHandler<T> opHandler, IEnumerable<T> values = null)
        {
            _opHandler = opHandler ?? throw new ArgumentNullException(nameof(opHandler));

            if (values != null)
            {
                foreach (var value in values)
                {
                    AddItem(value);
                }
            }
        }

        /// <summary>
        /// Checks if the set contains the item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            foreach (var range in _ranges)
            {
                if (_opHandler.IsGreaterThan(range.Min, item))
                {
                    return false;
                }
                else if (!_opHandler.IsLessThen(range.Max, item))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds the given item to the set.
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(T item)
        {
            switch (_ranges.Count)
            {
                case 0:
                    _ranges.Add(new InclusiveRange(item, item));
                    break;
                case 1:
                    var range = _ranges[0];

                    // before the range
                    if (_opHandler.IsLessThen(item, range.Min))
                    {
                        // 1 before the range
                        if (_opHandler.AreEqual(item, _opHandler.Decrement(range.Min)))
                        {
                            range.Min = item;
                        }
                        else
                        {
                            _ranges.Add(new InclusiveRange(item, item));
                        }
                    }
                    // after the range
                    else if (_opHandler.IsGreaterThan(item, range.Max))
                    {
                        // 1 after the range
                        if (_opHandler.AreEqual(item, _opHandler.Increment(range.Max)))
                        {
                            range.Max = item;
                        }
                        else
                        {
                            _ranges.Add(new InclusiveRange(item, item));
                        }
                    }
                    // already in the range
                    break;
                default:
                    var it = _ranges.GetEnumerator();
                    it.MoveNext();

                    // item is before all existing
                    if (_opHandler.IsLessThen(item, it.Current.Min))
                    {
                        // If it's just 1 less, expand the range to include it
                        if (_opHandler.AreEqual(item, _opHandler.Decrement(it.Current.Min)))
                        {
                            it.Current.Min = item;
                        }
                        // Otherwise add a new range
                        else
                        {
                            _ranges.Add(new InclusiveRange(item, item));
                        }

                        return;
                    }

                    // item is in the first range
                    if (!_opHandler.IsLessThen(it.Current.Max, item))
                    {
                        return;
                    }

                    // item is in the middle of ranges, possibly in a gap

                    var previous = it.Current;

                    // there are more than 1 entries, so this will run at lease once
                    while (it.MoveNext())
                    {
                        // one away from previous
                        if (_opHandler.AreEqual(item, _opHandler.Increment(previous.Max)))
                        {
                            //also one away from next so combine them
                            if (_opHandler.AreEqual(item, _opHandler.Decrement(it.Current.Min)))
                            {
                                previous.Max = it.Current.Max;
                                _ranges.Remove(it.Current);
                            }
                            //otherwise just expand previous to include it
                            else
                            {
                                previous.Max = item;
                            }

                            return;
                        }

                        // behind current
                        if (_opHandler.IsLessThen(item, it.Current.Min))
                        {
                            //one less
                            if (_opHandler.AreEqual(item, _opHandler.Decrement(it.Current.Min)))
                            {
                                it.Current.Min = item;
                            }
                            //otherwise add a new range
                            else
                            {
                                _ranges.Add(new InclusiveRange(item, item));
                            }

                            return;
                        }

                        // inside current
                        if (!_opHandler.IsLessThen(it.Current.Max, item))
                        {
                            return;
                        }

                        //iterate
                        previous = it.Current;
                    }

                    // item is one after largest value
                    if (_opHandler.AreEqual(item, _opHandler.Increment(previous.Max)))
                    {
                        previous.Max = item;
                    }
                    // otherwise it's off on its own
                    else
                    {
                        _ranges.Add(new InclusiveRange(item, item));
                    }
                    return;
            }
        }

        public void AddItems(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                AddItem(item);
            }
        }

        /// <summary>
        /// Removes the given item from the set.
        /// </summary>
        /// <param name="item"></param>
        public void RemoveItem(T item)
        {
            int index = 0;
            var it = _ranges.GetEnumerator();
            while (it.MoveNext())
            {
                if (_opHandler.IsLessThen(item, it.Current.Min))
                {
                    // If it's less than a ranges min, it's not in it or any of the following
                    return;
                }

                if (!_opHandler.IsGreaterThan(item, it.Current.Max))
                {
                    // It's inside the range, we gotta take it out bud
                    if (_opHandler.AreEqual(item, it.Current.Min))
                    {
                        it.Current.Min = _opHandler.Increment(it.Current.Min);
                    }
                    else if (_opHandler.AreEqual(item, it.Current.Max))
                    {
                        it.Current.Max = _opHandler.Decrement(it.Current.Max);
                    }
                    else
                    {
                        // split the range
                        _ranges.Insert(index, new InclusiveRange(it.Current.Min, _opHandler.Decrement(item)));
                        it.Current.Min = _opHandler.Increment(item);
                    }

                    return;
                }

                index++;
            }
        }

        /// <summary>
        /// Adds the next unused item to the set and returns it.
        /// </summary>
        /// <returns>The next unused item.</returns>
        public T AddUnusedItem()
        {
            switch (_ranges.Count)
            {
                case 0:
                    var d = _opHandler.Default();
                    _ranges.Add(new InclusiveRange(d, d));
                    return d;
                case 1:
                    var first = _ranges[0];
                    if (_opHandler.IsMax(first.Max))
                    {
                        if (_opHandler.IsMin(first.Min))
                        {
                            throw new Exception("Set contains all possible unique values and is unable to add another.");
                        }

                        first.Min = _opHandler.Decrement(first.Min);
                        return first.Min;
                    }

                    first.Max = _opHandler.Increment(first.Max);
                    return first.Max;
                default:
                    // We're fine with adding any unused value, so we can just use the first available ranges. 
                    var a = _ranges[0];
                    var b = _ranges[1];

                    // if they are 1 apart, combine them and return the middle
                    if (_opHandler.AreEqual(a.Max, _opHandler.Decrement(_opHandler.Decrement(b.Min))))
                    {
                        _ranges.RemoveAt(1);
                        a.Max = b.Max;
                        return _opHandler.Decrement(b.Min);
                    }

                    // otherwise just increase a.Max by 1 and return that.
                    a.Max = _opHandler.Increment(a.Max);
                    return a.Max;
            }
        }

        /// <summary>
        /// Returns an enumerator that enumerates the collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var range in _ranges)
            {
                if (_opHandler.AreEqual(range.Min, range.Max))
                {
                    yield return range.Min;
                }
                else
                {
                    var current = range.Min;

                    do
                    {
                        yield return current;
                        current = _opHandler.Increment(current);
                    } while (_opHandler.NotEqual(current, range.Max));

                    yield return current;
                }
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
