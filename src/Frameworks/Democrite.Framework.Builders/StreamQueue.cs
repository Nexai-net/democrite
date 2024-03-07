// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Core.Abstractions.Streams;

    using System;

    /// <summary>
    /// Tools use to define data stream
    /// </summary>
    public static class StreamQueue
    {
        /// <summary>
        /// Start building stream definition
        /// </summary>
        /// <returns></returns>
        public static StreamQueueDefinition Create(string streamConfiguration, string streamKey, string? streamCustomKey, Guid? fixUid = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(streamConfiguration);
            ArgumentNullException.ThrowIfNullOrEmpty(streamCustomKey);

            return new StreamQueueDefinition(fixUid ?? Guid.NewGuid(),
                                             $"{streamConfiguration}/{streamKey}" + (string.IsNullOrEmpty(streamCustomKey) ? "" : "+" + streamCustomKey),
                                             streamConfiguration,
                                             streamKey,
                                             streamCustomKey,
                                             null);
        }

        /// <summary>
        /// Start building stream definition
        /// </summary>
        /// <returns></returns>
        public static StreamQueueDefinition Create(string streamConfiguration, string streamKey, Guid? streamCustomUid, Guid? fixUid = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(streamConfiguration);

            return new StreamQueueDefinition(fixUid ?? Guid.NewGuid(),
                                             $"{streamConfiguration}/{streamKey}" + (streamCustomUid is null ? "" : "+" + streamCustomUid),
                                             streamConfiguration,
                                             streamKey,
                                             null,
                                             streamCustomUid);   
        }

        /// <summary>
        /// Get a new <see cref="StreamQueueDefinition"/> on default streamConfiguration <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </summary>
        /// <remarks>
        ///     Record from default stream <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </remarks>
        public static StreamQueueDefinition CreateFromDefaultStream(string streamKey, string? streamCustomKey = null, Guid? fixUid = null)
        {
            return Create(StreamQueueDefinition.DEFAULT_STREAM_KEY, streamKey, streamCustomKey, fixUid);
        }

        /// <summary>
        /// Get a new <see cref="StreamQueueDefinition"/> on default streamConfiguration <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </summary>
        /// <remarks>
        ///     Record from default stream <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </remarks>
        public static StreamQueueDefinition CreateFromDefaultStream(string streamKey, Guid? streamCustomUid = null, Guid? fixUid = null)
        {
            return Create(StreamQueueDefinition.DEFAULT_STREAM_KEY, streamKey, streamCustomUid, fixUid);
        }
    }
}
