﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using System;
    using System.ComponentModel;

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public readonly record struct SubscriptionId(Guid SignalId, bool FromDoor, Guid Uid);
}
