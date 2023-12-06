// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Helpers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Helper used to resude allocation on default values
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public static class DictionaryHelper<TKey, TValue>
        where TKey : notnull
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DictionaryHelper{TKey, TValue}"/> class.
        /// </summary>
        static DictionaryHelper()
        {
            var dic = new Dictionary<TKey, TValue>();
            ReadOnly = new ReadOnlyDictionary<TKey, TValue>(dic);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the read only.
        /// </summary>
        public static IReadOnlyDictionary<TKey, TValue> ReadOnly { get; }

        #endregion
    }
}
