using System.Collections.Generic;
using Hp2BaseMod.Elements.Interface;

namespace Hp2BaseMod.Elements
{
    /// <summary>
    /// Implementation of <see cref="IDiscreteValueOperator{}"/> for integers.
    /// </summary>
    public class IntOperator : IDiscreteValueOperator<int>
    {
        ///<inheritdoc/>
        public int Default() => 0;
        
        ///<inheritdoc/>
        public int Increment(int value) => value + 1;

        ///<inheritdoc/>
        public int Decrement(int value) => value - 1;

        ///<inheritdoc/>
        public bool AreEqual(int a, int b) => a == b;

        ///<inheritdoc/>
        public bool NotEqual(int a, int b) => a != b;

        ///<inheritdoc/>
        public bool IsMax(int value) => value == int.MaxValue;

        ///<inheritdoc/>
        public bool IsMin(int value) => value == int.MinValue;

        ///<inheritdoc/>
        public bool IsGreaterThan(int a, int b) => a > b;

        ///<inheritdoc/>
        public bool IsLessThan(int a, int b) => a < b;

        ///<inheritdoc/>
        public IEnumerable<int> Range(int start, int end)
        {
            if (start == end)
            {
                yield return start;
            }
            else
            {
                for (var i = start; i < end; i++)
                {
                    yield return i;
                }

                // avoid overflow from end+1 in for loop
                yield return end;
            }
        }

        ///<inheritdoc/>
        public int Compare(int x, int y) => x.CompareTo(y);
    }
}
