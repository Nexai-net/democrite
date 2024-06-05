// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Keep : Democrite.Framework.Core.Abstractions.Streams
namespace Democrite.Framework.Core.Abstractions.Streams
{
    using Orleans.Runtime;

    /// <summary>
    /// 
    /// </summary>
    public static class StreamQueueExtensions
    {
        /// <summary>
        /// Generate <see cref="Orleans.Runtime.StreamId"/> from <see cref="StreamQueueDefinition"/> information
        /// </summary>
        public static StreamId ToStreamId(this StreamQueueDefinition definition)
        {
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentNullException.ThrowIfNullOrEmpty(definition.StreamNamespace);

            if (string.IsNullOrEmpty(definition.StreamKey))
            {
                if (definition.StreamUid is not null)
                    return StreamId.Create(definition.StreamNamespace, definition.StreamUid.Value);
            }
            else
            {
                if (definition.StreamUid is null)
                    return StreamId.Create(definition.StreamNamespace, definition.StreamKey);
                else
                    return StreamId.Create(definition.StreamNamespace, definition.StreamKey + "-" + definition.StreamUid);
            }

            throw new NotSupportedException("A streamQueue MUST have a key or a custom UID Failed def (" + definition.Uid + ")");
        }
    }
}
