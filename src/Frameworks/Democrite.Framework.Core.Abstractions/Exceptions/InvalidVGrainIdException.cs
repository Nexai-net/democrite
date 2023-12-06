// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Raised vgrain id is not the one expected
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class InvalidVGrainIdException : DemocriteBaseException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidVGrainIdException"/> class.
        /// </summary>
        public InvalidVGrainIdException(GrainId receivedId, string expectDetails, Exception? innerException = null)
            : base(DemocriteExceptionSR.InvalidVGrainIdExceptionMessage.WithArguments(receivedId, expectDetails),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.VGrain, DemocriteErrorCodes.PartType.Identifier, DemocriteErrorCodes.ErrorType.Invalid),
                   innerException)
        {
            this.Data.Add(nameof(receivedId), receivedId);
            this.Data.Add(nameof(expectDetails), expectDetails);
        }

        #endregion
    }
}
