// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Inputs
{
    using Elvex.Toolbox.Abstractions.Supports;

    using Microsoft.Extensions.Logging;

    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define a data source using a predefined value
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class DataSourceFixValueDefinition<TData> : DataSourceDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceFixValueDefinition{TData}"/> class.
        /// </summary>
        public DataSourceFixValueDefinition(Guid uid,
                                            TData? value, DefinitionMetaData? metaData = null)
            : base(uid, DataSourceTypeEnum.FixValue, typeof(TData).GetAbstractType(), metaData)
        {
            this.Value = value;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [DataMember]
        [Id(0)]
        public TData? Value { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(DataSourceDefinition other)
        {
            return other is DataSourceFixValueDefinition<TData> otherTyped &&
                   EqualityComparer<TData>.Default.Equals(this.Value, otherTyped.Value);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.Value?.GetHashCode() ?? 0;  
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"Fix Value {(this.Value as ISupportDebugDisplayName)?.ToDebugDisplayName() ?? this.Value?.ToString()}";
        }

        /// <inheritdoc />
        public override bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            return true;
        }

        #endregion
    }
}
