// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Models.References;
    using Democrite.Framework.Core.Abstractions.Sequence;

    using System;

    /// <summary>
    /// Handler use to manually trigger sequence execution and get results
    /// </summary>
    public interface IDemocriteExecutionHandler
    {
        #region VGrain Direct Call

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
        IExecutionDirectBuilder<TVGrain> VGrain<TVGrain>(string id)
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
        IExecutionRefBuilder VGrain(Uri refId);

        /// <summary>
        /// Prepare a direct call to an vgrain method.
        /// </summary>
        IExecutionRefBuilder VGrain(RefVGrainQuery refIdQuery);

        /// <summary>
        /// Prepare a direct call to an vgrain method.
        /// </summary>
        IExecutionRefBuilder VGrain(RefVGrainQuery refIdQuery, string id);

        /// <summary>
        /// Prepare a direct call to an vgrain method.
        /// </summary>
        IExecutionRefBuilder VGrain(RefVGrainQuery refIdQuery, Guid id, string? customIdPart = null);

        /// <summary>
        /// Prepare a direct call to an vgrain method.
        /// </summary>
        IExecutionRefBuilder VGrain(RefVGrainQuery refIdQuery, long id, string? customIdPart = null);

        #endregion

        #region Sequence by Id

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/>.
        /// </summary>
        IExecutionFlowLauncher Sequence(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/>.
        /// </summary>
        IExecutionFlowLauncher Sequence(Guid sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/> with an input type <typeparamref name="TInput"/>.
        /// </summary>
        IExecutionBuilder<TInput, IExecutionFlowLauncher> Sequence<TInput>(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/> with an input type <typeparamref name="TInput"/>.
        /// </summary>
        IExecutionBuilder<TInput, IExecutionFlowLauncher> Sequence<TInput>(Guid sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/> with an input.
        /// </summary>
        IExecutionBuilder<object, IExecutionFlowLauncher> SequenceWithInput(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/> with an input.
        /// </summary>
        IExecutionBuilder<object, IExecutionFlowLauncher> SequenceWithInput(Guid sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions);

        #endregion

        #region Sequence by Ref

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/>.
        /// </summary>
        IExecutionFlowLauncher Sequence(RefSequenceQuery sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/>.
        /// </summary>
        IExecutionFlowLauncher Sequence(RefSequenceQuery sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/> with an input type <typeparamref name="TInput"/>.
        /// </summary>
        IExecutionBuilder<TInput, IExecutionFlowLauncher> Sequence<TInput>(RefSequenceQuery sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/> with an input type <typeparamref name="TInput"/>.
        /// </summary>
        IExecutionBuilder<TInput, IExecutionFlowLauncher> Sequence<TInput>(RefSequenceQuery sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/> with an input.
        /// </summary>
        IExecutionBuilder<object, IExecutionFlowLauncher> SequenceWithInput(RefSequenceQuery sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null);

        /// <summary>
        /// Prepare an execution with schema a <see cref="SequenceDefinition"/> with an input.
        /// </summary>
        IExecutionBuilder<object, IExecutionFlowLauncher> SequenceWithInput(RefSequenceQuery sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions);

        #endregion
    }
}
