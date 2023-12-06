// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Exceptions;

    using System;

    /// <summary>
    /// Raised when a security feature have been broken
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class VGrainSecurityDemocriteException : DemocriteBaseException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainSecurityDemocriteException"/> class.
        /// </summary>
        public VGrainSecurityDemocriteException(string message, Exception? innerException = null)
            : base(message,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Security),
                   innerException)
        {
        }

        #endregion
    }
}
