// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Models
{
    using Democrite.Framework.Toolbox.Helpers;

    using System.ComponentModel;

    /// <summary>
    /// MetaData about a vgrain implementation
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public readonly struct VGrainMetaData
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainMetaData"/> struct.
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        [System.Text.Json.Serialization.JsonConstructor]
        public VGrainMetaData(string implementation,
                              bool haveState,
                              IEnumerable<string>? vgrainCategories,
                              bool isDemocriteSystem)
        {

            this.Implementation = implementation;
            this.HaveState = haveState;
            this.VGrainCategories = vgrainCategories?.ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;
            this.IsDemocriteSystem = isDemocriteSystem;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of the impentation.
        /// </summary>
        [Id(0)]
        public string Implementation { get; }

        /// <summary>
        /// Gets a value indicating whether have a state.
        /// </summary>
        [Id(1)]
        public bool HaveState { get; }

        /// <summary>
        /// Gets the type of the vgrain.
        /// </summary>
        [Id(2)]
        public IReadOnlyCollection<string> VGrainCategories { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is democrite system.
        /// </summary>
        [Id(3)]
        public bool IsDemocriteSystem { get; }

        #endregion
    }
}
