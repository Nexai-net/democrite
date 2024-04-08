// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Deferred
{
    using Democrite.Framework.Core.Abstractions.Deferred;

    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Models;

    using System.ComponentModel;

    internal sealed class DeferredWork
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredWork"/> class.
        /// </summary>
        public DeferredWork(DeferredId deferredId,
                            DeferredStatusEnum status,
                            byte[]? response,
                            ConcretBaseType expectedReponseType,
                            DateTime lastUpdate)
        {
            this.DeferredId = deferredId;
            this.Status = status;
            this.Response = response;
            this.ExpectedReponseType = expectedReponseType;
            this.UTCLastUpdate = lastUpdate;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the deferredId.
        /// </summary>
        public DeferredId DeferredId { get; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public DeferredStatusEnum Status { get; private set; }

        /// <summary>
        /// Gets the response.
        /// </summary>
        public byte[]? Response { get; private set; }

        /// <summary>
        /// Gets the expected type of the response.
        /// </summary>
        public ConcretBaseType ExpectedReponseType { get; }

        /// <summary>
        /// Gets the last update.
        /// </summary>
        public DateTime UTCLastUpdate { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the response.
        /// </summary>
        public void SetResult(in ReadOnlyMemory<byte> reponse, ITimeManager timeManager, DeferredStatusEnum newStatus = DeferredStatusEnum.Finished)
        {
            this.Response = reponse.ToArray();
            ChangeStatus(newStatus, timeManager);
        }

        /// <summary>
        /// Changes the status.
        /// </summary>
        public bool ChangeStatus(DeferredStatusEnum status, ITimeManager timeManager)
        {
            if (this.Status >= status)
                return false;

            this.Status = status;
            this.UTCLastUpdate = timeManager.UtcNow;
            return true;
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    internal record struct DeferredWorkSurrogate(DeferredId Uid, DeferredStatusEnum Status, byte[]? Response, ConcretBaseType ExpectedReponseType, DateTime UTCLastUpdate);

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Orleans.IConverter&lt;Democrite.Framework.Node.Deferred.DeferredWork, Democrite.Framework.Node.Deferred.DeferredWorkSurrogate&gt;" />
    [RegisterConverter]
    internal sealed class DeferredWorkConverter : IConverter<DeferredWork, DeferredWorkSurrogate>
    {
        #region Ctor

        /// <summary>
        /// .cctors this instance.
        /// </summary>
        static DeferredWorkConverter()
        {
            Default = new DeferredWorkConverter();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static DeferredWorkConverter Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public DeferredWork ConvertFromSurrogate(in DeferredWorkSurrogate surrogate)
        {
            return new DeferredWork(surrogate.Uid,
                                    surrogate.Status,
                                    surrogate.Response,
                                    surrogate.ExpectedReponseType,
                                    surrogate.UTCLastUpdate);
        }

        /// <inheritdoc />
        public DeferredWorkSurrogate ConvertToSurrogate(in DeferredWork value)
        {
            return new DeferredWorkSurrogate(value.DeferredId,
                                             value.Status,
                                             value.Response,
                                             value.ExpectedReponseType,
                                             value.UTCLastUpdate);
        }

        #endregion
    }
}
