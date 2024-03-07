﻿namespace Democrite.Sample.Blackboard.Memory.Models
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class AutoComputeBlackboardOptions : ControllerBaseOptions, IControllerEventOptions
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoComputeBlackboardOptions"/> class.
        /// </summary>
        /// <param name="computeSequenceUid">The compute sequence uid.</param>
        public AutoComputeBlackboardOptions(Guid computeSequenceUid)
        {
            this.ComputeSequenceUid = computeSequenceUid;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the compute sequence uid.
        /// </summary>
        [Id(0)]
        [DataMember]
        public Guid ComputeSequenceUid { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals([NotNull] ControllerBaseOptions other)
        {
            return other is AutoComputeBlackboardOptions auto &&
                   auto.ComputeSequenceUid == this.ComputeSequenceUid;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.ComputeSequenceUid.GetHashCode();
        }

        #endregion
    }
}
