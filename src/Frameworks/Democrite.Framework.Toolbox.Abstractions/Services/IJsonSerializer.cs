// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Services
{
    /// <summary>
    /// Serializer and deserializer service
    /// </summary>
    public interface IJsonSerializer
    {
        #region Methods

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        string Serialize<TObject>(TObject obj);

        /// <summary>
        /// Convert <paramref name="json"/> into c# object
        /// </summary>
        object? Deserialize(string json, Type returnType);

        /// <summary>
        /// Convert <paramref name="stream"/> content into c# object
        /// </summary>
        object? Deserialize(Stream stream, Type returnType);

        /// <summary>
        /// Convert <paramref name="stream"/> content into c# <typeparamref name="TResult"/>
        /// </summary>
        TResult? Deserialize<TResult>(Stream stream);

        /// <summary>
        /// Convert <paramref name="json"/> into c# <typeparamref name="TResult"/>
        /// </summary>
        TResult? Deserialize<TResult>(string json);

        #endregion
    }
}
