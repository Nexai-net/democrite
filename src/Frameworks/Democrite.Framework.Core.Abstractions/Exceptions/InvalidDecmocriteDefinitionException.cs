// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Resources;
    using Elvex.Toolbox.Abstractions.Loggers;
    using Elvex.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Exception raised when a definition provide invalid validation
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class InvalidDecmocriteDefinitionException : DemocriteBaseException<InvalidDecmocriteDefinitionException>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDecmocriteDefinitionException"/> class.
        /// </summary>
        public InvalidDecmocriteDefinitionException(IDefinition definition, IEnumerable<SimpleLog> errors, Exception? innerException = null)
            : this(DemocriteExceptionSR.InvalidDefinitionErrorMessages.WithArguments(definition?.Uid, definition?.ToDebugDisplayName(), string.Join(Environment.NewLine, errors.Select(e => e.Message))),
                   definition?.Uid ?? Guid.Empty,
                   definition?.ToDebugDisplayName() ?? string.Empty,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Build, DemocriteErrorCodes.PartType.Definition, DemocriteErrorCodes.ErrorType.Invalid),
                   errors,
                   innerException)
        {
            ArgumentNullException.ThrowIfNull(definition);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDecmocriteDefinitionException"/> class.
        /// </summary>
        internal InvalidDecmocriteDefinitionException(string message,
                                                      Guid definitionUid,
                                                      string definitionDisplayName,
                                                      ulong errorCode,
                                                      IEnumerable<SimpleLog> errors,
                                                      Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Errors = errors?.ToArray() ?? EnumerableHelper<SimpleLog>.ReadOnlyArray;

            //this.Data.Add(nameof(InvalidDecmocriteDefinitionExceptionSurrogate.Errors), this.Errors);
            this.Data.Add(nameof(InvalidDecmocriteDefinitionExceptionSurrogate.DefinitionUid), definitionUid);
            this.Data.Add(nameof(InvalidDecmocriteDefinitionExceptionSurrogate.DefinitionDisplayName), definitionDisplayName);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the errors.
        /// </summary>
        public IReadOnlyCollection<SimpleLog> Errors { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(DemocriteBaseException other)
        {
            return other is InvalidDecmocriteDefinitionException inv &&
                   this.Errors.OrderBy(e => e.Message).SequenceEqual(inv.Errors.OrderBy(e => e.Message));
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.Errors.OrderBy(e => e.Message).Aggregate(0, (acc, e) => acc ^ e.GetHashCode());
        }

        #endregion
    }

    [GenerateSerializer]
    public struct SimpleLogSurrogate
    {
        /// <summary>
        /// Gets the log level.
        /// </summary>
        [Id(0)]
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        [Id(1)]
        public string Message { get; set; }
    }

    [GenerateSerializer]
    public struct InvalidDecmocriteDefinitionExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public Guid DefinitionUid { get; set; }

        [Id(3)]
        public string DefinitionDisplayName { get; set; }

        [Id(4)]
        public SimpleLogSurrogate[] Errors { get; set; }

        [Id(5)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class InvalidDecmocriteDefinitionExceptionConverter : IConverter<InvalidDecmocriteDefinitionException, InvalidDecmocriteDefinitionExceptionSurrogate>
    {
        public InvalidDecmocriteDefinitionException ConvertFromSurrogate(in InvalidDecmocriteDefinitionExceptionSurrogate surrogate)
        {
            return new InvalidDecmocriteDefinitionException(surrogate.Message,
                                                            surrogate.DefinitionUid,
                                                            surrogate.DefinitionDisplayName,
                                                            surrogate.ErrorCode,
                                                            surrogate.Errors?.Select(e => new SimpleLog(e.LogLevel, e.Message)).ToArray() ?? EnumerableHelper<SimpleLog>.ReadOnlyArray,
                                                            surrogate.InnerException);
        }

        public InvalidDecmocriteDefinitionExceptionSurrogate ConvertToSurrogate(in InvalidDecmocriteDefinitionException value)
        {
            return new InvalidDecmocriteDefinitionExceptionSurrogate()
            {
                Message = value.Message,
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode,
                DefinitionUid = (Guid)value.Data[nameof(InvalidDecmocriteDefinitionExceptionSurrogate.DefinitionUid)]!,
                DefinitionDisplayName = (string)value.Data[nameof(InvalidDecmocriteDefinitionExceptionSurrogate.DefinitionDisplayName)]!,
                Errors = value.Errors?.Select(e => new SimpleLogSurrogate() { LogLevel = e.LogLevel, Message = e.Message }).ToArray() ?? EnumerableHelper<SimpleLogSurrogate>.ReadOnlyArray
            };
        }
    }
}
