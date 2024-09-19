// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Core.Abstractions;

    /// <summary>
    /// Builder used to help create the <see cref="DefinitionMetaData"/> information
    /// </summary>
    public interface IDefinitionMetaDataBaseBuilder<TWizard>
        where TWizard : IDefinitionMetaDataBaseBuilder<TWizard>
    {
        /// <inheritdoc cref="IRefDefinition.RefId" />
        IDefinitionMetaDataBuilder Namespace(string? namespaceIdentifier);

        /// <inheritdoc cref="DefinitionMetaData.CategoryPath" />
        TWizard CategoryPath(string categoryPath);

        /// <inheritdoc cref="DefinitionMetaData.CategoryPath" />
        /// <remarks>
        ///     Contact all parts using "/" as separator
        /// </remarks>
        TWizard CategoryChain(params string[] categoryPath);

        /// <inheritdoc cref="DefinitionMetaData.Description" />
        TWizard Description(string description);

        /// <summary>
        /// Append tags
        /// </summary>
        TWizard AddTags(params string[] tags);
    }

    /// <summary>
    /// Builder used to help create the <see cref="DefinitionMetaData"/> information
    /// </summary>
    public interface IDefinitionMetaDataBuilder : IDefinitionMetaDataBaseBuilder<IDefinitionMetaDataBuilder>
    {
        /// <summary>
        /// Builds <see cref="DefinitionMetaData"/> >
        /// </summary>
        DefinitionMetaData Build();
    }

    /// <summary>
    /// Builder used to help create the <see cref="DefinitionMetaData"/> information
    /// </summary>
    public interface IDefinitionMetaDataWithDisplayNameBuilder : IDefinitionMetaDataBaseBuilder<IDefinitionMetaDataWithDisplayNameBuilder>
    {
        /// <inheritdoc cref="IDefinition.DisplayName" />
        IDefinitionMetaDataBuilder DisplayName(string displayName);

        /// <summary>
        /// Builds <see cref="DefinitionMetaData"/> >
        /// </summary>
        DefinitionMetaData Build(out string? displayName, out string? namespaceIdentifier);
    }
}
