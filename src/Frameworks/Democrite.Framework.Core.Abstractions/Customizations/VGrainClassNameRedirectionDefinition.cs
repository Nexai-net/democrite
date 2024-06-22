// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Customizations
{
    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Conditions;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define a redirection between a grain interface to another grain with another class name
    /// </summary>
    /// <seealso cref="Equatable{VGrainRedirectionDefinition}" />
    /// <seealso cref="IDefinition" />
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class VGrainClassNameRedirectionDefinition : VGrainRedirectionDefinition, IDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainInterfaceRedirectionDefinition"/> class.
        /// </summary>
        public VGrainClassNameRedirectionDefinition(Guid uid,
                                                    string displayName,
                                                    ConcretType source,
                                                    string redirectClassName,
                                                    DefinitionMetaData? metaData,
                                                    ConditionExpressionDefinition? redirectionCondition = null)
            : base(uid, displayName, source, Enums.VGrainRedirectionTypeEnum.ClassPrefixName, metaData, redirectionCondition)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(redirectClassName);

            this.RedirectClassName = redirectClassName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the redirect.
        /// </summary>
        [Id(0)]
        [DataMember]
        public string RedirectClassName { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return "Redirect " + this.Source + " To class " + this.RedirectClassName + (this.RedirectionCondition is null ? null : this.RedirectionCondition!.ToString());
        }

        /// <summary>
        /// Get a new redirection from <see cref="TSource"/> to <see cref="TRedirect"/>
        /// </summary>
        public static VGrainClassNameRedirectionDefinition Create<TSource>(Type redirectClassName, Expression<Func<IdSpan, string?, bool>>? cond = null, string? displayName = null, DefinitionMetaData? metaData = null)
        {
            return Create<TSource>(redirectClassName.Namespace + "." + redirectClassName.Name, cond, displayName, metaData);
        }

        /// <summary>
        /// Get a new redirection from <see cref="TSource"/> to <see cref="TRedirect"/>
        /// </summary>
        public static VGrainClassNameRedirectionDefinition Create<TSource>(string redirectClassName, Expression<Func<IdSpan, string?, bool>>? cond = null, string? displayName = null, DefinitionMetaData? metaData = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(redirectClassName);

            var def = new VGrainClassNameRedirectionDefinition(Guid.NewGuid(),
                                                               !string.IsNullOrEmpty(displayName) ? displayName : $"{typeof(TSource)} impl {redirectClassName}",
                                                               (ConcretType)typeof(TSource).GetAbstractType(),
                                                               redirectClassName,
                                                               metaData,
                                                               cond?.Serialize());
            def.ValidateWithException();
            return def;
        }

        #region Tools

        /// <inheritdoc />
        protected override bool OnValidate(ILogger logger, bool matchWarningAsError = false)
        {
            if (string.IsNullOrEmpty(this.RedirectClassName))
            {
                logger.OptiLog(LogLevel.Critical, "RedirectClassName must not be null or empty {abstractType}", this.Source);
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        protected override bool OnRedirectionEquals(VGrainRedirectionDefinition other)
        {
            return other is VGrainClassNameRedirectionDefinition otherClassName && 
                string.Equals(otherClassName.RedirectClassName, this.RedirectClassName);
        }

        /// <inheritdoc />
        protected override int OnRedirectionGetHashCode()
        {
            return this.RedirectClassName.GetHashCode();
        }

        #endregion

        #endregion
    }
}
