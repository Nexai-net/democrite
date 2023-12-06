// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    /// <summary>
    /// Define exception raise by the democrite system
    /// </summary>
    public interface IDemocriteException
    {

        /// <summary>
        /// Gets a code used to qualify error.
        /// </summary>
        /// <remarks>
        ///     Look for <see cref="DemocriteErrorCodes"/> to decrypt the code
        /// </remarks>
        ulong ErrorCode { get; }
    }
}
