// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence.Stages
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Toolbox.Abstractions.Supports;

    using Newtonsoft.Json;

    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Stage related to VGrain call information
    /// </summary>
    /// <seealso cref="ISequenceStageDefinition" />
    [Serializable]
    [ImmutableObject(true)]
    public sealed class SequenceStageCallDefinition : SequenceStageBaseDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceStageCallDefinition"/> class.
        /// </summary>
        public SequenceStageCallDefinition(Type? input,
                                           Type vgrainType,
                                           CallMethodDefinition callMethodDefinition,
                                           Type? output,
                                           Type? contextInfoType = null,
                                           object? contextInfo = null,
                                           SequenceOptionStageDefinition? options = null,
                                           bool preventReturn = false,
                                           Guid? uid = null)
            : base(StageTypeEnum.Call, input, output, options, preventReturn, uid)
        {
            ArgumentNullException.ThrowIfNull(callMethodDefinition);

            this.CallMethodDefinition = callMethodDefinition;

            this.VGrainType = vgrainType;

            this.ConfigurationType = contextInfoType;
            this.ConfigurationInfo = contextInfo;

            string genericArgs = string.Empty;

            if (callMethodDefinition?.GenericImplementationTypes != null && callMethodDefinition.GenericImplementationTypes.Any())
                genericArgs = "<" + string.Join(',', callMethodDefinition.GenericImplementationTypes.Select(t => t.Name)) + ">";

            this.UniqueKey = $"{vgrainType.FullName}.{callMethodDefinition!.Name}{genericArgs}({string.Join(',', callMethodDefinition.Arguments.Select(p => string.IsNullOrEmpty(p.FullName) ? p.Name : p.FullName))})";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the method information.
        /// </summary>
        public CallMethodDefinition CallMethodDefinition { get; }

        /// <summary>
        /// Gets the type of the vgrain.
        /// </summary>
        public Type VGrainType { get; }

        /// <summary>
        /// Gets the type of the context information.
        /// </summary>
        public Type? ConfigurationType { get; }

        /// <summary>
        /// Gets the context information.
        /// </summary>
        public object? ConfigurationInfo { get; }

        /// <summary>
        /// Gets a unique key resuming the informations.
        /// </summary>
        /// <remarks>
        ///     Used for <see cref="ISupportDebugDisplayName"/> and cache
        /// </remarks>
        [JsonIgnore]
        [IgnoreDataMember]
        public string UniqueKey { get; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override bool OnStageEquals(ISequenceStageDefinition other)
        {
            return other is SequenceStageCallDefinition callOther &&
                   callOther.CallMethodDefinition == this.CallMethodDefinition &&
                   callOther.VGrainType == this.VGrainType;
        }

        /// <inheritdoc/>
        protected override int OnStageGetHashCode()
        {
            return this.CallMethodDefinition.GetHashCode() ^
                   this.VGrainType.GetHashCode();
        }

        #endregion
    }
}
