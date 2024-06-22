// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Inputs
{
    using Elvex.Toolbox.Abstractions.Expressions;

    using Microsoft.Extensions.Logging;

    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define a data source used to input data and transform it
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class DataSourceConvertValueDefinition<TData> : DataSourceDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceConvertValueDefinition{TData}"/> class.
        /// </summary>
        public DataSourceConvertValueDefinition(Guid uid, AccessExpressionDefinition access, DefinitionMetaData? metaData = null)
            : base(uid, DataSourceTypeEnum.Convert, typeof(TData).GetAbstractType(), metaData)
        {
            ArgumentNullException.ThrowIfNull(access);
            this.Access = access;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [DataMember]
        [Id(0)]
        public AccessExpressionDefinition Access { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(DataSourceDefinition other)
        {
            return other is DataSourceFixValueDefinition<TData> otherTyped &&
                   this.Access.Equals(otherTyped.Value);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.Access?.GetHashCode() ?? 0;  
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"Convert Value {this.Access.ToDebugDisplayName()}";
        }

        /// <inheritdoc />
        public override bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            return true;
        }

        #endregion
    }
}
