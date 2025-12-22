using System.Collections.Generic;

namespace Sourceage.Element.Interface
{
    /// <summary>
    /// Implements operations on discrete sets
    /// </summary>
    /// <typeparam name="TValue">The type of values in the set.</typeparam>
    public interface IDiscreteValueOperator<TValue> : IComparer<TValue>
    {
        /// <returns>The default value.</returns>
        TValue Default();

        /// <returns>A new instance of the value 1 unit larger than the given value.</returns>
        TValue Increment(TValue value);

        /// <returns>A new instance of the value 1 unit smaller than the given value.</returns>
        TValue Decrement(TValue value);

        // This is technically a discrete set property, but TNumeric is the problem, 
        // ICollection only returns int precision in its count property which is where
        // this would be useful.
        /// <returns>The number of increments needed to reach max from min.</returns>
        //TNumeric OrdinalDistance(TValue min, TValue max);

        /// <returns>Returns true if the given values are equal. False otherwise.</returns>
        bool AreEqual(TValue a, TValue b);

        /// <returns>Returns true if the given values are not equal. False otherwise.</returns>
        bool NotEqual(TValue a, TValue b);

        /// <returns>If a is greater than b</returns>
        bool IsGreaterThan(TValue a, TValue b);

        /// <returns>If a is less than b</returns>
        bool IsLessThan(TValue a, TValue b);

        /// <returns>If the given value cannot be Incremented.</returns>
        bool IsMax(TValue value);

        /// <returns>If the given value cannot be Decremented.</returns>
        bool IsMin(TValue value);

        /// <returns>An ordered enumeration min to max inclusive.</returns>
        IEnumerable<TValue> Range(TValue min, TValue max);
    }
}
