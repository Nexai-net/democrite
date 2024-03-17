// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Node.Abstractions.Services;

    using Elvex.Toolbox.Models;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Grain service using local redirection rules
    /// </summary>
    /// <seealso cref="GrainRouteBaseService" />
    internal sealed class GrainRouteFixedService : GrainRouteBaseService, IVGrainRouteScopedService
    {
        #region Fields

        private readonly IReadOnlyDictionary<ConcretType, IReadOnlyCollection<Tuple<VGrainRedirectionDefinition, Func<Type, object?, IExecutionContext?, string?, bool>?>>> _redirectionDefinitions;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GrainRouteFixedService"/> class.
        /// </summary>
        public GrainRouteFixedService(IEnumerable<VGrainRedirectionDefinition>? redirectionDefinitions, IVGrainRouteService? parentRouteService = null)
            : base(parentRouteService)
        {
            this._redirectionDefinitions = GrainRouteBaseService.BuildRedirections(redirectionDefinitions);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override void DisposeBegin()
        {
            if (this._redirectionDefinitions is IDictionary<ConcretType, IReadOnlyCollection<VGrainRedirectionDefinition>> writable)
                writable.Clear();

            base.DisposeBegin();
        }

        /// <summary>
        /// Gets the redirectionsIndex information
        /// </summary>
        protected override IReadOnlyDictionary<ConcretType, IReadOnlyCollection<Tuple<VGrainRedirectionDefinition, Func<Type, object?, IExecutionContext?, string?, bool>?>>> GetRedirections()
        {
            return this._redirectionDefinitions;
        }

        #endregion
    }
}
