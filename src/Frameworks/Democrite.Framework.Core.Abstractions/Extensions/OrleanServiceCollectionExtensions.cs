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
        #region Methods

        /// <summary>
        /// Gets the service in the collection indexed to Key
        /// </summary>
        public static ServiceDescriptor? GetServiceByKey<TKey, TService>(this IServiceCollection services, TKey key)
        {
            var serviceKeyed = services.FirstOrDefault(s => s.IsKeyedService && s.ServiceKey is TKey sKey && EqualityComparer<TKey>.Default.Equals(sKey, key));
            return serviceKeyed;
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

            return true;
        }

        #endregion
    }
}
