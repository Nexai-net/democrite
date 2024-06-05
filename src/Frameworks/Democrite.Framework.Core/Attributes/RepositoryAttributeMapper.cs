// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Attributes
{
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Storages;

    using Elvex.Toolbox.Abstractions.Supports;

    using Microsoft.Extensions.DependencyInjection;

    using Orleans;
    using Orleans.Runtime;

    using System.Diagnostics;
    using System.Reflection;

    /// <summary>
    /// Mapper used to provide the corresponding <see cref="IStorageRepository"/> in function of the parameter requested
    /// </summary>
    /// <seealso cref="IAttributeToFactoryMapper{StorageRepositoryAttribute}" />
    public sealed class RepositoryAttributeMapper : IAttributeToFactoryMapper<RepositoryAttribute>
    {
        #region Fields

        private static readonly MethodInfo s_factoryWithEntityGenericMth;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="RepositoryAttributeMapper"/> class.
        /// </summary>
        static RepositoryAttributeMapper()
        {
            var factoryWithEntityMth = typeof(RepositoryAttributeMapper).GetMethod(nameof(GetFactoryWithEntity), BindingFlags.Static | BindingFlags.NonPublic);
            Debug.Assert(factoryWithEntityMth != null);

            s_factoryWithEntityGenericMth = factoryWithEntityMth.GetGenericMethodDefinition();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Factory<IGrainContext, object> GetFactory(ParameterInfo parameter, RepositoryAttribute metadata)
        {
            var repositoryType = parameter.ParameterType
                                          .GetTypeInfoExtension()
                                          .GetAllCompatibleTypes()
                                          .FirstOrDefault(t => t.IsInterface &&
                                                               t.GetGenericArguments().Length.IsBetween(1, 2) &&
                                                               (t.GetGenericTypeDefinition() == typeof(IReadOnlyRepository<>) ||
                                                                t.GetGenericTypeDefinition() == typeof(IReadOnlyRepository<,>)));

#pragma warning disable IDE0270 // Use coalesce expression

            if (repositoryType == null)
                throw new InvalidCastException("Parameter tag with attribute RepositoryParameter MUST inherite from IReadOnlyRepository<> and be an interface");

#pragma warning restore IDE0270 // Use coalesce expression

            var genericRequestedType = repositoryType.GetGenericArguments();

            Debug.Assert(genericRequestedType != null && genericRequestedType.Length == 1, "Only repository are managed");

            return (Factory<IGrainContext, object>)s_factoryWithEntityGenericMth.MakeGenericMethod(parameter.ParameterType, genericRequestedType.First()).Invoke(null, new[] { metadata })!;
        }

        #region Tools

        /// <inheritdoc />
        private static Factory<IGrainContext, object> GetFactoryWithEntity<TTargetRepo, TEntity>(RepositoryAttribute metadata)
            where TTargetRepo : IReadOnlyRepository<TEntity>
        {
            return ctx =>
            {
                var factory = ctx.ActivationServices.GetRequiredService<IRepositoryFactory>();
                var repositoryValueTask = factory.GetAsync<TTargetRepo, TEntity>(metadata.StateName, metadata.StorageName);

                if (repositoryValueTask.IsCompleted == false)
                {
                    // due to IAttributeToFactoryMapper<RepositoryAttribute> signaure async could not be used
                    // So we have to block the current thread waiting.

                    // Attention : dead lock normally .GetAwaiter().GetResult(); prevent it
                    return repositoryValueTask.GetAwaiter().GetResult();
                }

                return repositoryValueTask.Result;
            };
        }

        #endregion

        #endregion
    }
}
