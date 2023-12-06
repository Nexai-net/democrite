// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when definition required is missing
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class MissingDefinitionException : DemocriteBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingDefinitionException"/> class.
        /// </summary>
        public MissingDefinitionException(Type definitionType,
                                          Guid definitionId,
                                          Exception? innerException = null)
            : base(DemocriteExceptionSR.DefinitionMissingExceptionMessage.WithArguments(definitionType, definitionId),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Definition, DemocriteErrorCodes.PartType.Identifier, DemocriteErrorCodes.ErrorType.Missing),
                   innerException)
        {
            this.Data.Add(nameof(definitionType), definitionType);
            this.Data.Add(nameof(definitionId), definitionId);
        }
    }
}
