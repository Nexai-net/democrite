// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when door information provide is missing from the <see cref="IDoorDefinitionProvider"/>
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class DoorNotFoundException : DemocriteBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoorNotFoundException"/> class.
        /// </summary>
        public DoorNotFoundException(string doorIdentifier, Exception? innerException = null)
            : base(DemocriteExceptionSR.DoorNotFounded.WithArguments(doorIdentifier),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Door, DemocriteErrorCodes.PartType.Definition, DemocriteErrorCodes.ErrorType.NotFounded),
                   innerException)
        {

        }
    }
}
