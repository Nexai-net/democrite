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
            ArgumentNullException.ThrowIfNullOrEmpty(definition.StreamKey);

            if (string.IsNullOrEmpty(definition.StreamCustomKey))
            {
                if (definition.StreamCustomUid is not null)
                    return StreamId.Create(definition.StreamKey, definition.StreamCustomUid.Value);
            }
            else
            {
                if (definition.StreamCustomUid is null)
                    return StreamId.Create(definition.StreamKey, definition.StreamCustomKey);
                else
                    return StreamId.Create(definition.StreamKey, definition.StreamCustomKey + "-" + definition.StreamCustomUid);
            }

            throw new NotSupportedException("A streamQueue MUST have a key or a custom UID Failed def (" + definition.Uid + ")");
        }
    }
}
