// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Toolbox.Helpers;

    using Orleans.Runtime;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Information about the signal source that result on the current one.
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public class SignalSource
    {
        #region Ctor

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
        public string? CarryMessageType { get; }

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
    public sealed class SignalSource<TData> : SignalSource
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
    }
}
