// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.References
{
    using Orleans.Runtime;

    using System;
    using System.ComponentModel;

    [ImmutableObject(true)]
    public interface IRefExecuteCommand
    {
        #region Properties

        /// <summary>
        /// Gets the execute reference.
        /// </summary>
        Uri ExecReference { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the type of the input.
        /// </summary>
        Type? GetInputType();

        /// <summary>
        /// Gets the type of the configuration.
        /// </summary>
        Type? GetConfigType();

        #endregion
    }

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    internal record struct RefVGrainExecuteCommand<TInput, TConfig>(Uri ExecReference, string simpleMethodNameIdentifier, TInput? Input = default, TConfig? Config = default, IdSpan? ForceId = null) : IRefExecuteCommand
    {
        #region Fields

        private static readonly Type s_inputType = typeof(TInput);
        private static readonly Type s_configType = typeof(TConfig);

        #endregion

        #region Methods

        /// <inheritdoc />
        public Type? GetConfigType()
        {
            return s_inputType;
        }

        /// <inheritdoc />s
        public Type? GetInputType()
        {
            return s_configType;
        }

        #endregion
    }
}
