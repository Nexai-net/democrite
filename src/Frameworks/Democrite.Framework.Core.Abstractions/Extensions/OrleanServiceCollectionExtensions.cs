// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Microsoft.Extensions.DependencyInjection
{
    using Orleans.Runtime;

    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extensions method use to easy service manipulation
    /// </summary>
    public static class OrleanServiceCollectionExtensions
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
            var serviceKeyed = services.FirstOrDefault(s => s.IsKeyedService && s.ServiceKey is TKey sKey && EqualityComparer<TKey>.Default.Equals(sKey, key));
            return serviceKeyed;

//            foreach (var service in serviceKeyed)
//            {
//                var serviceKey = default(TKey);
//                object? serviceKeyObj = null;

//                if (!s_cachedServiceKeyedByServices.TryGetValue(service, out serviceKeyObj))
//                {
//                    var keyImpl = service.ImplementationInstance as IKeyedService<TKey, TService>;

//#pragma warning disable CS8602 // Dereference of a possibly null reference.
//#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
//                    keyImpl ??= (IKeyedService<TKey, TService>)service.ImplementationFactory(null);
//#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
//#pragma warning restore CS8602 // Dereference of a possibly null reference.

//                    if (keyImpl == null)
//                        continue;

//                    s_cachedServiceKeyedByServices.Add(service, keyImpl.Key as object);

//                    serviceKey = keyImpl.Key;
//                }

//                if (serviceKeyObj is TKey cachedKey)
//                    serviceKey = cachedKey;

//                if (EqualityComparer<TKey>.Default.Equals(serviceKey, key))
//                    return service;
//            }

//            return null;
        }

        /// <summary>
        /// Removes the keyed service.
        /// </summary>
        public static bool RemoveKeyedService<TKey, TService>(this IServiceCollection services, TKey key)
        {
            var desc = GetServiceByKey<TKey, TService>(services, key);

            if (desc == null)
                return false;

            services.Remove(desc);
            s_cachedServiceKeyedByServices.Remove(desc);

            return true;

        }

        #endregion
    }
}
