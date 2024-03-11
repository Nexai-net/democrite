// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Inputs
{
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    /// <summary>
    /// Definition of data source
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]

    [KnownType(typeof(DataSourceStaticCollectionDefinition<>))]
    [KnownType(typeof(DataSourceFixValueDefinition<>))]
    

    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public abstract class DataSourceDefinition : IEquatable<DataSourceDefinition>, IDefinition
    {
        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceDefinition"/> class.
        /// </summary>
        protected DataSourceDefinition(Guid uid,
                                       DataSourceTypeEnum InputSourceType,
                                       AbstractType dataType)
        {
            this.Uid = uid;
            this.InputSourceType = InputSourceType;
            this.DataType = dataType;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [IgnoreDataMember]
        public string DisplayName
        {
            get { return ToDebugDisplayName(); }
        }

        /// <inheritdoc />
        [DataMember]
        public Guid Uid { get; }

        /// <summary>
        /// Gets the type of the input source.
        /// </summary>
        [IgnoreDataMember]
        public DataSourceTypeEnum InputSourceType { get; }

        /// <summary>
        /// Gets the type of the input.
        /// </summary>
        [IgnoreDataMember]
        public AbstractType DataType { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public abstract bool Validate(ILogger logger, bool matchWarningAsError = false);

        /// <inheritdoc />
        public abstract string ToDebugDisplayName();

        /// <inheritdoc />
        public bool Equals(DataSourceDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            return this.InputSourceType == other.InputSourceType &&
                   this.DataType == other.DataType &&
                   OnEquals(other);
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is DataSourceDefinition other)
                return Equals(other);

            return false;
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return this.InputSourceType.GetHashCode() ^
                   this.DataType.GetHashCode() ^
                   OnGetHashCode();
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected abstract int OnGetHashCode();

        /// <inheritdoc cref="IEquatable{T}.Equals(T)" />
        /// <remarks>
        ///     Null check and reference check are already done
        /// </remarks>
        protected abstract bool OnEquals(DataSourceDefinition other);

        #endregion
    }
}
