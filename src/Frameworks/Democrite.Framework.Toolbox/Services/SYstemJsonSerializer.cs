// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Services
{
    using Democrite.Framework.Toolbox.Abstractions.Services;

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Implementation of <see cref="IJsonSerializer"/> used .net building serializer
    /// </summary>
    /// <seealso cref="IJsonSerializer" />
    public sealed class SystemJsonSerializer : IJsonSerializer
    {
        #region Fields

        private static readonly JsonSerializerOptions s_defaultDeserializationOptions;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="SystemJsonSerializer"/> class.
        /// </summary>
        static SystemJsonSerializer()
        {
            s_defaultDeserializationOptions = new JsonSerializerOptions()
            {
                IncludeFields = true,
                PropertyNameCaseInsensitive = false,
                WriteIndented = Debugger.IsAttached,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
            };
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public object? Deserialize(string json, Type returnType)
        {
            return JsonSerializer.Deserialize(json, returnType, s_defaultDeserializationOptions);
        }

        /// <inheritdoc />
        public object? Deserialize(Stream stream, Type returnType)
        {
            return JsonSerializer.Deserialize(stream, returnType, s_defaultDeserializationOptions);
        }

        /// <inheritdoc />
        public TResult? Deserialize<TResult>(Stream stream)
        {
            return JsonSerializer.Deserialize<TResult>(stream, s_defaultDeserializationOptions);
        }

        /// <inheritdoc />
        public TResult? Deserialize<TResult>(string json)
        {
            return JsonSerializer.Deserialize<TResult>(json, s_defaultDeserializationOptions);
        }

        /// <inheritdoc />
        public string Serialize<TObject>(TObject obj)
        {
            return JsonSerializer.Serialize<TObject>(obj);
        }

        #endregion
    }
}
