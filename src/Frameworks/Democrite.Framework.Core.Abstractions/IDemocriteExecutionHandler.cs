﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Sequence;

    using System;

    /// <summary>
    /// Handler use to manually trigger sequence execution and get results
    /// </summary>
    public interface IDemocriteExecutionHandler
    {
        /// <summary>
        /// Prepare a direct call to an vgrain method.
        /// </summary>
        /// <remarks>
        ///     Using this call technique ensure to respect the auto-gen vgrain id and <see cref="IExecutionContext"/> information.
        /// </remarks>
        IExecutionDirectBuilder<TVGrain> VGrain<TVGrain>()
            where TVGrain : IVGrain;

        /// <summary>
        /// Prepare a direct call to an vgrain method.
        /// </summary>
        /// <remarks>
        ///     Using this call technique ensure to respect the auto-gen vgrain id and <see cref="IExecutionContext"/> information.
        /// </remarks>
        IExecutionDirectBuilder<TVGrain> VGrain<TVGrain>(Guid id, string? customIdPart = null)
            where TVGrain : IVGrain;

        /// <summary>
        /// Prepare a direct call to an vgrain method.
        /// </summary>
        /// <remarks>
        ///     Using this call technique ensure to respect the auto-gen vgrain id and <see cref="IExecutionContext"/> information.
        /// </remarks>
        IExecutionDirectBuilder<TVGrain> VGrain<TVGrain>(long id, string? customIdPart = null)
            where TVGrain : IVGrain;

        /// <summary>
        /// Prepare a direct call to an vgrain method.
        /// </summary>
        /// <remarks>
        ///     Using this call technique ensure to respect the auto-gen vgrain id and <see cref="IExecutionContext"/> information.
        /// </remarks>
        IExecutionDirectBuilder<TVGrain> VGrain<TVGrain>(string id)
            where TVGrain : IVGrain;

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/>.
        /// </summary>
        IExecutionLauncher Sequence(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/>.
        /// </summary>
        IExecutionLauncher Sequence(Guid sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/> with an input type <typeparamref name="TInput"/>.
        /// </summary>
        IExecutionBuilder<TInput> Sequence<TInput>(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/> with an input type <typeparamref name="TInput"/>.
        /// </summary>
        IExecutionBuilder<TInput> Sequence<TInput>(Guid sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/> with an input.
        /// </summary>
        IExecutionBuilder<object> SequenceWithInput(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/> with an input.
        /// </summary>
        IExecutionBuilder<object> SequenceWithInput(Guid sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions);
    }
}
