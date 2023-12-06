// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace System
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// 
    /// </summary>
    public static class SpanExtensions
    {
        /// <summary>
        /// Counts number of time the value <paramref name="valueToCount"/> is in <paramref name="values"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Count<T>(in this ReadOnlySpan<T> values, T valueToCount)
        {
            return Count(values, (item) => EqualityComparer<T>.Default.Equals(item, valueToCount));
        }

        /// <summary>
        /// Counts number of time the <paramref name="valueToCountPredicate"/> is valid
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Count<T>(in this ReadOnlySpan<T> values, Func<T?, bool> valueToCountPredicate)
        {
            uint counter = 0;
            foreach (var item in values)
            {
                if (valueToCountPredicate(item))
                    counter++;
            }

            return counter;
        }
    }
}
