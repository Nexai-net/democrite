// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.StreamQueue
{
    using Democrite.Framework.Node.Abstractions.Triggers;

    using Orleans.Streams;

    using System.Runtime.Serialization;

    /// <summary>
    /// State used to stored trigger informations
    /// </summary>
    /// <seealso cref="Democrite.Framework.Node.Abstractions.Triggers.TriggerState" />
    [GenerateSerializer]
    [DataContract]
    public class StreamTriggerState : TriggerState
    {
        [IgnoreDataMember]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public StreamSubscriptionHandle<object>? StreamSubscription { get; set; }
    }
}
