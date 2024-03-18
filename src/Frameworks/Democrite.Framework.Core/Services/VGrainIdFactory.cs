// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Elvex.Toolbox.Disposables;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans.Runtime;

    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Factory implementation used to generate <see cref="GrainId"/> base on format <see cref="VGrainIdFormatAttribute"/>
    /// </summary>
    /// <seealso cref="IVGrainIdFactory" />
    public sealed class VGrainIdFactory : SafeDisposable, IVGrainIdFactory
    {
        #region Fields

        private readonly IReadOnlyCollection<IVGrainIdDedicatedFactory> _grainIdDedicatedFactories;
        private readonly IVGrainIdDefaultFactory? _grainIdDefaultFactory;
        private readonly ILogger<IVGrainIdFactory> _defaultLogger;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdFactory"/> class.
        /// </summary>
        public VGrainIdFactory(IEnumerable<IVGrainIdDedicatedFactory> grainIdDedicatedFactories,
                               IVGrainIdDefaultFactory? grainIdDefaultFactory = null,
                               ILogger<IVGrainIdFactory>? logger = null)
        {
            this._defaultLogger = logger ?? NullLogger<VGrainIdFactory>.Instance;

            this._grainIdDedicatedFactories = grainIdDedicatedFactories?.ToArray() ?? EnumerableHelper<IVGrainIdDedicatedFactory>.ReadOnlyArray;
            this._grainIdDefaultFactory = grainIdDefaultFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IVGrainId BuildNewId<TVGrain>(object? input, IExecutionContext? executionContext, ILogger logger) where TVGrain : IVGrain
        {
            return BuildNewId(typeof(TVGrain), input, executionContext, logger);
        }

        /// <inheritdoc />
        public IVGrainId BuildNewId(Type vgrainType, object? input, IExecutionContext? executionContext, ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(vgrainType);

            var localLogger = logger ?? this._defaultLogger;

            var attr = vgrainType.GetCustomAttribute<VGrainIdBaseFormatorAttribute>();

            if (attr is not null)
            {
                foreach (var dedicated in this._grainIdDedicatedFactories)
                {
                    if (dedicated.CanHandled(attr))
                        return dedicated.BuildNewId(attr, vgrainType, input, executionContext, localLogger);
                }
            }

            if (this._grainIdDefaultFactory is not null)
                return this._grainIdDefaultFactory.BuildNewId(vgrainType, input, executionContext, localLogger);

            return LocalDefaultGrainIdProduction(vgrainType);
        }

        /// <summary>
        /// Locals the default grain identifier production.
        /// </summary>
        /// <remarks>
        ///     Used if no default formator have been setups
        /// </remarks>
        private IVGrainId LocalDefaultGrainIdProduction(Type vgrainType)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
