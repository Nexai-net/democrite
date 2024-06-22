// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Inputs
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using Microsoft.Extensions.Logging;

    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define a data source using a predefined static collection.
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class DataSourceStaticCollectionDefinition<TData> : DataSourceDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceStaticCollectionDefinition{TInputType}"/> class.
        /// </summary>
        public DataSourceStaticCollectionDefinition(Guid uid,
                                                    IEnumerable<TData?> collection,
                                                    PullModeEnum pullMode,
                                                    DefinitionMetaData? definitionMeta = null)
            : base(uid, DataSourceTypeEnum.StaticCollection, typeof(TData).GetAbstractType(), definitionMeta)
        {
            this.Collection = collection.ToReadOnlyList();
            this.PullMode = pullMode;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [DataMember]
        [Id(0)]
        public IReadOnlyList<TData?> Collection { get; }

        /// <inheritdoc />
        [DataMember]
        [Id(1)]
        public PullModeEnum PullMode { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(DataSourceDefinition other)
        {
            return other is DataSourceStaticCollectionDefinition<TData> otherTyped &&
                   this.PullMode == otherTyped.PullMode &&
                   this.Collection.SequenceEqual(otherTyped.Collection);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.PullMode.GetHashCode() ^
                   (this.Collection?.Aggregate(0, (acc, i) => acc ^ (i?.GetHashCode() ?? 0)) ?? 0);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"Static Collection {typeof(TData).Name} ({this.Collection.Count}) (Mode: {this.PullMode})";
        }

        /// <inheritdoc />
        public override bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            return true;
        }

        #endregion
    }
}
