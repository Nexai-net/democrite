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
    public sealed class DefinitionException : DemocriteBaseException<DefinitionException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionException"/> class.
        /// </summary>
        public DefinitionException(Type definitionType,
                                   string definitionId,
                                   string errorDetails,
                                   ulong? customerrorCode = null,
                                   Exception? innerException = null)
            : this(DemocriteExceptionSR.DefinitionExceptionMessage.WithArguments(definitionType, definitionId, errorDetails),
                   definitionType.GetAbstractType(),
                   definitionId,
                   customerrorCode ?? DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Definition, DemocriteErrorCodes.PartType.Identifier, DemocriteErrorCodes.ErrorType.Missing),
                   innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionException"/> class.
        /// </summary>
        internal DefinitionException(string message,
                                     AbstractType definitionType,
                                     string definitionId,
                                     ulong errorCode,
                                     Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(DefinitionExceptionSurrogate.DefinitionType), definitionType);
            this.Data.Add(nameof(DefinitionExceptionSurrogate.DefinitionId), definitionId);
        }
    }

    [GenerateSerializer]
    public struct DefinitionExceptionSurrogate : IDemocriteBaseExceptionSurrogate
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
    public sealed class DefinitionExceptionConverter : IConverter<DefinitionException, DefinitionExceptionSurrogate>
    {
        public DefinitionException ConvertFromSurrogate(in DefinitionExceptionSurrogate surrogate)
        {
            return new DefinitionException(surrogate.Message,
                                                  surrogate.DefinitionType,
                                                  surrogate.DefinitionId,
                                                  surrogate.ErrorCode,
                                                  surrogate.InnerException);
        }

        public DefinitionExceptionSurrogate ConvertToSurrogate(in DefinitionException value)
        {
            return new DefinitionExceptionSurrogate()
            {
                InnerException = value.InnerException,
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                DefinitionType = (AbstractType)value.Data[nameof(DefinitionExceptionSurrogate.DefinitionType)]!,
                DefinitionId = (string)value.Data[nameof(DefinitionExceptionSurrogate.DefinitionId)]!
            };
        }
    }
}
