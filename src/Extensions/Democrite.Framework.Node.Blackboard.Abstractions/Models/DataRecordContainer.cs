// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions.Repositories;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Abstractions.Supports;
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
    public abstract class DataRecordContainer : IEquatable<DataRecordContainer>, ISupportDebugDisplayName, IEntityWithId<Guid>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRecordContainer"/> class.
        /// </summary>
        public DataRecordContainer(string logicalType,
                                   Guid uid,
                                   string displayName,
                                   RecordContainerTypeEnum recordContainerType,
                                   RecordStatusEnum status,
                                   DateTime utcCreationTime,
                                   string? creatorIdentity,
                                   DateTime utcLastUpdateTime,
                                   string? lastUpdaterIdentity,
                                   RecordMetadata? customMetadata)
        {
            this.Uid = uid;
            this.LogicalType = logicalType;
            this.DisplayName = displayName;
            this.RecordContainerType = recordContainerType;
            this.Status = status;
            this.UTCCreationTime = utcCreationTime;
            this.CreatorIdentity = creatorIdentity;
            this.UTCLastUpdateTime = utcLastUpdateTime;
            this.LastUpdaterIdentity = lastUpdaterIdentity;
            this.CustomMetadata = customMetadata;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the record unique identifier; If value is <see cref="Guid.Empty"/> during push it will be interpreted as push new
        /// </summary>
        [DataMember]
        public Guid Uid { get; private set; }

        /// <summary>
        /// Gets a logic type label used to categorise the record
        /// </summary>
        [DataMember]
        public string LogicalType { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        [DataMember]
        public string DisplayName { get; }

        /// <summary>
        /// Gets the type of the record container.
        /// </summary>
        [DataMember]
        public RecordContainerTypeEnum RecordContainerType { get; }

        /// <summary>
        /// Gets the type of the contains.
        /// </summary>
        [DataMember]
        public abstract ConcretBaseType? ContainsType { get; }

        /// <summary>
        /// Gets the data status.
        /// </summary>
        [DataMember]
        public RecordStatusEnum Status { get; }

        /// <summary>
        /// Gets the UTC creation time.
        /// </summary>
        [DataMember]
        public DateTime UTCCreationTime { get; }

        /// <summary>
        /// Gets the create by.
        /// </summary>
        [DataMember]
        public string? CreatorIdentity { get; }

        /// <summary>
        /// Gets the UTC last update time.
        /// </summary>
        [DataMember]
        public DateTime UTCLastUpdateTime { get; }

        /// <summary>
        /// Gets the last updater identity.
        /// </summary>
        [DataMember]
        public string? LastUpdaterIdentity { get; }

        /// <summary>
        /// Gets the custom metadata.
        /// </summary>
        [DataMember]
        public RecordMetadata? CustomMetadata { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual string ToDebugDisplayName()
        {
            return this.DisplayName;
        }

        /// <summary>
        /// Gets data carried in <see cref="TData"/> format
        /// </summary>
        public abstract TData GetData<TData>();

        /// <summary>
        /// Forces the uid.
        /// </summary>
        /// <remarks>
        ///     Mainly used on new iten added
        /// </remarks>
        internal void ForceUid(Guid newUid)
        {
            this.Uid = newUid;
        }

        /// <inheritdoc />
        public bool Equals(DataRecordContainer? other)
        {
            if (other is null)
                return false;

            return other.Uid == this.Uid &&
                   string.Equals(other.LogicalType, this.LogicalType) &&
                   string.Equals(other.DisplayName, this.DisplayName, StringComparison.OrdinalIgnoreCase) &&
                   (this.CustomMetadata?.Equals(other.CustomMetadata) ?? other.CustomMetadata is null) &&
                   OnEquals(other!);
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is DataRecordContainer otherRecordContainer)
                return Equals(otherRecordContainer);
            return false;
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.Uid,
                                    this.LogicalType,
                                    this.DisplayName,
                                    this.CustomMetadata,
                                    OnGetHashCode());
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected abstract object OnGetHashCode();

        /// <inheritdoc cref="IEquatable{T}.Equals(T?)" />
        protected abstract bool OnEquals(DataRecordContainer dataRecordContainer);

        /// <summary>
        /// Duplicate with a new status
        /// </summary>
        public abstract DataRecordContainer WithNewStatus(RecordStatusEnum newRecordStatus, DateTime utcNow);

        /// <summary>
        /// Try create data projection
        /// </summary>
        public abstract bool TryProjectTo<TDataProjection>(IObjectConverter converter, out DataRecordContainer<TDataProjection>? projectContainer);

        #endregion
    }

    /// <summary>
    /// Container about one data record through a blackboard
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <seealso cref="IEquatable{DataRecordContainer}" />
    /// <seealso cref="ISupportDebugDisplayName" />
    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class DataRecordContainer<TData> : DataRecordContainer
    {
        #region Fields

        private static readonly Type s_dataTrait;
        private static readonly ConcretBaseType s_dataTraitAbstract;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DataRecordContainer{TData}"/> class.
        /// </summary>
        static DataRecordContainer()
        {
            s_dataTrait = typeof(TData);
            s_dataTraitAbstract = (ConcretBaseType)s_dataTrait.GetAbstractType();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRecordContainer{TData}"/> class.
        /// </summary>
        public DataRecordContainer(string logicalType,
                                   Guid uid,
                                   string displayName,
                                   TData? data,
                                   RecordStatusEnum status,
                                   DateTime utcCreationTime,
                                   string? creatorIdentity,
                                   DateTime utcLastUpdateTime,
                                   string? lastUpdaterIdentity,
                                   RecordMetadata? customMetadata)
            : base(logicalType,
                   uid,
                   displayName,
                   RecordContainerTypeEnum.Direct,
                   status,
                   utcCreationTime,
                   creatorIdentity,
                   utcLastUpdateTime,
                   lastUpdaterIdentity,
                   customMetadata)
        {
            this.Data = data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the carry data.
        /// </summary>
        [DataMember]
        public TData? Data { get; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public override ConcretBaseType? ContainsType
        {
            get { return s_dataTraitAbstract; }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override TDataProjection GetData<TDataProjection>()
        {
            if (this.Data is TDataProjection projection)
                return projection;

            throw new NotSupportedException("Convertion is not yet supported");
        }

        /// <inheritdoc />
        public override DataRecordContainer WithNewStatus(RecordStatusEnum newRecordStatus, DateTime utcNow)
        {
            return new DataRecordContainer<TData>(this.LogicalType,
                                                  this.Uid,
                                                  this.DisplayName,
                                                  this.Data,
                                                  newRecordStatus,
                                                  this.UTCCreationTime,
                                                  this.CreatorIdentity,
                                                  utcNow,
                                                  null,
                                                  this.CustomMetadata);
        }

        /// <inheritdoc />
        public override bool TryProjectTo<TDataProjection>(IObjectConverter converter, out DataRecordContainer<TDataProjection>? projectContainer)
        {
            projectContainer = default;

            if (this is DataRecordContainer<TDataProjection> alreadyGoodType)
            {
                projectContainer = alreadyGoodType;
                return true;
            }

            var projectionSucceed = false;
            TDataProjection? projectResult = default;

            if (NoneType.IsEqualTo<TData>() || this.Data is null)
            {
                projectionSucceed = true;
            }
            else if (converter.TryConvert<TDataProjection>(this.Data, out var projData))
            {
                projectResult = projData;
                projectionSucceed = true;
            }

            if (projectionSucceed)
            {
                projectContainer = new DataRecordContainer<TDataProjection>(this.LogicalType,
                                                                            this.Uid,
                                                                            this.DisplayName,
                                                                            projectResult,
                                                                            this.Status,
                                                                            this.UTCCreationTime,
                                                                            this.CreatorIdentity,
                                                                            this.UTCLastUpdateTime,
                                                                            this.LastUpdaterIdentity, 
                                                                            this.CustomMetadata);
            }

            return projectionSucceed;
        }

        /// <inheritdoc/>
        protected override bool OnEquals(DataRecordContainer dataRecordContainer)
        {
            return EqualityComparer<ConcretBaseType>.Default.Equals(this.ContainsType, dataRecordContainer.ContainsType) &&
                   EqualityComparer<TData>.Default.Equals(this.Data, dataRecordContainer.GetData<TData>());
        }

        /// <inheritdoc/>
        protected override object OnGetHashCode()
        {
            return this.Data?.GetHashCode() ?? 0;
        }

        #endregion

    }
}
