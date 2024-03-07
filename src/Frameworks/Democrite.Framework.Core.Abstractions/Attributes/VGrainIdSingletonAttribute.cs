// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using System;

    /// <summary>
    /// Specific how vgrain id must be formated
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class VGrainIdSingletonAttribute : VGrainIdBaseFormatorAttribute
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdFormatAttribute"/> class.
        /// </summary>
        public VGrainIdSingletonAttribute(string? singletonValue = null, IdFormatTypeEnum type = IdFormatTypeEnum.String)
            : base(type)
        {
            this.SingletonValue = singletonValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton value.
        /// </summary>
        public string? SingletonValue { get; }

        #endregion
    }
}
