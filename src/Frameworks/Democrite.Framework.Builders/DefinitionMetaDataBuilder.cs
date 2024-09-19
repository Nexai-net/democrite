// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Core.Abstractions;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    public sealed class DefinitionMetaDataBuilder : IDefinitionMetaDataBuilder, IDefinitionMetaDataWithDisplayNameBuilder
    {
        #region Fields

        private readonly HashSet<string> _tags;
        private string? _categoryPath;
        private string? _description;
        private string? _displayName;
        private string? _namespaceIdntifier;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionMetaDataBuilder"/> class.
        /// </summary>
        public DefinitionMetaDataBuilder()
        {
            this._tags = new HashSet<string>();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IDefinitionMetaDataBuilder AddTags(params string[] tags)
        {
            foreach (var tag in tags ?? EnumerableHelper<string>.ReadOnlyArray)
                this._tags.Add(tag);

            return this;
        }

        /// <inheritdoc />
        public IDefinitionMetaDataBuilder CategoryChain(params string[] categoryPath)
        {
            if (categoryPath is not null && categoryPath.Any())
                this._categoryPath = string.Join("/", categoryPath.Where(c => string.IsNullOrEmpty(c) == false));
            return this;
        }

        /// <inheritdoc />
        public IDefinitionMetaDataBuilder CategoryPath(string categoryPath)
        {
            this._categoryPath = categoryPath;
            return this;
        }

        /// <inheritdoc />
        public IDefinitionMetaDataBuilder Description(string description)
        {
            this._description = description;
            return this;
        }

        /// <inheritdoc />
        public IDefinitionMetaDataBuilder DisplayName(string displayName)
        {
            this._displayName = displayName;
            return this;
        }

        /// <inheritdoc />
        public IDefinitionMetaDataBuilder Namespace(string? namespaceIdentifier)
        {
            this._namespaceIdntifier = namespaceIdentifier;
            return this;
        }

        /// <inheritdoc />
        public DefinitionMetaData Build()
        {
            return Build(out _, out _);
        }

        /// <inheritdoc />
        public DefinitionMetaData Build(out string? displayName, out string? namespaceIdentifier)
        {
            displayName = this._displayName;
            namespaceIdentifier = this._namespaceIdntifier;

            if (string.IsNullOrEmpty(this._description) &&
                string.IsNullOrEmpty(this._categoryPath) &&
                this._tags.Count == 0 &&
                string.IsNullOrEmpty(this._namespaceIdntifier))
            {
                return null!;
            }

            return new DefinitionMetaData(this._description,
                                          this._categoryPath,
                                          this._tags.ToArray(),
                                          DateTime.UtcNow,
                                          this._namespaceIdntifier);
        }

        /// <inheritdoc />
        public static DefinitionMetaData? Execute(Action<IDefinitionMetaDataBuilder>? metaDataBuilder)
        {
            if (metaDataBuilder is null)
                return null;

            var builder = new DefinitionMetaDataBuilder();
            metaDataBuilder(builder);
            return builder.Build();
        }

        /// <inheritdoc />
        IDefinitionMetaDataBuilder IDefinitionMetaDataWithDisplayNameBuilder.DisplayName(string displayName)
        {
            this.DisplayName(displayName);
            return this;
        }

        /// <inheritdoc />
        DefinitionMetaData IDefinitionMetaDataWithDisplayNameBuilder.Build(out string? displayName, out string? namespaceIdentifier)
        {
            return this.Build(out displayName, out namespaceIdentifier);
        }

        /// <inheritdoc />
        IDefinitionMetaDataWithDisplayNameBuilder IDefinitionMetaDataBaseBuilder<IDefinitionMetaDataWithDisplayNameBuilder>.CategoryPath(string categoryPath)
        {
            ((IDefinitionMetaDataBuilder)this).CategoryPath(categoryPath);
            return this;
        }

        /// <inheritdoc />
        IDefinitionMetaDataWithDisplayNameBuilder IDefinitionMetaDataBaseBuilder<IDefinitionMetaDataWithDisplayNameBuilder>.CategoryChain(params string[] categoryPath)
        {
            ((IDefinitionMetaDataBuilder)this).CategoryChain(categoryPath);
            return this;
        }

        /// <inheritdoc />
        IDefinitionMetaDataWithDisplayNameBuilder IDefinitionMetaDataBaseBuilder<IDefinitionMetaDataWithDisplayNameBuilder>.Description(string description)
        {
            ((IDefinitionMetaDataBuilder)this).Description(description);
            return this;
        }

        /// <inheritdoc />
        IDefinitionMetaDataWithDisplayNameBuilder IDefinitionMetaDataBaseBuilder<IDefinitionMetaDataWithDisplayNameBuilder>.AddTags(params string[] tags)
        {
            ((IDefinitionMetaDataBuilder)this).AddTags(tags);
            return this;
        }

        #endregion
    }
}
