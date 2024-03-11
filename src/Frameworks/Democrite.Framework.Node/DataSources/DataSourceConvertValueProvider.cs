// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.DataSources
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Node.Inputs;
    using Elvex.Toolbox.Extensions;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provide dedicated value
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <seealso cref="DataSourceBaseProvider{DataSourceFixValueDefinition{TData}}" />
    internal sealed class DataSourceConvertValueProvider<TData> : DataSourceBaseProvider<DataSourceConvertValueDefinition<TData>>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceConvertValueProvider{TData}"/> class.
        /// </summary>
        public DataSourceConvertValueProvider(DataSourceConvertValueDefinition<TData> definition) 
            : base(definition)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override ValueTask<object?> GetNextAsync(object? sourceValue, CancellationToken token = default)
        {
            var result = this.Definition.Access.Resolve(sourceValue);
            return ValueTask.FromResult<object?>(result);
        }

        #endregion
    }
}
