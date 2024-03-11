﻿// Copyright (c) Nexai.
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
    /// Define a redirection between a grain interface to another
    /// </summary>
    /// <seealso cref="Equatable{VGrainRedirectionDefinition}" />
    /// <seealso cref="IDefinition" />
    [Immutable]
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public sealed class VGrainInterfaceRedirectionDefinition : VGrainRedirectionDefinition, IDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainInterfaceRedirectionDefinition"/> class.
        /// </summary>
        public VGrainInterfaceRedirectionDefinition(Guid uid,
                                                    string displayName,
                                                    AbstractType source,
                                                    AbstractType redirect,
                                                    ConditionExpressionDefinition? redirectionCondition = null)
            : base(uid, displayName, source, redirectionCondition)
        {
            ArgumentNullException.ThrowIfNull(redirect);

            this.Redirect = redirect;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the redirect.
        /// </summary>
        public AbstractType Redirect { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return "Redirect " + this.Source + " To " + this.Redirect + (this.RedirectionCondition is null ? null : this.RedirectionCondition!.ToString());
        }

        /// <summary>
        /// Get a new redirection from <see cref="TSource"/> to <see cref="TRedirect"/>
        /// </summary>
        public static VGrainInterfaceRedirectionDefinition Create<TSource, TRedirect>(Expression<Func<IdSpan, string?, bool>>? cond = null, string? displayName = null)
            where TRedirect : TSource
        {
            return new VGrainInterfaceRedirectionDefinition(Guid.NewGuid(),
                                                            !string.IsNullOrEmpty(displayName) ? displayName : $"{typeof(TSource)} -> {typeof(TRedirect)}",
                                                            typeof(TSource).GetAbstractType(),
                                                            typeof(TRedirect).GetAbstractType(),
                                                            cond?.Serialize());
        }

        #region Tools

        /// <inheritdoc />
        protected override bool OnValidate(ILogger logger, bool matchWarningAsError = false)
        {
            var redirectType = this.Redirect.ToType();
            var sourceType = this.Source.ToType();

            if (sourceType is null)
            {
                logger.OptiLog(LogLevel.Critical, "Redirection source type could not be restored {abstractType}", this.Source);
                return false;
            }

            if (redirectType is null)
            {
                logger.OptiLog(LogLevel.Critical, "Redirection redirect type could not be restored {abstractType}", this.Redirect);
                return false;
            }

            if (redirectType.IsAssignableTo(sourceType))
            {
                logger.OptiLog(LogLevel.Critical, "Redirect type {redirectType} must inherite from {abstractType}", this.Redirect, this.Source);
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        protected override bool OnRedirectionEquals(VGrainRedirectionDefinition other)
        {
            return other is VGrainInterfaceRedirectionDefinition otherInterface && this.Redirect.Equals(otherInterface.Redirect);
        }

        /// <inheritdoc />
        protected override int OnRedirectionGetHashCode()
        {
            return this.Redirect.GetHashCode();
        }

        #endregion

        #endregion
    }
}
