// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace System.Collections.Generic
{
    /// <summary>
    /// Helper associate to <see cref="IEnumerable{T}"/>
    /// </summary>
    public static class EnumerableHelper<T>
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="EnumerableHelper{T}"/> class.
        /// </summary>
        static EnumerableHelper()
        {
            ReadOnlyArray = new T[0];
            ReadOnly = ReadOnlyArray;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the read only.
        /// </summary>
        public static IReadOnlyCollection<T> ReadOnly { get; }

        /// <summary>
        /// Gets the read only.
        /// </summary>
        public static IEnumerable<T> Enumerable
        {
            get { return ReadOnlyArray; }
        }

        /// <summary>
        /// Gets the read only array.
        /// </summary>
        public static T[] ReadOnlyArray { get; }

        #endregion
    }
}
