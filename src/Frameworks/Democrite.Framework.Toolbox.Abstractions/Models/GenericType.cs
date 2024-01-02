// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;

    [DataObject]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class GenericType : AbstractType
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericType"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        internal GenericType(string displayName,
                             IEnumerable<AbstractType> constraintTypes)
            : base(displayName, null, AbstractTypeCategoryEnum.Generic)

        {
            this.ConstraintTypes = constraintTypes?.ToArray() ?? Array.Empty<AbstractType>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the constraint types.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IReadOnlyCollection<AbstractType> ConstraintTypes { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override Type ToType()
        {
            throw new NotSupportedException("Resolved the Generic first");
        }

        /// <inheritdoc />
        protected override bool OnEquals(AbstractType other)
        {
            return other is GenericType generic &&
                   this.ConstraintTypes.SequenceEqual(generic.ConstraintTypes);
        }

        /// <inheritdoc />
        protected override object OnGetHashCode()
        {
            return this.ConstraintTypes.Aggregate(0, (acc, c) => acc ^ c.GetHashCode());
        }

        #endregion
    }
}
