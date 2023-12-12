﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// <see cref="ITriggerDefinition"/> provider
    /// </summary>
    /// <seealso cref="IProviderStrategy{ITriggerDefinition, Guid}" />
    public interface ITriggerDefinitionProvider : IProviderStrategy<TriggerDefinition, Guid>
    {
    }
}
