// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using Elvex.Toolbox.Abstractions.Supports;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Validator used to ensure <see cref="IVGrainId"/> follow the requested rules
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public abstract class VGrainIdBaseValidatorAttribute : Attribute, ISupportDebugDisplayName
    {
        #region Methods

        /// <summary>
        /// Validates the specified grain identifier.
        /// </summary>
        public abstract bool Validate(GrainId grainId, in ReadOnlySpan<char> grainIdStr, ILogger logger);

        /// <inheritdoc />
        public abstract string ToDebugDisplayName();

        #endregion
    }
}
