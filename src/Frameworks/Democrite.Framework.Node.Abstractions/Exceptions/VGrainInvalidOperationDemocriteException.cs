// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Exceptions;

    /// <summary>
    /// Raised when a not autorize action is done that MUST not arrived
    /// </summary>
    /// <seealso cref="DemocriteBaseException" />
    public sealed class VGrainInvalidOperationDemocriteException : DemocriteBaseException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainMethodDemocriteException"/> class.
        /// </summary>
        public VGrainInvalidOperationDemocriteException(string message, IExecutionContext executionContext)
            : base(message)
        {
            this.Data.Add(nameof(IExecutionContext), executionContext);
        }

        #endregion
    }
}
