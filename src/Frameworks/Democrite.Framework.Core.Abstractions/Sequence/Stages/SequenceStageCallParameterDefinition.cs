// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence.Stages
{
    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Expressions;
    using Elvex.Toolbox.Abstractions.Supports;

    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class SequenceStageCallParameterDefinition : Equatable<SequenceStageCallParameterDefinition>, ISupportDebugDisplayName
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceStageCallParameterDefinition"/> class.
        /// </summary>
        public SequenceStageCallParameterDefinition(int position, string paramName, AccessExpressionDefinition access)
        {
            this.Position = position;
            this.ParamName = paramName;
            this.Access = access;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the argument's position call.
        /// </summary>
        [Id(0)]
        [DataMember]
        public int Position { get; }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        [Id(1)]
        [DataMember]
        public string ParamName { get; }

        /// <summary>
        /// Gets the access.
        /// </summary>
        [Id(2)]
        [DataMember]
        public AccessExpressionDefinition Access { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return "[{1}] Param {0} - {2}".WithArguments(this.Position, this.ParamName, this.Access.ToDebugDisplayName());
        }

        /// <inheritdoc />
        protected override bool OnEquals([NotNull] SequenceStageCallParameterDefinition other)
        {
            return this.Position == other.Position &&
                   string.Equals(this.ParamName, other.ParamName) &&
                   (this.Access?.Equals(other.Access) ?? other.Access is null);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.Position, this.ParamName, this.Access);
        }

        #endregion
    }
}
