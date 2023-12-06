// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals
{
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Node.Signals.Doors;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Virtual Grain Service in charge to manager the door initialization and lifecycle
    /// </summary>
    /// <seealso cref="GrainService" />
    /// <seealso cref="IDoorVGrainService" />
    internal sealed class DoorVGrainService : GrainService, IDoorVGrainService
    {
        #region Fields

        private readonly IDoorDefinitionProvider _doorDefinitionProvider;
        private readonly IGrainFactory _grainFactory;

        private readonly CancellationContext _cancellationContext;
        private readonly ILogger _logger;

        #endregion

        #region Ctor 

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorVGrainService"/> class.
        /// </summary>
        public DoorVGrainService(GrainId grainId,
                                   Silo silo,
                                   ILoggerFactory loggerFactory,
                                   IGrainFactory grainFactory,
                                   IDoorDefinitionProvider doorDefinitionProvider)
            : base(grainId, silo, loggerFactory)
        {
            this._cancellationContext = new CancellationContext();

            this._logger = loggerFactory.CreateLogger(nameof(DoorVGrainService));

            this._grainFactory = grainFactory;
            this._doorDefinitionProvider = doorDefinitionProvider;

            this._doorDefinitionProvider.DataChanged -= DoorDefinitionProvider_DataChanged;
            this._doorDefinitionProvider.DataChanged += DoorDefinitionProvider_DataChanged;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override async Task Start()
        {
            await base.Start();
            await LaunchSynpaseVGrainsAsync();
        }

        /// <summary>
        /// Doors the definition provider data changed.
        /// </summary>
        private async void DoorDefinitionProvider_DataChanged(object? sender, EventArgs e)
        {
            await LaunchSynpaseVGrainsAsync();
        }

        #region Tools

        /// <summary>
        /// Launch door handler base on definition
        /// </summary>
        private async Task LaunchSynpaseVGrainsAsync()
        {
            try
            {
                using (var token = this._cancellationContext.Lock())
                {
                    var allDoordDefinitions = await this._doorDefinitionProvider.GetValuesAsync(s => true);

                    foreach (var doorDefinition in allDoordDefinitions)
                    {
                        var grainId = doorDefinition.DoorId.Uid;

                        try
                        {
                            var vgrainInterfaceType = Type.GetType(doorDefinition.VGrainInterfaceFullName, true, true);

                            var grain = this._grainFactory.GetGrain(vgrainInterfaceType, grainId);
                            await grain.AsReference<IDoorVGrain>().InitializeAsync(doorDefinition);
                        }
                        catch (Exception ex)
                        {
                            this._logger.OptiLog(LogLevel.Critical,
                                                 "Could not start door {uid} - {name} due to {exception}",
                                                 doorDefinition.Uid,
                                                 doorDefinition.Name,
                                                 ex);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        #endregion

        #endregion
    }
}
