// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Customizations
{
    using Democrite.Framework.Toolbox;
    using Democrite.Framework.Toolbox.Abstractions.Conditions;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Models;

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
    public abstract class VGrainRedirectionDefinition : Equatable<VGrainRedirectionDefinition>, IDefinition
    {
        #region Fields

        private static readonly AbstractType s_idSpanType;
        private static readonly AbstractType s_stringType;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="VGrainRedirectionDefinition"/> class.
        /// </summary>
        static VGrainRedirectionDefinition()
        {
            s_idSpanType = typeof(IdSpan).GetAbstractType();
            s_stringType = typeof(string).GetAbstractType();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainRedirectionDefinition"/> class.
        /// </summary>
        public VGrainRedirectionDefinition(Guid uid,
                                           string displayName,
                                           AbstractType source,
                                           ConditionExpressionDefinition? redirectionCondition = null)
        {
            ArgumentNullException.ThrowIfNull(source);

            this.Uid = uid;
            this.DisplayName = displayName;
            this.Source = source;
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
        public AbstractType Source { get; }

        /// <summary>
        /// Gets the redirection condition.
        /// </summary>
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        [Id(3)]
        public ConditionExpressionDefinition? RedirectionCondition { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public abstract string ToDebugDisplayName();

        /// <inheritdoc />
        public bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            if (this.RedirectionCondition is not null &&
                (this.RedirectionCondition.Parameters.Count != 2 || 
                 this.RedirectionCondition.Parameters.First().Type != s_idSpanType ||
                 this.RedirectionCondition.Parameters.Last().Type != s_stringType))
            {
                logger.OptiLog(LogLevel.Critical, "Redirection condition must take only one parameter type GrainId");
                return false;
            }

            return OnValidate(logger, matchWarningAsError);
        }

        /// <inheritdoc cref="Equatable{VGrainRedirectionDefinition}.Equals(VGrainRedirectionDefinition?)" />
        protected abstract bool OnRedirectionEquals(VGrainRedirectionDefinition other);

        /// <inheritdoc cref="Equatable{VGrainRedirectionDefinition}.OnGetHashCode()" />
        protected abstract int OnRedirectionGetHashCode();

        /// <inheritdoc cref="IDefinition.Validate(ILogger, bool)" />
        protected abstract bool OnValidate(ILogger logger, bool matchWarningAsError = false);

        /// <inheritdoc />
        protected sealed override bool OnEquals([NotNull] VGrainRedirectionDefinition other)
        {
            return this.Source.Equals(other.Source) &&
                   (this.RedirectionCondition?.Equals(other.RedirectionCondition) ?? other.RedirectionCondition is null) &&
                   OnRedirectionEquals(other);
        }

        /// <inheritdoc />
        protected sealed override int OnGetHashCode()
        {
            return HashCode.Combine(this.Source, 
                                    this.RedirectionCondition,
                                    OnRedirectionGetHashCode());
        }

        #endregion
    }
}
