// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Abstractions.Resources;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Raised when method doesn't have been founded on vgrain
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class VGrainMethodDemocriteException : DemocriteBaseException
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
        public static VGrainMethodDemocriteException MethodNotFounded(string signature, Type vgrainType, Exception? innerException = null)
        {
            return new VGrainMethodDemocriteException(string.Format(NodeAbstractionExceptionSR.MethodNotFound, signature, vgrainType), innerException);
        }

        /// <summary>
        /// Format exception about vgrain method not founded
        /// </summary>
        public static VGrainMethodDemocriteException MethodNotFounded(Type vgrainType, Type? output, string methodName, IReadOnlyCollection<Type> argumentTypes)
        {
            var returnSignature = "Task";

            if (output != null)
                returnSignature = string.Format("Task<{0}>", output);

            var args = "";

            if (argumentTypes != null && argumentTypes.Any())
                args = string.Join(", ", argumentTypes);

            return MethodNotFounded($"{returnSignature} {methodName}({args})", vgrainType);
        }

        #endregion
    }
}
