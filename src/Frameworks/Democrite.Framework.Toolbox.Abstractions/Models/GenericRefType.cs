// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Models
{
    using System;
    using System.ComponentModel;

    [DataObject]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class GenericRefType : AbstractType
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericType"/> class.
        /// </summary>
        internal GenericRefType(string displayName)
            : base(displayName, null, AbstractTypeCategoryEnum.GenericRef)

        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override Type ToType()
        {
            throw new NotSupportedException("Resolved the Generic refe first");
        }

        /// <inheritdoc />
        protected override bool OnEquals(AbstractType other)
        {
            return true;
        }

        /// <inheritdoc />
        protected override object OnGetHashCode()
        {
            return 0;
        }

        #endregion
    }
}
