using System.Collections.Generic;
using Sourceage.Element.Interface;

namespace Sourceage.Element
{
    public class IntOperator : IDiscreteValueOperator<int>
    {
        public int Default() => 0;
        public int Increment(int value) => value + 1;
        public int Decrement(int value) => value - 1;
        public bool AreEqual(int a, int b) => a == b;
        public bool NotEqual(int a, int b) => a != b;
        public bool IsMax(int value) => value == int.MaxValue;
        public bool IsMin(int value) => value == int.MinValue;
        public bool IsGreaterThan(int a, int b) => a > b;
        public bool IsLessThan(int a, int b) => a < b;

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

        public int Compare(int x, int y) => x.CompareTo(y);
    }
}
