// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Models;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Container about one data record through a blackboard
    /// </summary>
    /// <seealso cref="IEquatable{DataRecordContainer}" />
    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class MetaDataRecordContainer : DataRecordContainer
    {
        #region Fields

        private readonly ConcretBaseType? _containsType;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRecordContainer{TData}"/> class.
        /// </summary>
        public MetaDataRecordContainer(string logicalType,
                                       Guid uid,
                                       string displayName,
                                       ConcretBaseType? containsType,
                                       RecordStatusEnum status,
                                       DateTime utcCreationTime,
                                       string? creatorIdentity,
                                       DateTime utcLastUpdateTime,
                                       string? lastUpdaterIdentity,
                                       RecordMetadata? customMetadata)

            : base(logicalType,
                   uid,
                   displayName,
                   RecordContainerTypeEnum.MetaData,
                   status,
                   utcCreationTime,
                   creatorIdentity,
                   utcLastUpdateTime,
                   lastUpdaterIdentity,
                   customMetadata)
        {
            this._containsType = containsType;
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        [DataMember]
        public override ConcretBaseType? ContainsType
        {
            get { return this._containsType; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Container type metadata doesn't support direct access to data
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        public override TData GetData<TData>()
        {
            throw new NotSupportedException("Container type metadata doesn't support direct access to data");
        }

        /// <inheritdoc />
        public override bool TryProjectTo<TDataProjection>(IObjectConverter converter, out DataRecordContainer<TDataProjection>? projectContainer)
        {
            throw new NotSupportedException("A meta data only container can't be convert to typed project one");
        }

        /// <inheritdoc />
        public override DataRecordContainer WithNewStatus(RecordStatusEnum newRecordStatus, DateTime utcNow)
        {
            return new MetaDataRecordContainer(this.LogicalType,
                                               this.Uid,
                                               this.DisplayName,
                                               this.ContainsType,
                                               newRecordStatus,
                                               this.UTCCreationTime,
                                               this.CreatorIdentity,
                                               this.UTCLastUpdateTime,
                                               this.LastUpdaterIdentity,
                                               this.CustomMetadata);
        }

        /// <inheritdoc />
        protected override bool OnEquals(DataRecordContainer dataRecordContainer)
        {
            return EqualityComparer<ConcretBaseType>.Default.Equals(dataRecordContainer.ContainsType, this._containsType);
        }

        /// <inheritdoc />
        protected override object OnGetHashCode()
        {
            return this.ContainsType?.GetHashCode() ?? 0;
        }

        #endregion

    }
}
