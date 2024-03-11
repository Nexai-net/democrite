﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Streams
{
    using Elvex.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// <see cref="StreamQueueDefinition"/> provider
    /// </summary>
    /// <seealso cref="IProviderStrategy{StreamQueueDefinition, Guid}" />
    public interface IStreamQueueDefinitionProvider : IProviderStrategy<StreamQueueDefinition, Guid>
    {
    }
}
