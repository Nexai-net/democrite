﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public enum BlackboardCommandTypeEnum
    {
        None,
        Storage,
        Trigger,
        Reject,
        NotSupported,
        Deferred,
        RetryDeferred,
        Reponse
    }
}
