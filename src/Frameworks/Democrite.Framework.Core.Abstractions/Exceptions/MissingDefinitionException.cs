// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;
    using Elvex.Toolbox.Models;

    using System;

    /// <summary>
    /// Raised when definition required is missing
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class MissingDefinitionException : DemocriteBaseException<MissingDefinitionException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingDefinitionException"/> class.
        /// </summary>
        public MissingDefinitionException(Type definitionType,
                                          string definitionId,
                                          string? extraInfo = null,
                                          Exception? innerException = null)
            : this(DemocriteExceptionSR.DefinitionMissingExceptionMessage.WithArguments(definitionType, definitionId) + (string.IsNullOrEmpty(extraInfo) ? "" : " : " + extraInfo),
                   definitionType.GetAbstractType(),
                   definitionId,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Definition, DemocriteErrorCodes.PartType.Identifier, DemocriteErrorCodes.ErrorType.Missing),
                   innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingDefinitionException"/> class.
        /// </summary>
        internal MissingDefinitionException(string message,
                                            AbstractType definitionType,
                                            string definitionId,
                                            ulong errorCode,
                                            Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(MissingDefinitionExceptionSurrogate.DefinitionType), definitionType);
            this.Data.Add(nameof(MissingDefinitionExceptionSurrogate.DefinitionId), definitionId);
        }
    }

    [GenerateSerializer]
    public struct MissingDefinitionExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public AbstractType DefinitionType { get; set; }

        [Id(3)]
        public string DefinitionId { get; set; }

        [Id(4)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class MissingDefinitionExceptionConverter : IConverter<MissingDefinitionException, MissingDefinitionExceptionSurrogate>
    {
        public MissingDefinitionException ConvertFromSurrogate(in MissingDefinitionExceptionSurrogate surrogate)
        {
            return new MissingDefinitionException(surrogate.Message,
                                                  surrogate.DefinitionType,
                                                  surrogate.DefinitionId,
                                                  surrogate.ErrorCode,
                                                  surrogate.InnerException);
        }

        public MissingDefinitionExceptionSurrogate ConvertToSurrogate(in MissingDefinitionException value)
        {
            return new MissingDefinitionExceptionSurrogate()
            {
                InnerException = value.InnerException,
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                DefinitionType = (AbstractType)value.Data[nameof(MissingDefinitionExceptionSurrogate.DefinitionType)]!,
                DefinitionId = (string)value.Data[nameof(MissingDefinitionExceptionSurrogate.DefinitionId)]!
            };
        }
    }
}
