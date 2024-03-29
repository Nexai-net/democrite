﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using System;

    /// <summary>
    /// Define wizard tool to provide configuration for a stage
    /// </summary>
    /// <typeparam name="TPreviousMessage">The type of the previous message.</typeparam>
    public interface ISequencePipelineStageConfiguratorRoot<TWizard>
    {
        /// <summary>
        /// Define a context parameter implementation
        /// </summary>
        TWizard ContextParameter<TParameterType>(string parameterName, TParameterType value);

        /// <summary>
        /// Define a context parameter implementation
        /// </summary>
        TWizard Options<TParameterType>(Func<TParameterType> optionReturned);

        /// <summary>
        /// Define a context parameter implementation
        /// </summary>
        TWizard Options<TParameterType>(Action<TParameterType> optionSetup)
            where TParameterType : new();

        /// <summary>
        /// Force stage unique id; otherwise define by default at creation.
        /// </summary>
        TWizard Uid(Guid uid);
    }

    /// <summary>
    /// Define wizard tool to provide configuration for a stage
    /// </summary>
    public interface ISequencePipelineStageConfigurator : ISequencePipelineStageConfiguratorRoot<ISequencePipelineStageConfigurator>
    {
    }

    ///// <summary>
    ///// Define wizard tool to provide configuration for a stage
    ///// </summary>
    ///// <typeparam name="TPreviousMessage">The type of the previous message.</typeparam>
    //public interface ISequencePipelineStageConfigurator<TPreviousMessage> : ISequencePipelineStageConfiguratorRoot<ISequencePipelineStageConfigurator<TPreviousMessage>>
    //{
    //}
}
