// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Streams;
    using Microsoft.CodeAnalysis;

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
        public static StreamQueueDefinition Create(string simpleNameIdentifier,
                                                   string streamConfiguration,
                                                   string streamNamespace,
                                                   string streamKey,
                                                   Guid? fixUid = null,
                                                   Action<IDefinitionMetaDataBuilder>? metaDataBuilder = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(streamConfiguration);
            ArgumentNullException.ThrowIfNullOrEmpty(streamKey);

            var metaData = DefinitionMetaDataBuilder.Execute(metaDataBuilder);

            return new StreamQueueDefinition(fixUid ?? Guid.NewGuid(),
                                             RefIdHelper.Generate(RefTypeEnum.StreamQueue, simpleNameIdentifier, metaData?.NamespaceIdentifier),
                                             $"{streamConfiguration}/{streamNamespace}" + "+" + streamKey,
                                             streamConfiguration,
                                             streamNamespace,
                                             streamKey,
                                             null,
                                             metaData);
        }

        /// <summary>
        /// Start building stream definition
        /// </summary>
        /// <returns></returns>
        public static StreamQueueDefinition Create(string simpleNameIdentifier,
                                                   string streamConfiguration,
                                                   string streamNamespace,
                                                   Guid streamKey,
                                                   Guid? fixUid = null,
                                                   Action<IDefinitionMetaDataBuilder>? metaDataBuilder = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(streamConfiguration);

            var metaData = DefinitionMetaDataBuilder.Execute(metaDataBuilder);
            
            return new StreamQueueDefinition(fixUid ?? Guid.NewGuid(),
                                             RefIdHelper.Generate(RefTypeEnum.StreamQueue, simpleNameIdentifier, metaData?.NamespaceIdentifier),
                                             $"{streamConfiguration}/{streamNamespace}" + "+" + streamKey,
                                             streamConfiguration,
                                             streamNamespace,
                                             null,
                                             streamKey,
                                             metaData);
        }

        /// <summary>
        /// Get a new <see cref="StreamQueueDefinition"/> on default streamConfiguration <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </summary>
        /// <remarks>
        ///     Record from default stream <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </remarks>
        public static StreamQueueDefinition CreateFromDefaultStream(string simpleNameIdentifier,
                                                                    string streamKey,
                                                                    Guid? fixUid = null,
                                                                    Action<IDefinitionMetaDataBuilder>? metaDataBuilder = null)
        {
            return Create(simpleNameIdentifier, StreamQueueDefinition.DEFAULT_STREAM_KEY, "global", streamKey, fixUid, metaDataBuilder);
        }

        /// <summary>
        /// Get a new <see cref="StreamQueueDefinition"/> on default streamConfiguration <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </summary>
        /// <remarks>
        ///     Record from default stream <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </remarks>
        public static StreamQueueDefinition CreateFromDefaultStream(string simpleNameIdentifier,
                                                                    string streamNamespace,
                                                                    string streamKey,
                                                                    Guid? fixUid = null,
                                                                    Action<IDefinitionMetaDataBuilder>? metaDataBuilder = null)
        {
            return Create(simpleNameIdentifier, StreamQueueDefinition.DEFAULT_STREAM_KEY, streamNamespace, streamKey, fixUid, metaDataBuilder);
        }

        /// <summary>
        /// Get a new <see cref="StreamQueueDefinition"/> on default streamConfiguration <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </summary>
        /// <remarks>
        ///     Record from default stream <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </remarks>
        public static StreamQueueDefinition CreateFromDefaultStream(string simpleNameIdentifier,
                                                                    Guid streamKey,
                                                                    Guid? fixUid = null,
                                                                    Action<IDefinitionMetaDataBuilder>? metaDataBuilder = null)
        {
            return Create(simpleNameIdentifier, StreamQueueDefinition.DEFAULT_STREAM_KEY, "global", streamKey, fixUid, metaDataBuilder);
        }

        /// <summary>
        /// Get a new <see cref="StreamQueueDefinition"/> on default streamConfiguration <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </summary>
        /// <remarks>
        ///     Record from default stream <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </remarks>
        public static StreamQueueDefinition CreateFromDefaultStream(string simpleNameIdentifier,
                                                                    string streamNamespace,
                                                                    Guid streamKey,
                                                                    Guid? fixUid = null,
                                                                    Action<IDefinitionMetaDataBuilder>? metaDataBuilder = null)
        {
            return Create(simpleNameIdentifier, StreamQueueDefinition.DEFAULT_STREAM_KEY, streamNamespace, streamKey, fixUid, metaDataBuilder);
        }
    }
}
