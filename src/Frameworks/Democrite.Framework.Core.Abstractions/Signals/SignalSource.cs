// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Democrite.Framework.Core.Abstractions.Models;
    using Elvex.Toolbox.Helpers;

    using Orleans.Runtime;

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;

    /// <summary>
    /// Information about the signal source that result on the current one.
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public class SignalSource : IEquatable<SignalSource>
    {
        #region Fields

        private static readonly Dictionary<Type, MethodInfo> s_genericSignalSourceCache;

        private static readonly ReaderWriterLockSlim s_genericSignalSourceCacheLocker;
        private static readonly MethodInfo s_genericSignalSource;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="SignalSource"/> class.
        /// </summary>
        static SignalSource()
        {
            var genericSignalSource = typeof(SignalSource).GetMethod(nameof(CreateSignalContentFromType), BindingFlags.Static | BindingFlags.NonPublic);
            Debug.Assert(genericSignalSource != null);

            s_genericSignalSource = genericSignalSource;

            s_genericSignalSourceCache = new Dictionary<Type, MethodInfo>();
            s_genericSignalSourceCacheLocker = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalSource"/> class.
        /// </summary>
        protected SignalSource(Guid signalUid,
                               Guid sourceDefinitionId,
                               string sourceDefinitionName,
                               bool isDoor,
                               DateTime sendUtcTime,
                               GrainId? vgrainSourceId,
                               VGrainMetaData? vgrainMetaData,
                               string? carryMessageType,
                               IEnumerable<SignalSource>? origins = null)
            : this(signalUid, sourceDefinitionId, sourceDefinitionName, isDoor, sendUtcTime, vgrainSourceId, vgrainMetaData, origins)
        {
            this.CarryMessageType = carryMessageType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalSource"/> class.
        /// </summary>
        public SignalSource(Guid signalUid,
                            Guid sourceDefinitionId,
                            string sourceDefinitionName,
                            bool isDoor,
                            DateTime sendUtcTime,
                            GrainId? vgrainSourceId,
                            VGrainMetaData? vgrainMetaData,
                            IEnumerable<SignalSource>? origins = null)
        {
            this.SignalUid = signalUid;
            this.SourceDefinitionId = sourceDefinitionId;
            this.SourceDefinitionName = sourceDefinitionName;
            this.IsDoor = isDoor;
            this.VGrainSourceId = vgrainSourceId;
            this.VGrainMetaData = vgrainMetaData;
            this.SendUtcTime = sendUtcTime;

            this.Origins = origins?.ToArray() ?? EnumerableHelper<SignalSource>.ReadOnlyArray;
        }

        #endregion

        #region Properties        

        /// <summary>
        /// Gets the signal unique identifier.
        /// </summary>
        [Id(0)]
        public Guid SignalUid { get; }

        /// <summary>
        /// Gets the source identifier.
        /// </summary>
        [Id(1)]
        public Guid SourceDefinitionId { get; }

        /// <summary>
        /// Gets the name of the source.
        /// </summary>
        [Id(2)]
        public string SourceDefinitionName { get; }

        /// <summary>
        /// Gets a value indicating whether source is door.
        /// </summary>
        [Id(3)]
        public bool IsDoor { get; }

        /// <summary>
        /// Gets the vgrain source identifier.
        /// </summary>
        [Id(4)]
        public GrainId? VGrainSourceId { get; }

        /// <summary>
        /// Gets the vgrain meta data.
        /// </summary>
        [Id(5)]
        public VGrainMetaData? VGrainMetaData { get; }

        /// <summary>
        /// Gets the froms.
        /// </summary>
        [Id(6)]
        public IReadOnlyCollection<SignalSource> Origins { get; }

        /// <summary>
        /// Gets the send UTC time.
        /// </summary>
        [Id(7)]
        public DateTime SendUtcTime { get; }

        /// <summary>
        /// Gets the type of the carry message if to use to correcly cast to <see cref="SignalSource{TData}"/>
        /// </summary>
        [Id(8)]
        public string? CarryMessageType { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the content.
        /// </summary>
        public virtual object? GetContent()
        {
            return null;
        }

        /// <summary>
        /// Creates a <see cref="SignalSource{TData}"/> using <paramref name="contentType"/>
        /// </summary>
        public static SignalSource Create(Guid signalId,
                                          Guid sourceDefinitionId,
                                          string sourceDefinitionName,
                                          bool isDoor,
                                          DateTime sendUtcTime,
                                          GrainId? vgrainSourceId,
                                          VGrainMetaData? vgrainMetaData,
                                          Type? contentType = null,
                                          object? content = null,
                                          IEnumerable<SignalSource>? origins = null)
        {
            if (contentType == null)
            {
                return new SignalSource(signalId,
                                        sourceDefinitionId,
                                        sourceDefinitionName,
                                        isDoor,
                                        sendUtcTime,
                                        vgrainSourceId,
                                        vgrainMetaData,
                                        origins);
            }

            MethodInfo? builder = null;

            s_genericSignalSourceCacheLocker.EnterReadLock();
            try
            {
                if (s_genericSignalSourceCache.TryGetValue(contentType, out var cachedbuilder))
                    builder = cachedbuilder;
            }
            finally
            {
                s_genericSignalSourceCacheLocker.ExitReadLock();
            }

            if (builder == null)
            {
                s_genericSignalSourceCacheLocker.EnterWriteLock();
                try
                {
                    if (s_genericSignalSourceCache.TryGetValue(contentType, out var cachedbuilder))
                    {
                        builder = cachedbuilder;
                    }
                    else
                    {
                        builder = s_genericSignalSource.MakeGenericMethod(contentType);
                        s_genericSignalSourceCache.Add(contentType, builder);
                    }
                }
                finally
                {
                    s_genericSignalSourceCacheLocker.ExitWriteLock();
                }
            }

            return (SignalSource)builder.Invoke(null, new object?[]
            {
                signalId,
                sourceDefinitionId,
                sourceDefinitionName,
                isDoor,
                sendUtcTime,
                vgrainSourceId,
                vgrainMetaData,
                content,
                origins
            })!;
        }

        /// <inheritdoc />
        public bool Equals(SignalSource? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.SignalUid == other.SignalUid &&
                   this.SourceDefinitionId == other.SourceDefinitionId &&
                   this.SourceDefinitionName == other.SourceDefinitionName &&
                   this.VGrainSourceId == other.VGrainSourceId &&
                   this.IsDoor == other.IsDoor &&
                   this.VGrainMetaData.Equals(other.VGrainMetaData) &&
                   this.SendUtcTime == other.SendUtcTime &&
                   this.CarryMessageType == other.CarryMessageType &&
                   this.Origins.SequenceEqual(other.Origins) &&
                   OnEquals(other);
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is SignalSource other)
                return Equals(other);

            return false;
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(HashCode.Combine(this.SourceDefinitionName, this.SourceDefinitionId, this.VGrainSourceId),
                                    HashCode.Combine(this.SignalUid, this.IsDoor, this.VGrainMetaData, this.SendUtcTime, this.CarryMessageType),
                                    this.Origins.OrderBy(o => o.SignalUid).Aggregate(0, (acc, source) => acc ^ source.GetHashCode()),
                                    OnGetHashCode());
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected virtual int OnGetHashCode()
        {
            return 0;
        }

        /// <inheritdoc cref="object.GetHashCode" />
        /// <inheritdoc cref="IEquatable{SignalSource}.Equals(SignalSource?)" />
        /// <remarks>
        /// null chack and reference have already been checked
        /// </remarks>
        protected virtual bool OnEquals(SignalSource other)
        {
            return true;
        }

        /// <summary>
        /// Creates the type of the signal content from.
        /// </summary>
        private static SignalSource<TContent> CreateSignalContentFromType<TContent>(Guid signalUid,
                                                                                    Guid sourceDefinitionId,
                                                                                    string sourceDefinitionName,
                                                                                    bool isDoor,
                                                                                    DateTime sendUtcTime,
                                                                                    GrainId? vgrainSourceId,
                                                                                    VGrainMetaData? vgrainMetaData,
                                                                                    TContent data,
                                                                                    IEnumerable<SignalSource>? origins = null)
            where TContent : struct
        {
            return new SignalSource<TContent>(signalUid,
                                              sourceDefinitionId,
                                              sourceDefinitionName,
                                              isDoor,
                                              sendUtcTime,
                                              vgrainSourceId,
                                              vgrainMetaData,
                                              data,
                                              origins);
        }

        #endregion
    }

    /// <summary>
    /// Information about the signal source that result on the current one with <typeparamref name="TData"/> carried.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <remarks>
    ///     To prevent any memory issue do not used signal to carry heavy information, 
    ///     prefer a simple id to target the information from a knowledge bank
    /// </remarks>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class SignalSource<TData> : SignalSource, IEquatable<SignalSource<TData>>
        where TData : struct
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalSource{TData}"/> class.
        /// </summary>
        public SignalSource(Guid signalUid,
                            Guid sourceDefinitionId,
                            string sourceDefinitionName,
                            bool isDoor,
                            DateTime sendUtcTime,
                            GrainId? vgrainSourceId,
                            VGrainMetaData? vgrainMetaData,
                            TData data,
                            IEnumerable<SignalSource>? origins = null)
            : base(signalUid,
                   sourceDefinitionId,
                   sourceDefinitionName,
                   isDoor,
                   sendUtcTime,
                   vgrainSourceId,
                   vgrainMetaData,
                   typeof(TData).Name,
                   origins)
        {
            this.Data = data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the data carry by the signal.
        /// </summary>
        [Id(0)]
        public TData Data { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the content.
        /// </summary>
        public override object? GetContent()
        {
            return this.Data;
        }

        /// <inheritdoc />
        public bool Equals(SignalSource<TData>? other)
        {
            return base.Equals((SignalSource?)other);
        }

        /// <inheritdoc />
        protected override bool OnEquals(SignalSource other)
        {
            return other is SignalSource<TData> otherData &&
                   this.Data.Equals(otherData);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.Data.GetHashCode();
        }

        #endregion
    }
}
