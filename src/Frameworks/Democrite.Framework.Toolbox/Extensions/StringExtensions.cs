// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace System
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Extensions methods about <see cref="string"/>
    /// </summary>
    public static class StringExtensions
    {
        #region Methods

        /// <summary>
        /// Extensions to simplify the call of <see cref="string.Format"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string WithArguments(this string str, params object?[] arguments)
        {
            return string.Format(str, arguments);
        }

        #endregion
    }
}
