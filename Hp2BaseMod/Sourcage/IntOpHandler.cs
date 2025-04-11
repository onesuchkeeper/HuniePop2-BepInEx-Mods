using Sourceage.Element.Interface;

namespace Sourceage.Element
{
    public class IntOpHandler : ISimpleOpHandler<int>
    {
        public int Default() => 0;
        public int Increment(int value) => value + 1;
        public int Decrement(int value) => value - 1;
        public bool AreEqual(int a, int b) => a == b;
        public bool NotEqual(int a, int b) => a != b;
        public bool IsMax(int value) => value == int.MaxValue;
        public bool IsMin(int value) => value == int.MinValue;
        public bool IsGreaterThan(int a, int b) => a > b;
        public bool IsLessThen(int a, int b) => a < b;
    }
}
