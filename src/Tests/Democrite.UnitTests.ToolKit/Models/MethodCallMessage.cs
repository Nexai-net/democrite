// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Models
{
    using Elvex.Toolbox.Abstractions.Models;

    /// <summary>
    /// Method call argument
    /// </summary>
    public sealed class MethodCallMessage
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallMessage"/> class.
        /// </summary>
        public MethodCallMessage(string methodName,
                                 string methodUniqueId,
                                 TypedArgument? argumentRoot,
                                 string? returnTypeAssemblyName)
        {
            this.MethodName = methodName;
            this.MethodUniqueId = methodUniqueId;
            this.ArgumentRoot = argumentRoot;
            this.ReturnTypeAssemblyName = returnTypeAssemblyName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Gets the name of the method unique id generate by <see cref="Elvex.Toolbox.Extensions.ReflectionExtensions.GetUniqueId"/>.
        /// </summary>
        public string MethodUniqueId { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public TypedArgument? ArgumentRoot { get; }

        /// <summary>
        /// Gets the name of the return type assembly.
        /// </summary>
        public string? ReturnTypeAssemblyName { get; }

        #endregion
    }
}
