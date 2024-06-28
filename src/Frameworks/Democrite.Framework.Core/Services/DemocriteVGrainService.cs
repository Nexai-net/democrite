// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Base Safe class of GRainServier
    /// </summary>
    /// <seealso cref="Orleans.Runtime.GrainService" />
    public abstract class DemocriteVGrainService : GrainService
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteVGrainService"/> class.
        /// </summary>
        protected DemocriteVGrainService(GrainId grainId,
                                         Silo silo,
                                         ILoggerFactory loggerFactory)
            : base(grainId, silo, loggerFactory)
        {

            this.Logger = loggerFactory.CreateLogger("[VGrain Service : " + GetType().Name + "] [Silo: '" + silo.SiloAddress + "']");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public sealed override async Task Start()
        {
            try
            {
                this.Logger.OptiLog(LogLevel.Debug, "Grain Service Started ...");

                await RefreshInfoAsync();

                this.Logger.OptiLog(LogLevel.Information, "Grain Service Start ...");
            }
            catch (Exception ex)
            {
                this.Logger.OptiLog(LogLevel.Error, "VGrain Service Start Failed : {exception}", ex);
            }
        }

        /// <inheritdoc cref="GrainService.Start" />
        protected abstract Task RefreshInfoAsync();

        #endregion
    }
}
