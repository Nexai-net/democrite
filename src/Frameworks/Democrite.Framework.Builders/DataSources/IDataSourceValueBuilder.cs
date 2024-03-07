// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.DataSources
{
    using Democrite.Framework.Core.Abstractions.Inputs;

    /// <summary>
    /// Data source use a simple value for exepression, fix, convertion ...
    /// </summary>
    /// <typeparam name="TOutputData">The type of the data source output.</typeparam>
    /// <seealso cref="IDefinitionBaseBuilder{DataSourceDefinition}" />
    public interface IDataSourceValueBuilder<TOutputData> : IDefinitionBaseBuilder<DataSourceDefinition>
    {
    }
}
