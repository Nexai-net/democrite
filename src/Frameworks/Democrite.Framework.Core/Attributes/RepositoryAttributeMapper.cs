// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Attributes
{
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Storages;

    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans;
    using Orleans.Runtime;

    using System.Diagnostics;
    using System.Linq.Expressions;
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
            Expression<Func<RepositoryAttribute, Factory<IGrainContext, object>>> factoryFunc = m => GetFactoryWithEntity<IReadOnlyRepository<SequenceDefinition, Guid>, SequenceDefinition, Guid>(m);
            s_factoryWithEntityGenericMth = ((MethodCallExpression)factoryFunc.Body).Method.GetGenericMethodDefinition();
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
                                                               t.GetGenericArguments().Length == 2 &&
                                                               t.GetGenericTypeDefinition().IsAssignableTo(typeof(IReadOnlyRepository<,>)));

#pragma warning disable IDE0270 // Use coalesce expression

            if (repositoryType == null)
                throw new InvalidCastException("Parameter tag with attribute RepositoryParameter MUST inherite from IReadOnlyRepository<> and be an interface");

#pragma warning restore IDE0270 // Use coalesce expression

            var genericRequestedType = parameter.ParameterType.AsEnumerable()
                                                .Concat(repositoryType.GetGenericArguments())
                                                .ToArray();

            var factory = (Factory<IGrainContext, object>)s_factoryWithEntityGenericMth.MakeGenericMethod(genericRequestedType).Invoke(null, new[] { metadata })!;
            return factory;
        }

        #region Tools

        /// <inheritdoc />
        private static Factory<IGrainContext, object> GetFactoryWithEntity<TTargetRepo, TEntity, TEntityId>(RepositoryAttribute metadata)
            where TEntity : IEntityWithId<TEntityId>
            where TEntityId : notnull, IEquatable<TEntityId>
            where TTargetRepo : IReadOnlyRepository<TEntity, TEntityId>
        {
            return ctx =>
            {
                try
                {
                    var factory = ctx.ActivationServices.GetRequiredService<IRepositoryFactory>();

                    var request = new RepositoryGetOptions(metadata.StorageName,
                                                           typeof(TTargetRepo).IsAssignableTo(typeof(IRepository<TEntity, TEntityId>)) == false,
                                                           metadata.ConfigurationName,
                                                           metadata.PreventAnyKindOfDiscriminatorUsage);

                    var repository = factory.Get<TTargetRepo, TEntity, TEntityId>(request);

                    return repository;
                }
                catch (Exception ex)
                {
                    var logger = ctx.ActivationServices.GetService<ILogger<RepositoryAttributeMapper>>() ?? NullLogger<RepositoryAttributeMapper>.Instance;

                    logger.OptiLog(LogLevel.Error, "Failed RepositoryAttributeMapper {exception}", ex);

                    throw;
                }
            };
        }

        #endregion

        #endregion
    }
}
