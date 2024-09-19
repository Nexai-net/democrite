// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Minimal meta data on a definition
    /// </summary>
    /// <remarks>
    ///     Those information are used in wysiwyg interface but also be meta-ia to understand the goal of the definition and associate way to wish
    /// </remarks>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public class DefinitionMetaData : IEquatable<DefinitionMetaData>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionMetaData"/> class.
        /// </summary>
        public DefinitionMetaData(string? description,
                                  string? categoryPath,
                                  IEnumerable<string>? tags,
                                  DateTime utcUpdateTime,
                                  string? namespaceIdentifier = null)
        {
            this.Description = description;
            this.CategoryPath = categoryPath;
            this.Tags = tags?.ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;
            this.UTCUpdateTime = utcUpdateTime;
            this.NamespaceIdentifier = namespaceIdentifier;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Gets the category path.
        /// </summary>
        /// <remarks>
        ///     use "/" as stage separator
        /// </remarks>
        public string? CategoryPath { get; }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        public IReadOnlyCollection<string> Tags { get; }

        /// <summary>
        /// Gets the UTC update time.
        /// </summary>
        public DateTime UTCUpdateTime { get; }

        /// <summary>
        /// Gets the namespace identifier.
        /// </summary>
        public string? NamespaceIdentifier { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        public bool Equals(DefinitionMetaData? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.Description == other.Description &&
                   this.CategoryPath == other.CategoryPath &&
                   this.Tags.SequenceEqual(other.Tags) &&
                   this.NamespaceIdentifier == other.NamespaceIdentifier &&
                   OnEquals(other);
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T?)" />
        protected virtual bool OnEquals(DefinitionMetaData other)
        {
            return true;
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.Description,
                                    this.CategoryPath,
                                    this.Tags.Aggregate(0, (acc, t) => acc ^ (t?.GetHashCode() ?? 0)),
                                    this.NamespaceIdentifier,
                                    OnGetHashCode());
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected virtual int OnGetHashCode()
        {
            return 0;
        }

        /// <inheritdoc />
        public static bool operator==(DefinitionMetaData? lhs, DefinitionMetaData? rhs)
        {
            return lhs?.Equals(rhs) ?? lhs is null;
        }

        /// <inheritdoc />
        public static bool operator !=(DefinitionMetaData? lhs, DefinitionMetaData? rhs)
        {
            return !(lhs == rhs);
        }

        #endregion
    }
}
