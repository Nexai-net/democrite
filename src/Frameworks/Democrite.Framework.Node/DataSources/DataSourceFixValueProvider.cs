// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.DataSources
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Node.Inputs;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provide dedicated value
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <seealso cref="DataSourceBaseProvider{DataSourceFixValueDefinition{TData}}" />
    internal sealed class DataSourceFixValueProvider<TData> : DataSourceBaseProvider<DataSourceFixValueDefinition<TData>>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceFixValueProvider{TData}"/> class.
        /// </summary>
        public DataSourceFixValueProvider(DataSourceFixValueDefinition<TData> definition) 
            : base(definition)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override ValueTask<object?> GetNextAsync(object? sourceValue, CancellationToken token = default)
        {
            return ValueTask.FromResult<object?>(this.Definition.Value);
        }

        #endregion
    }
}
