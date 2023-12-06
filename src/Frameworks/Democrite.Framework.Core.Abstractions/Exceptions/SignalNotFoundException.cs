// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when signal name provide is missing from the <see cref="ISignalDefinitionProvider"/>
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class SignalNotFoundException : DemocriteBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalNotFoundException"/> class.
        /// </summary>
        public SignalNotFoundException(string signalName, Exception? innerException = null)
            : base(DemocriteExceptionSR.SignalNotFounded.WithArguments(signalName),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Signal, DemocriteErrorCodes.PartType.Definition, DemocriteErrorCodes.ErrorType.NotFounded),
                   innerException)
        {

        }
    }
}
