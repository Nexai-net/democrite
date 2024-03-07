// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class BlackboardStorageLogicalTypeRule : BlackboardLogicalTypeBaseRule
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardStorageLogicalTypeRule"/> class.
        /// </summary>
        public BlackboardStorageLogicalTypeRule(string logicalTypePattern,
                                                BlackboardStorageDefinition storage) 
            : base(logicalTypePattern)
        {
            this.Storage = storage;
        }

        #endregion

        #region Properties

        [DataMember]
        public BlackboardStorageDefinition Storage { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(BlackboardLogicalTypeBaseRule other)
        {
            return other is BlackboardStorageLogicalTypeRule storageRule &&
                   this.Storage == storageRule.Storage;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.Storage?.GetHashCode() ?? 0;
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.LogicalTypePattern}] - Storage - {this.Storage.ToDebugDisplayName()}";
        }

        #endregion
    }
}
