// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Services
{
    using Democrite.Framework.Core.Abstractions;

    using Orleans.Runtime;
    using Orleans.Runtime.Services;
    using Orleans.Services;

    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Factory in charge to provide handled to other <see cref="IGrainService"/>
    /// </summary>
    /// <seealso cref="Orleans.Runtime.Services.GrainServiceClient&lt;Orleans.Services.IGrainService&gt;" />
    /// <seealso cref="Democrite.Framework.Core.Abstractions.IRemoteGrainServiceFactory" />
    internal sealed class RemoteGrainServiceFactory : IRemoteGrainServiceFactory
    {
        #region Fields

        private readonly ConcurrentDictionary<Type, IProxyGrainService> _proxyCache;
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteGrainServiceFactory"/> class.
        /// </summary>
        public RemoteGrainServiceFactory(IServiceProvider serviceProvider)
        {
            this._proxyCache = new ConcurrentDictionary<Type, IProxyGrainService>();
            this._serviceProvider = serviceProvider;
        }

        #endregion

        #region Nested

        private interface IProxyGrainService
        {
            IGrainService GetGenericGrainServiceHandler(GrainId grainId);
        }

        private sealed class ProxyGrainService<TGrainService> : GrainServiceClient<TGrainService>, IProxyGrainService
            where TGrainService : IGrainService
        {
            public ProxyGrainService(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public IGrainService GetGenericGrainServiceHandler(GrainId grainId)
            {
                return GetGrainServiceHandler(grainId);
            }

            public TGrainService GetGrainServiceHandler(GrainId grainId)
            {
                return base.GetGrainService(grainId);
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public TGrainService GetRemoteGrainService<TGrainService>(GrainId grainId)
            where TGrainService : IGrainService
        {
            return GetRemoteGrainService<TGrainService>(grainId, typeof(TGrainService));
        }

        /// <inheritdoc />
        public TService GetRemoteGrainService<TService>(GrainId grainId, Type type)
        {
            var proxy = this._proxyCache.GetOrAdd(type, t => (IProxyGrainService)Activator.CreateInstance(typeof(ProxyGrainService<>).MakeGenericType(t), new object[] { this._serviceProvider })!);
            return (TService)proxy.GetGenericGrainServiceHandler(grainId);
        }

        #endregion
    }
}
