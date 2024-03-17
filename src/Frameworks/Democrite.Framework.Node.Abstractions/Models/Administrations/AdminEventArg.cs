// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Models.Administrations
{
    using Democrite.Framework.Node.Abstractions.Enums;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base class of every admin event information categorise by <paramref name="Type"/> with an etag version
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]    
    public record class AdminEventArg(AdminEventTypeEnum Type, string Etag);
}
