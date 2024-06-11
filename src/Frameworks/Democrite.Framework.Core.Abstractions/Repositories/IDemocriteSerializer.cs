// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Repositories
{
    using Elvex.Toolbox.Abstractions.Services;

    /// <summary>
    /// Serializer that used the surrogate converters possible
    /// </summary>
    public interface IDemocriteSerializer : ISerializer
    {
        /// <summary>
        /// Based on system configuration return the serializable instance
        /// </summary>
        object? ToSerializableObject<TObj>(in TObj obj);

        /// <summary>
        /// Serializes to binary format, using orlean <see cref="IGrainStorageSerializer"/> ans surrogate if needed
        /// </summary>
        ReadOnlyMemory<byte> SerializeToBinary<TObj>(in TObj obj);

        /// <summary>
        /// Deserializes the specified serialize ibj.
        /// </summary>
        TObj Deserialize<TObj>(in ReadOnlyMemory<byte> serializeIbj);
    }
}
