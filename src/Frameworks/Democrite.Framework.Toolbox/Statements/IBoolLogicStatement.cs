// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Statements
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public interface IBoolLogicStatement
    {
        /// <summary>
        /// Ask is statement is valid
        /// </summary>
        /// <param name="input">The inputs variables.</param>
        bool Ask(in ReadOnlySpan<bool> input);
    }
}
