// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;
    using Elvex.Toolbox.Models;

    using System;

    /// <summary>
    /// Raised when an vgrain doesn't expose the attribute <see cref="ExpectedVGrainIdFormat"/>
    /// </summary>
    /// <remarks>
    ///     Attention this attribute is not herited.
    /// </remarks>
    public sealed class VGrainIdTemplateMissingException : DemocriteBaseException<VGrainIdTemplateMissingException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdTemplateMissingException"/> class.
        /// </summary>
        public VGrainIdTemplateMissingException(Type vgrainType, Exception? innerException = null)
            : this(DemocriteExceptionSR.VGrainIdTemplateMissingExceptionMessage.WithArguments(vgrainType),
                   vgrainType.GetAbstractType(),
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.VGrain, DemocriteErrorCodes.PartType.MetaInformation, DemocriteErrorCodes.ErrorType.Missing),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdTemplateMissingException"/> class.
        /// </summary>
        internal VGrainIdTemplateMissingException(string message,
                                                  AbstractType vgrainType,
                                                  ulong errorCode,
                                                  Exception? innerException)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(VGrainIdTemplateMissingExceptionSurrogate.VGrainType), vgrainType);
        }
    }

    [GenerateSerializer]
    public struct VGrainIdTemplateMissingExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public Exception? InnerException { get; set; }

        [Id(3)]
        public AbstractType VGrainType { get; set; }
    }

    [RegisterConverter]
    public sealed class VGrainIdTemplateMissingExceptionConverter : IConverter<VGrainIdTemplateMissingException, VGrainIdTemplateMissingExceptionSurrogate>
    {
        public VGrainIdTemplateMissingException ConvertFromSurrogate(in VGrainIdTemplateMissingExceptionSurrogate surrogate)
        {
            return new VGrainIdTemplateMissingException(surrogate.Message,
                                                        surrogate.VGrainType,
                                                        surrogate.ErrorCode,
                                                        surrogate.InnerException);
        }

        public VGrainIdTemplateMissingExceptionSurrogate ConvertToSurrogate(in VGrainIdTemplateMissingException value)
        {
            return new VGrainIdTemplateMissingExceptionSurrogate()
            {
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                InnerException = value.InnerException,
                VGrainType = (AbstractType)value.Data[nameof(VGrainIdTemplateMissingExceptionSurrogate.VGrainType)]!
            };
        }
    }
}
