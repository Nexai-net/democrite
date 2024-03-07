// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Models.Surrogates
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    using System.Collections.Generic;

    [GenerateSerializer]
    public record struct BlackboardRecordRegistryStateSurrogate(IReadOnlyCollection<BlackboardRecordMetadata> RecordMetadatas);

    [RegisterConverter]
    public sealed class BlackboardRecordRegistryStateConverter : IConverter<BlackboardRecordRegistryState, BlackboardRecordRegistryStateSurrogate>
    {
        #region Ctor

        /// <summary>
        /// .cctors this instance.
        /// </summary>
        /// <returns></returns>
        static BlackboardRecordRegistryStateConverter()
        {
            Default = new BlackboardRecordRegistryStateConverter();
        }

        #endregion

        #region Properties

        public static BlackboardRecordRegistryStateConverter Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public BlackboardRecordRegistryState ConvertFromSurrogate(in BlackboardRecordRegistryStateSurrogate surrogate)
        {
            return new BlackboardRecordRegistryState(surrogate.RecordMetadatas);
        }

        /// <inheritdoc />
        public BlackboardRecordRegistryStateSurrogate ConvertToSurrogate(in BlackboardRecordRegistryState value)
        {
            return value.ToSurrogate();
        }

        #endregion
    }
}
