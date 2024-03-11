// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Resources;
    using Elvex.Toolbox.Models;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Raised when method doesn't have been founded on vgrain
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class VGrainMethodDemocriteException : DemocriteBaseException<VGrainMethodDemocriteException>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainMethodDemocriteException"/> class.
        /// </summary>
        public VGrainMethodDemocriteException(string message, Exception? innerException = null)
            : base(message,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Sequence,
                                             DemocriteErrorCodes.PartType.Reflection,
                                             DemocriteErrorCodes.ErrorType.Invalid),
                   innerException)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Format exception about vgrain method not founded
        /// </summary>
        public static VGrainMethodDemocriteException MethodNotFounded(string signature, AbstractType vgrainType, Exception? innerException = null)
        {
            return new VGrainMethodDemocriteException(string.Format(DemocriteExceptionSR.MethodNotFound, signature, vgrainType), innerException);
        }

        /// <summary>
        /// Format exception about vgrain method not founded
        /// </summary>
        public static VGrainMethodDemocriteException MethodNotFounded(AbstractType vgrainType, AbstractType? output, string methodName, IReadOnlyCollection<AbstractType> argumentTypes)
        {
            var returnSignature = "Task";

            if (output is not null)
                returnSignature = string.Format("Task<{0}>", output);

            var args = "";

            if (argumentTypes != null && argumentTypes.Any())
                args = string.Join(", ", argumentTypes);

            return MethodNotFounded($"{returnSignature} {methodName}({args})", vgrainType);
        }

        #endregion
    }

    [GenerateSerializer]
    public struct VGrainMethodDemocriteExceptionSurrogate : IDemocriteBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class VGrainMethodDemocriteExceptionConverter : IConverter<VGrainMethodDemocriteException, VGrainMethodDemocriteExceptionSurrogate>
    {
        public VGrainMethodDemocriteException ConvertFromSurrogate(in VGrainMethodDemocriteExceptionSurrogate surrogate)
        {
           return new VGrainMethodDemocriteException(surrogate.Message, surrogate.InnerException);
        }

        public VGrainMethodDemocriteExceptionSurrogate ConvertToSurrogate(in VGrainMethodDemocriteException value)
        {
            return new VGrainMethodDemocriteExceptionSurrogate() { Message = value.Message, InnerException = value.InnerException };
        }
    }
}
