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
        public static StreamQueueDefinition Create(string streamConfiguration, string streamNamespace, string streamKey, Guid? fixUid = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(streamConfiguration);
            ArgumentNullException.ThrowIfNullOrEmpty(streamKey);

            return new StreamQueueDefinition(fixUid ?? Guid.NewGuid(),
                                             $"{streamConfiguration}/{streamNamespace}" + "+" + streamKey,
                                             streamConfiguration,
                                             streamNamespace,
                                             streamKey,
                                             null);
        }

        /// <summary>
        /// Start building stream definition
        /// </summary>
        /// <returns></returns>
        public static StreamQueueDefinition Create(string streamConfiguration, string streamNamespace, Guid streamKey, Guid? fixUid = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(streamConfiguration);

            return new StreamQueueDefinition(fixUid ?? Guid.NewGuid(),
                                             $"{streamConfiguration}/{streamNamespace}" + "+" + streamKey,
                                             streamConfiguration,
                                             streamNamespace,
                                             null,
                                             streamKey);
        }

        /// <summary>
        /// Get a new <see cref="StreamQueueDefinition"/> on default streamConfiguration <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </summary>
        /// <remarks>
        ///     Record from default stream <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </remarks>
        public static StreamQueueDefinition CreateFromDefaultStream(string streamKey, Guid? fixUid = null)
        {
            return Create(StreamQueueDefinition.DEFAULT_STREAM_KEY, "global", streamKey, fixUid);
        }

        /// <summary>
        /// Get a new <see cref="StreamQueueDefinition"/> on default streamConfiguration <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </summary>
        /// <remarks>
        ///     Record from default stream <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </remarks>
        public static StreamQueueDefinition CreateFromDefaultStream(string streamNamespace, string streamKey, Guid? fixUid = null)
        {
            return Create(StreamQueueDefinition.DEFAULT_STREAM_KEY, streamNamespace, streamKey, fixUid);
        }

        /// <summary>
        /// Get a new <see cref="StreamQueueDefinition"/> on default streamConfiguration <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </summary>
        /// <remarks>
        ///     Record from default stream <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </remarks>
        public static StreamQueueDefinition CreateFromDefaultStream(Guid streamKey, Guid? fixUid = null)
        {
            return Create(StreamQueueDefinition.DEFAULT_STREAM_KEY, "global", streamKey, fixUid);
        }

        /// <summary>
        /// Get a new <see cref="StreamQueueDefinition"/> on default streamConfiguration <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </summary>
        /// <remarks>
        ///     Record from default stream <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </remarks>
        public static StreamQueueDefinition CreateFromDefaultStream(string streamNamespace, Guid streamKey, Guid? fixUid = null)
        {
            return Create(StreamQueueDefinition.DEFAULT_STREAM_KEY, streamNamespace, streamKey, fixUid);
        }
    }
}
