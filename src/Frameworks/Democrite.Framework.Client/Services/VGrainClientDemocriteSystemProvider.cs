// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Client.Services
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Services;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class VGrainClientDemocriteSystemProvider : VGrainProvider, IVGrainDemocriteSystemProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainClientDemocriteSystemProvider"/> class.
        /// </summary>
        public VGrainClientDemocriteSystemProvider(IVGrainIdFactory grainIdFactory, IGrainFactory grainFactory)
            : base(grainFactory, grainIdFactory)
        {
            
        }

        #endregion
    }
}
