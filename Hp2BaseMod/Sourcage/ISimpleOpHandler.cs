namespace Sourceage.Element.Interface
{
    /// <summary>
    /// Impliments unary, and comparable operatins for the type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISimpleOpHandler<T>
    {
        /// <summary>
        /// Returns the default value.
        /// </summary>
        /// <returns>The default value.</returns>
        T Default();

        /// <summary>
        /// Increaces the value by 1 unit.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>A new instance of the value 1 unit larger than the given value.</returns>
        T Increment(T value);

        /// <summary>
        /// Decreases the value by 1 unit.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>A new instance of the value 1 unit smaller than the given value.</returns>
        T Decrement(T value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Returns true if the given values are equal. False otherwise.</returns>
        bool AreEqual(T a, T b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Returns true if the given values are not equal. False otherwise.</returns>
        bool NotEqual(T a, T b);

        /// <summary>
        /// If a is greater than b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        bool IsGreaterThan(T a, T b);

        /// <summary>
        /// If a is less than b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        bool IsLessThen(T a, T b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>If the given value cannot be Incremented.</returns>
        bool IsMax(T value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>If the given value cannot be Decremented.</returns>
        bool IsMin(T value);
    }
}
