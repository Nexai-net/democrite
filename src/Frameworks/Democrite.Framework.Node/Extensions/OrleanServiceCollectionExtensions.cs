﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Extensions
{
    using Microsoft.Extensions.DependencyInjection;

    using Orleans.Runtime;

    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    internal static class OrleanServiceCollectionExtensions
    {
        #region Fields

        private static readonly Dictionary<ServiceDescriptor, object?> s_cachedServiceKeyedByServices;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="OrleanServiceCollectionExtensions"/> class.
        /// </summary>
        static OrleanServiceCollectionExtensions()
        {
            s_cachedServiceKeyedByServices = new Dictionary<ServiceDescriptor, object?>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the service in the collection indexed to Key
        /// </summary>
        public static ServiceDescriptor? GetServiceByKey<TKey, TService>(this IServiceCollection services, TKey key)
        {
            var serviceKeyed = services.Where(s => s.ServiceType == typeof(IKeyedService<TKey, TService>))
                                       .ToArray();

            foreach (var service in serviceKeyed)
            {
                var serviceKey = default(TKey);
                object? serviceKeyObj = null;

                if (!s_cachedServiceKeyedByServices.TryGetValue(service, out serviceKeyObj))
                {
                    var keyImpl = service.ImplementationInstance as IKeyedService<TKey, TService>;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    keyImpl ??= (IKeyedService<TKey, TService>)service.ImplementationFactory(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    if (keyImpl == null)
                        continue;

                    s_cachedServiceKeyedByServices.Add(service, keyImpl.Key as object);

                    serviceKey = keyImpl.Key;
                }

                if (serviceKeyObj is TKey cachedKey)
                    serviceKey = cachedKey;

                if (EqualityComparer<TKey>.Default.Equals(serviceKey, key))
                    return service;
            }

            return null;
        }

        #endregion
    }
}
