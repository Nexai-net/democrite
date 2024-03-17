// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Inputs
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Node.Abstractions.Inputs;
    using Democrite.Framework.Node.DataSources;

    using Microsoft.Extensions.DependencyInjection;

    using System;
    using System.Collections.Frozen;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Factory used to produce a "consumer" associate to a <see cref="DataSourceDefinition"/> to be able to produce data on demand
    /// </summary>
    /// <seealso cref="IDataSourceProviderFactory" />
    public sealed class DataSourceProviderFactory : IDataSourceProviderFactory
    {
        #region Fields

        private static readonly IReadOnlyDictionary<DataSourceTypeEnum, Func<DataSourceProviderFactory, DataSourceDefinition, IDataSourceProvider>> s_builders;
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceProviderFactory"/> class.
        /// </summary>
        static DataSourceProviderFactory()
        {
            s_builders = new Dictionary<DataSourceTypeEnum, Func<DataSourceProviderFactory, DataSourceDefinition, IDataSourceProvider>>()
            {
                { DataSourceTypeEnum.StaticCollection, (factory, def) => factory.BuildProvider<DataSourceStaticCollectionProxyProvider<int>>(def) },
                { DataSourceTypeEnum.FixValue, (factory, def) => factory.BuildProvider<DataSourceFixValueProvider<int>>(def) },
                { DataSourceTypeEnum.Convert, (factory, def) => factory.BuildProvider<DataSourceConvertValueProvider<int>>(def) }
            }.ToFrozenDictionary();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceProviderFactory"/> class.
        /// </summary>
        public DataSourceProviderFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IDataSourceProvider GetProvider(DataSourceDefinition inputSourceDefinition)
        {
            ArgumentNullException.ThrowIfNull(inputSourceDefinition);

            if (s_builders.TryGetValue(inputSourceDefinition.InputSourceType, out var builder))
                return builder.Invoke(this, inputSourceDefinition);

            throw new NotSupportedException(inputSourceDefinition.InputSourceType + " is not supported yet by the factory.");
        }

        /// <inheritdoc />
        public ValueTask<bool> IsStillValidAsync(IDataSourceProvider? provider, DataSourceDefinition inputSourceDefinition, CancellationToken token = default)
        {
            if (provider == null)
                return ValueTask.FromResult(false);

            return provider.IsStillValidAsync(inputSourceDefinition, token);
        }

        #region Tools

        /// <summary>
        /// Builds provider based on <see cref="DataSourceStaticCollectionDefinition{TInputType}"/>.
        /// </summary>
        private IDataSourceProvider BuildProvider<TProviderGeneric>(DataSourceDefinition definition)
        {
            var dataType = definition.DataType;
            return (IDataSourceProvider)ActivatorUtilities.CreateInstance(this._serviceProvider,
                                                                          typeof(TProviderGeneric).GetGenericTypeDefinition().MakeGenericType(dataType.ToType()),
                                                                          definition)!;
        }

        #endregion

        #endregion
    }
}
