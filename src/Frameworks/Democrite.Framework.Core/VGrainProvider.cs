// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Models;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// VGrain provider
    /// </summary>
    /// <seealso cref="ISequenceVGrainProvider" />
    internal class VGrainProvider : IVGrainProvider
    {
        #region Fields

        private readonly IVGrainIdFactory _vgrainIdFactory;
        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainProvider"/> class.
        /// </summary>
        public VGrainProvider(IGrainFactory grainFactory, IVGrainIdFactory vgrainIdFactory)
        {
            this._grainFactory = grainFactory;
            this._vgrainIdFactory = vgrainIdFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask<TVGrainType> GetVGrainAsync<TVGrainType>()
            where TVGrainType : IVGrain
        {
            return GetVGrainAsync<TVGrainType>(null, null, null);
        }

        /// <inheritdoc />
        public ValueTask<TVGrainType> GetVGrainAsync<TVGrainType>(IExecutionContext? executionContext, ILogger? logger = null)
            where TVGrainType : IVGrain
        {
            return GetVGrainAsync<TVGrainType>(null, executionContext, logger);
        }

        /// <inheritdoc />
        public ValueTask<TVGrainType> GetVGrainAsync<TVGrainType>(object? input, IExecutionContext? executionContext, ILogger? logger = null)
            where TVGrainType : IVGrain
        {
            var vgrain = GetVGrainImpl(typeof(TVGrainType), input, executionContext, logger);
            return ValueTask.FromResult<TVGrainType>((TVGrainType)vgrain);
        }

        /// <inheritdoc />
        public ValueTask<IVGrain> GetVGrainAsync(Type vgrainInterfaceType, IExecutionContext? executionContext, ILogger? logger = null)
        {
            return GetVGrainAsync(vgrainInterfaceType, null, executionContext, logger);
        }

        /// <inheritdoc />
        public ValueTask<IVGrain> GetVGrainAsync(Type vgrainInterfaceType)
        {
            return GetVGrainAsync(vgrainInterfaceType, null, null, null);
        }

        /// <inheritdoc />
        public ValueTask<IVGrain> GetVGrainAsync(Type vgrainInterfaceType, object? input, IExecutionContext? executionContext, ILogger? logger = null)
        {
            var vgrain = GetVGrainImpl(vgrainInterfaceType, input, executionContext, logger);
            return ValueTask.FromResult((IVGrain)vgrain);
        }

        /// <inheritdoc />
        public ValueTask<IVGrain> GetVGrainWithConfigAsync<TConfig>(Type vgrainInterfaceType, TConfig? executionContextConfig, ILogger? logger = null)
        {
            return GetVGrainAsync(vgrainInterfaceType, ExecutionContextWithConfiguration<TConfig>.EmptyWithConfig(executionContextConfig), logger);
        }

        /// <inheritdoc />
        public ValueTask<IVGrain> GetVGrainWithConfigAsync<TConfig>(Type vgrainInterfaceType, object? input, TConfig? executionContextConfig, ILogger? logger = null)
        {
            return GetVGrainAsync(vgrainInterfaceType, input, ExecutionContextWithConfiguration<TConfig>.EmptyWithConfig(executionContextConfig), logger);
        }

        /// <inheritdoc />
        public ValueTask<TVGrainType> GetVGrainWithConfigAsync<TVGrainType, TConfig>(TConfig? executionContextConfig, ILogger? logger = null) where TVGrainType : IVGrain
        {
            return GetVGrainAsync<TVGrainType>(ExecutionContextWithConfiguration<TConfig>.EmptyWithConfig(executionContextConfig), logger);
        }

        /// <inheritdoc />
        public ValueTask<TVGrainType> GetVGrainWithConfigAsync<TVGrainType, TConfig>(object? input, TConfig? executionContextConfig, ILogger? logger = null) where TVGrainType : IVGrain
        {
            return GetVGrainAsync<TVGrainType>(input, ExecutionContextWithConfiguration<TConfig>.EmptyWithConfig(executionContextConfig), logger);
        }

        #region Tools

        /// <summary>
        /// Gets the vgrain orlean proxy to consume the vgrain
        /// </summary>
        private IVGrain GetVGrainImpl(Type vgrainType, object? input, IExecutionContext? executionContext, ILogger? logger = null)
        {
            var grainId = this._vgrainIdFactory.BuildNewId(vgrainType,
                                                           input,
                                                           executionContext,
                                                           logger ?? NullLogger.Instance);

            IGrain vgrain;

            var extension = grainId.GetVGrainIdExtensionParameter();

            // Optimize : could be cleaner in a Dictionary
            switch (grainId.IdFormatType)
            {
                case IdFormatTypeEnum.String:
                    vgrain = this._grainFactory.GetGrain(vgrainType,
                                                         grainId.GetVGrainIdPrimaryValue<string>());
                    break;

                case IdFormatTypeEnum.CompositionStringString:
                    vgrain = this._grainFactory.GetGrain(vgrainType,
                                                         grainId.GetVGrainIdPrimaryValue<string>() + (!string.IsNullOrEmpty(extension) ? ":" + extension : null));
                    break;

                case IdFormatTypeEnum.Guid:
                    vgrain = this._grainFactory.GetGrain(vgrainType,
                                                        grainId.GetVGrainIdPrimaryValue<Guid>());
                    break;

                case IdFormatTypeEnum.CompositionGuidString:
                    vgrain = this._grainFactory.GetGrain(vgrainType,
                                                         grainId.GetVGrainIdPrimaryValue<Guid>(),
                                                         extension);
                    break;

                case IdFormatTypeEnum.Long:
                    vgrain = this._grainFactory.GetGrain(vgrainType,
                                                         grainId.GetVGrainIdPrimaryValue<long>());
                    break;

                case IdFormatTypeEnum.CompositionLongString:
                    vgrain = this._grainFactory.GetGrain(vgrainType,
                                                         grainId.GetVGrainIdPrimaryValue<long>(),
                                                         extension);
                    break;

                default:
                    throw new InvalidOperationException(grainId.VGrainType + "not managed");
            }

            return (IVGrain)vgrain;
        }

        #endregion

        #endregion
    }
}
