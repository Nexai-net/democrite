// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Helpers
{
    /// <summary>
    /// Helper Around regex
    /// </summary>
    public static class RegexHelper
    {
        /// <summary>
        /// Define regex classic pattern
        /// </summary>
        public static class Pattern
        {
            /// <summary>
            /// The unique identifier pattern
            /// </summary>
            public const string GUID = "[a-zA-Z0-9]{8}[-]?[a-zA-Z0-9]{4}[-]?[a-zA-Z0-9]{4}[-]?[a-zA-Z0-9]{4}[-]?[a-zA-Z0-9]{12}";
        }
    }
}
