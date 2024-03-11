// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Streams
{
    using Elvex.Toolbox;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IDefinition" />
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class StreamQueueDefinition : Equatable<StreamQueueDefinition>, IDefinition
    {
        #region Fields

        public const string DEFAULT_STREAM_KEY = nameof(Democrite) + "ClusterStream";

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamQueueDefinition"/> class.
        /// </summary>
        public StreamQueueDefinition(Guid uid,
                                     string displayName,
                                     string streamConfiguration,
                                     string streamKey,
                                     string? streamCustomKey,
                                     Guid? streamCustomUid)
        {
            this.Uid = uid;
            this.DisplayName = displayName;
            this.StreamKey = streamKey;
            this.StreamCustomUid = streamCustomUid;
            this.StreamCustomKey = streamCustomKey;
            this.StreamConfiguration = streamConfiguration;
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        [Id(0)]
        public Guid Uid { get; }

        /// <inheritdoc/>
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        [Id(1)]
        public string DisplayName { get; }

        /// <summary>
        /// Gets the custom stream uid to extend the key
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [Newtonsoft.Json.JsonProperty(DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [Id(2)]
        public Guid? StreamCustomUid { get; }

        /// <summary>
        /// Gets the custom stream uid to extend the key
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [Newtonsoft.Json.JsonProperty(DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [Id(3)]
        public string? StreamCustomKey { get; }

        /// <summary>
        /// Gets the stream namespace.
        /// </summary>
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        [Id(4)]
        public string StreamConfiguration { get; }

        /// <summary>
        /// Gets the stream key.
        /// </summary>
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        [Id(5)]
        public string StreamKey { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return $"[{this.StreamConfiguration}] - {this.StreamKey} - {this.StreamCustomKey}{(this.StreamCustomKey is null ? "" : "-")}{this.StreamCustomKey}";
        }

        /// <inheritdoc />
        public bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            var isValid = true;

            if (string.IsNullOrEmpty(this.StreamKey))
            {
                logger.OptiLog(LogLevel.Error, "StreamKey nust not be null or empty");
                isValid = false;
            }

            if (string.IsNullOrEmpty(this.StreamConfiguration))
            {
                logger.OptiLog(LogLevel.Error, "StreamNamespace nust not be null or empty");
                isValid = false;
            }

            if (string.IsNullOrEmpty(this.StreamCustomKey) && (this.StreamCustomUid is null || this.StreamCustomUid == Guid.Empty))
            {
                logger.OptiLog(LogLevel.Error, "A stream queue MUST have a custom uid or/and custom key");
                isValid = false;
            }

            return isValid;
        }

        /// <inheritdoc />
        protected sealed override bool OnEquals([NotNull] StreamQueueDefinition other)
        {
            return this.StreamKey == other.StreamKey &&
                   this.StreamCustomKey == other.StreamCustomKey &&
                   this.StreamConfiguration == other.StreamConfiguration &&
                   this.StreamCustomUid == other.StreamCustomUid;
        }

        /// <inheritdoc />
        protected sealed override int OnGetHashCode()
        {
            return HashCode.Combine(this.StreamCustomKey,
                                    this.StreamConfiguration,
                                    this.StreamCustomUid, 
                                    this.StreamKey);
        }

        #endregion
    }
}
