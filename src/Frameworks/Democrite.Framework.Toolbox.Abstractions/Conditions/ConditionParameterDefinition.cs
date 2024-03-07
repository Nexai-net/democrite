// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Conditions
{
    using Democrite.Framework.Toolbox.Models;

    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define one expression parameters
    /// </summary>
    /// <seealso cref="ConditionBaseDefinition" />
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class ConditionParameterDefinition : ConditionBaseDefinition
    {
        #region Fields

        public const string TypeDiscriminator = "parameter";

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionParameterDefinition"/> class.
        /// </summary>
        public ConditionParameterDefinition(Guid uid,
                                            string name,
                                            AbstractType type,
                                            ushort order)
        {
            this.Uid = uid;
            this.Name = name;
            this.Type = type;
            this.Order = order;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the uid.
        /// </summary>
        [DataMember]
        public Guid Uid { get; }

        /// <summary>
        /// Gets the name of the varibale used in the expression.
        /// </summary>
        [DataMember]
        public string Name { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        [DataMember]
        public AbstractType Type { get; }

        /// <summary>
        /// Gets the order.
        /// </summary>
        [DataMember]
        public ushort Order { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(ConditionBaseDefinition other)
        {
            return other is ConditionParameterDefinition p &&
                   this.Uid == p.Uid &&
                   this.Name == p.Name &&
                   this.Type == p.Type &&
                   this.Order == p.Order;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.Uid,
                                    this.Name,
                                    this.Type,
                                    this.Order);
        }

        #endregion
    }
}
