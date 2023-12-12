// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Resources;
    using Democrite.Framework.Toolbox.Abstractions.Loggers;
    using Democrite.Framework.Toolbox.Helpers;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Exception raised when a definition provide invalid validation
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class InvalidDecmocriteDefinitionException : DemocriteBaseException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDecmocriteDefinitionException"/> class.
        /// </summary>
        public InvalidDecmocriteDefinitionException(IDefinition definition, IEnumerable<SimpleLog> errors, Exception? innerException = null)
            : base(DemocriteExceptionSR.InvalidDefinitionErrorMessages.WithArguments(definition?.Uid, definition?.ToDebugDisplayName(), string.Join(Environment.NewLine, errors)),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Build, DemocriteErrorCodes.PartType.Definition, DemocriteErrorCodes.ErrorType.Invalid),
                   innerException)
        {
            ArgumentNullException.ThrowIfNull(definition);

            this.Errors = errors?.ToArray() ?? EnumerableHelper<SimpleLog>.ReadOnlyArray;

            this.Data.Add(nameof(this.Errors), this.Errors);
            this.Data.Add("DefinitionId", definition.Uid);
            this.Data.Add("DefinitionName", definition.ToDebugDisplayName());
        }

        #endregion

        #region

        /// <summary>
        /// Gets the errors.
        /// </summary>
        public IReadOnlyCollection<SimpleLog> Errors { get; }

        #endregion
    }
}
