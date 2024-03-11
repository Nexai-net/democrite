// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Resources;
    using Elvex.Toolbox.Models;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Raised when a <see cref="VGrainIdBaseValidatorAttribute"/> failed
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class VGrainIdValidationFaildException : DemocriteBaseException<VGrainIdValidationFaildException>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdValidationFaildException"/> class.
        /// </summary>
        public VGrainIdValidationFaildException(VGrainIdBaseValidatorAttribute attribute,
                                                Type grain,
                                                GrainId grainId,
                                                Exception? innerException = null)
            : this(DemocriteExceptionSR.VGrainIdValidationFaildExceptionMessage.WithArguments(attribute.ToDebugDisplayName(), grain.Name, grainId),
                   grain.GetAbstractType(),
                   grainId.ToString(),
                   attribute.ToDebugDisplayName(),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.VGrain, DemocriteErrorCodes.PartType.Identifier, DemocriteErrorCodes.ErrorType.Invalid),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdValidationFaildException"/> class.
        /// </summary>
        internal VGrainIdValidationFaildException(string message,
                                                  AbstractType grainType,
                                                  string grainId,
                                                  string validatorDisplayName,
                                                  ulong errorCode,
                                                  Exception? innerException)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(VGrainIdValidationFaildExceptionSurrogate.GrainId), grainId);
            this.Data.Add(nameof(VGrainIdValidationFaildExceptionSurrogate.GrainType), grainType);
            this.Data.Add(nameof(VGrainIdValidationFaildExceptionSurrogate.validatorDisplayName), validatorDisplayName);
        }

        #endregion
    }

    [GenerateSerializer]
    public struct VGrainIdValidationFaildExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public Exception? InnerException { get; set; }

        [Id(3)]
        public AbstractType GrainType { get; set; }

        [Id(4)]
        public string GrainId { get; set; }

        [Id(5)]
        public string validatorDisplayName { get; set; }
    }

    [RegisterConverter]
    public sealed class VGrainIdValidationFaildExceptionConverter : IConverter<VGrainIdValidationFaildException, VGrainIdValidationFaildExceptionSurrogate>
    {
        /// <inheritdoc />
        public VGrainIdValidationFaildException ConvertFromSurrogate(in VGrainIdValidationFaildExceptionSurrogate surrogate)
        {
            return new VGrainIdValidationFaildException(surrogate.Message,
                                                        surrogate.GrainType,
                                                        surrogate.GrainId,
                                                        surrogate.validatorDisplayName,
                                                        surrogate.ErrorCode,
                                                        surrogate.InnerException);
        }

        /// <inheritdoc />
        public VGrainIdValidationFaildExceptionSurrogate ConvertToSurrogate(in VGrainIdValidationFaildException value)
        {
            return new VGrainIdValidationFaildExceptionSurrogate()
            {
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                InnerException = value.InnerException,
                GrainId = (string)value.Data[nameof(VGrainIdValidationFaildExceptionSurrogate.GrainId)]!,
                GrainType = (AbstractType)value.Data[nameof(VGrainIdValidationFaildExceptionSurrogate.GrainType)]!,
                validatorDisplayName = (string)value.Data[nameof(VGrainIdValidationFaildExceptionSurrogate.validatorDisplayName)]!
            };
        }
    }
}
