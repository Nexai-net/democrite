﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Microsoft.Extensions.Logging;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provide a proxy handler on the virtual grain requested
    /// </summary>
    public interface IVGrainProvider
    {
        /// <summary>
        /// Gets a transformer by vgrain interface type
        /// </summary>
        ValueTask<IVGrain> GetVGrainAsync(Type vgrainInterfaceType, IExecutionContext? executionContext, ILogger? logger = null);

        /// <summary>
        /// Gets a transformer by vgrain interface type
        /// </summary>
        ValueTask<IVGrain> GetVGrainAsync(Type vgrainInterfaceType, object? input, IExecutionContext? executionContext, ILogger? logger = null);

        /// <summary>
        /// Gets a transformer by vgrain interface type
        /// </summary>
        ValueTask<TVGrainType> GetVGrainAsync<TVGrainType>(IExecutionContext? executionContext, ILogger? logger = null) where TVGrainType : IVGrain;

        /// <summary>
        /// Gets a transformer by vgrain interface type
        /// </summary>
        ValueTask<TVGrainType> GetVGrainAsync<TVGrainType>(object? input, IExecutionContext? executionContext, ILogger? logger = null) where TVGrainType : IVGrain;
    }
}
