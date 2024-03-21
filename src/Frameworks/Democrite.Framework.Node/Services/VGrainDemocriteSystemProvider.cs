// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Services;
    using Democrite.Framework.Node.Abstractions.Services;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Democrite.Framework.Core.VGrainProvider" />
    /// <seealso cref="Democrite.Framework.Node.Abstractions.Services.IVGrainDemocriteSystemProvider" />
    internal sealed class VGrainDemocriteSystemProvider : VGrainProvider, IVGrainDemocriteSystemProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainDemocriteSystemProvider"/> class.
        /// </summary>
        public VGrainDemocriteSystemProvider(IGrainOrleanFactory grainFactory,
                                             IVGrainIdFactory vgrainIdFactory) 
            : base(grainFactory, vgrainIdFactory)
        {
        }

        #endregion
    }
}
