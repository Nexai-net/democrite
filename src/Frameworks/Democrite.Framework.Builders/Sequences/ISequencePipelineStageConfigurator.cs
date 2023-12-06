// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using System;

    /// <summary>
    /// Define wizard tool to provide configuration for a stage
    /// </summary>
    public interface ISequencePipelineStageConfigurator
    {
        /// <summary>
        /// Define a context parameter value
        /// </summary>
        ISequencePipelineStageConfigurator ContextParameter<TParameterType>(string parameterName, TParameterType value);

        /// <summary>
        /// Define a context parameter implementation
        /// </summary>
        ISequencePipelineStageConfigurator Options<TParameterType>(Func<TParameterType> optionReturned);

        /// <summary>
        /// Define a context parameter implementation
        /// </summary>
        ISequencePipelineStageConfigurator Options<TParameterType>(Action<TParameterType> optionSetup)
            where TParameterType : new();

        /// <summary>
        /// Force stage unique id
        /// </summary>
        ISequencePipelineStageConfigurator Uid(Guid uid);
    }

    /// <summary>
    /// Define wizard tool to provide configuration for a stage
    /// </summary>
    /// <typeparam name="TPreviousMessage">The type of the previous message.</typeparam>
    public interface ISequencePipelineStageConfigurator<TPreviousMessage>
    {
        /// <summary>
        /// Define a context parameter implementation
        /// </summary>
        ISequencePipelineStageConfigurator<TPreviousMessage> ContextParameter<TParameterType>(string parameterName, TParameterType value);

        /// <summary>
        /// Define a context parameter implementation
        /// </summary>
        ISequencePipelineStageConfigurator<TPreviousMessage> Options<TParameterType>(Func<TParameterType> optionReturned);

        /// <summary>
        /// Define a context parameter implementation
        /// </summary>
        ISequencePipelineStageConfigurator<TPreviousMessage> Options<TParameterType>(Action<TParameterType> optionSetup)
            where TParameterType : new();

        /// <summary>
        /// Force stage unique id; otherwise define by default at creation.
        /// </summary>
        ISequencePipelineStageConfigurator<TPreviousMessage> Uid(Guid uid);
    }
}
