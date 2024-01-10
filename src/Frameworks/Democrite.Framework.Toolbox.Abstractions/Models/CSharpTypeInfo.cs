// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// All information about csharp type
    /// </summary>
    public static class CSharpTypeInfo
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="CSharpTypeInfo"/> class.
        /// </summary>
        static CSharpTypeInfo()
        {
            // TODO : Replace by frozen collection when .net 8.0+
            ScalarTypes = new HashSet<Type>(new []
            {
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(string),
                typeof(char),
                typeof(byte),
                typeof(ushort),
                typeof(uint),
                typeof(ulong),
            });
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the scalar types.
        /// </summary>
        public static IReadOnlySet<Type> ScalarTypes { get; }

        #endregion
    }
}
