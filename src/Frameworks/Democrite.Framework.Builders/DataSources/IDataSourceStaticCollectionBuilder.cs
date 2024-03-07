// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.DataSources
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Inputs;

    /// <summary>
    /// Get in charge to customize an trigger input using a static collection of value provide are the construction
    /// </summary>
    /// <typeparam name="TData">The type of the trigger output.</typeparam>
    /// <seealso cref="ITriggerOutputBuilder{TTriggerOutput}" />
    public interface IDataSourceStaticCollectionBuilder<TData> : IDefinitionBaseBuilder<DataSourceDefinition>
    {
        /// <summary>
        /// Define how to pick a value from the collection on each call
        /// </summary>
        /// <remarks>
        ///     <c>Default</c> : <see cref="PullModeEnum.Circling"/> <br />
        ///     <c><see cref="PullModeEnum.Broadcast"/></c> all values are executed in parallel<br />
        /// </remarks>
        IDataSourceStaticCollectionBuilder<TData> PullMode(PullModeEnum mode);
    }
}
