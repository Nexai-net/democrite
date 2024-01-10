// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using System;

    /// <summary>
    /// Raised when a security feature have been broken
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class VGrainSecurityDemocriteException : DemocriteBaseException<VGrainSecurityDemocriteException>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainSecurityDemocriteException"/> class.
        /// </summary>
        public VGrainSecurityDemocriteException(string message, Exception? innerException = null)
            : base(message,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Security),
                   innerException)
        {
        }

        #endregion
    }

    [GenerateSerializer]
    public struct VGrainSecurityDemocriteExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class VGrainSecurityDemocriteExceptionConverter : IConverter<VGrainSecurityDemocriteException, VGrainSecurityDemocriteExceptionSurrogate>
    {
        public VGrainSecurityDemocriteException ConvertFromSurrogate(in VGrainSecurityDemocriteExceptionSurrogate surrogate)
        {
            return new VGrainSecurityDemocriteException(surrogate.Message, surrogate.InnerException);
        }

        public VGrainSecurityDemocriteExceptionSurrogate ConvertToSurrogate(in VGrainSecurityDemocriteException value)
        {
            return new VGrainSecurityDemocriteExceptionSurrogate() 
            {
                Message = value.Message, 
                ErrorCode = value.ErrorCode,
                InnerException = value.InnerException 
            };
        }
    }
}
