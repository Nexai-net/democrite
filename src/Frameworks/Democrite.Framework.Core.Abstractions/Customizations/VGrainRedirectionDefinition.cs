// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Customizations
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Conditions;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define a VGRain redirection to used a vgrain instead of another
    /// </summary>
    /// <seealso cref="Equatable{VGrainRedirectionDefinition}" />
    /// <seealso cref="IDefinition" />
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public abstract class VGrainRedirectionDefinition : IEquatable<VGrainRedirectionDefinition>, IDefinition
    {
        #region Fields

        private static readonly ConcretType[] s_expectedConditionParams;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="VGrainRedirectionDefinition"/> class.
        /// </summary>
        static VGrainRedirectionDefinition()
        {
            s_expectedConditionParams = new[]
            {
                (ConcretType)typeof(Type).GetAbstractType(),
                (ConcretType)typeof(object).GetAbstractType(),
                (ConcretType)typeof(IExecutionContext).GetAbstractType(),
                (ConcretType)typeof(string).GetAbstractType(),
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainRedirectionDefinition"/> class.
        /// </summary>
        public VGrainRedirectionDefinition(Guid uid,
                                           string displayName,
                                           ConcretType source,
                                           VGrainRedirectionTypeEnum type,
                                           DefinitionMetaData? metaData,
                                           ConditionExpressionDefinition? redirectionCondition = null)
        {
            ArgumentNullException.ThrowIfNull(source);

            this.Uid = uid;
            this.MetaData = metaData;
            this.DisplayName = displayName;
            this.Source = source;
            this.Type = type;
            this.RedirectionCondition = redirectionCondition;
        }

        #endregion

        #region Properties

        /// <inheritdoc cref="IDefinition.Uid"/>
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        [Id(0)]
        public Guid Uid { get; }

        /// <inheritdoc/>
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        [Id(1)]
        public string DisplayName { get; }

        /// <summary>
        /// Gets the source to redirect fomr
        /// </summary>
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        [Id(2)]
        public ConcretType Source { get; }

        /// <summary>
        /// Gets the redirection condition.
        /// </summary>
        /// <remarks>
        ///     Expect func (Type originTarget, object? Input, IExecutionContext? ctx) => true
        /// </remarks>
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        [Id(3)]
        public ConditionExpressionDefinition? RedirectionCondition { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// </value>
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        [Id(4)]
        public VGrainRedirectionTypeEnum Type { get; }

        /// <summary>
        /// Gets the meta data.
        /// </summary>
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        [Id(5)]
        public DefinitionMetaData? MetaData { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public abstract string ToDebugDisplayName();

        /// <inheritdoc />
        public bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            if (this.RedirectionCondition is not null &&
                this.RedirectionCondition.Parameters.Select(k => k.Type).SequenceEqual(s_expectedConditionParams))
            {
                logger.OptiLog(LogLevel.Critical, "Redirection condition must follow the signature : func(Type originTarget, object? Input, IExecutionContext? ctx, string? grainPrefixExtensions) => true");
                return false;
            }

            return OnValidate(logger, matchWarningAsError);
        }

        /// <summary>
        /// Test if <paramref name="redirectionDefinition"/> conflict with current
        /// </summary>
        public bool Conflict(VGrainRedirectionDefinition redirectionDefinition)
        {
            if (Equals(redirectionDefinition))
                return false;

            return this.Source.Equals(redirectionDefinition.Source) &&
                   (this.RedirectionCondition?.Equals(redirectionDefinition?.RedirectionCondition) ?? redirectionDefinition?.RedirectionCondition is null) &&
                   OnConflict(redirectionDefinition);
        }

        /// <inheritdoc />
        public bool Equals(VGrainRedirectionDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            return this.Source.Equals((AbstractType)other.Source) &&
                   (this.RedirectionCondition?.Equals(other.RedirectionCondition) ?? other.RedirectionCondition is null) &&
                   OnRedirectionEquals(other);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is VGrainInterfaceRedirectionDefinition redirectionDefinition)
                return Equals(redirectionDefinition);
            return false;
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.Source,
                                    this.RedirectionCondition,
                                    OnRedirectionGetHashCode());
        }

        #region Tools

        /// <summary>
        /// Called to check if rule conflict
        /// </summary>
        protected virtual bool OnConflict(VGrainRedirectionDefinition? redirectionDefinition)
        {
            return true;
        }

        /// <inheritdoc cref="Equatable{VGrainRedirectionDefinition}.Equals(VGrainRedirectionDefinition?)" />
        protected abstract bool OnRedirectionEquals(VGrainRedirectionDefinition other);

        /// <inheritdoc cref="Equatable{VGrainRedirectionDefinition}.OnGetHashCode()" />
        protected abstract int OnRedirectionGetHashCode();

        /// <inheritdoc cref="IDefinition.Validate(ILogger, bool)" />
        protected abstract bool OnValidate(ILogger logger, bool matchWarningAsError = false);

        #endregion

        #endregion
    }
}
