// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// MetaData about a vgrain implementation
    /// </summary>
    [Immutable]
    [DataContract]
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
        public VGrainMetaData(IEnumerable<string> vgrainInterfaces,
                              string? mainVGrainInterface,
                              bool haveState,
                              IEnumerable<string>? vgrainCategories,
                              IEnumerable<IdFormatTypeEnum> idFormatTypes,
                              bool isSingleton,
                              bool isStatelessWorker,
                              bool isDemocriteSystem)
        {

            this.VGrainInterfaces = vgrainInterfaces?.ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;
            this.MainVGrainInterface = mainVGrainInterface;
            this.HaveState = haveState;
            this.VGrainCategories = vgrainCategories?.ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;
            this.IsDemocriteSystem = isDemocriteSystem;
            this.IsSingleton = isSingleton;
            this.IsStatelessWorker = isStatelessWorker;
            this.IdFormatTypes = idFormatTypes?.ToArray() ?? EnumerableHelper<IdFormatTypeEnum>.ReadOnlyArray;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the vgrain interface implementated
        /// </summary>
        [DataMember]
        [Id(0)]
        public IReadOnlyCollection<string> VGrainInterfaces { get; }

        /// <summary>
        /// Gets the main vgrain interface declared in the <see cref="VGrainBase"/>
        /// </summary>
        [DataMember]
        [Id(1)]
        public string? MainVGrainInterface { get; }

        /// <summary>
        /// Gets a value indicating whether have a state.
        /// </summary>
        [DataMember]
        [Id(2)]
        public bool HaveState { get; }

        /// <summary>
        /// Gets the type of the vgrain.
        /// </summary>
        [DataMember]
        [Id(3)]
        public IReadOnlyCollection<string> VGrainCategories { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is democrite system.
        /// </summary>
        [DataMember]
        [Id(4)]
        public bool IsDemocriteSystem { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is singleton.
        /// </summary>
        [DataMember]
        [Id(5)]
        public bool IsSingleton { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is stateless worker.
        /// </summary>
        [DataMember]
        [Id(6)]
        public bool IsStatelessWorker { get; }

        /// <summary>
        /// Gets the identifier format types.
        /// </summary>
        [DataMember]
        [Id(7)]
        public IReadOnlyCollection<IdFormatTypeEnum> IdFormatTypes { get; }

        #endregion
    }
}
