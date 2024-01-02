// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Models
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// <see cref="AbstractType"/> representing a final type that could be directly be use
    /// </summary>
    /// <seealso cref="AbstractType" />
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class ConcreteType : ConcreteBaseType
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcreteType"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public ConcreteType(string displayName,
                            string? namespaceName,
                            string assemblyQualifiedName,
                            bool isInterface,
                            IEnumerable<AbstractType> genericParameters)
            : base(displayName,
                   namespaceName,
                   assemblyQualifiedName,
                   isInterface,
                   genericParameters?.Any() ?? false,
                   AbstractTypeCategoryEnum.Concret)
        {
            this.GenericParameters = genericParameters?.ToArray() ?? Array.Empty<AbstractType>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the generic parameters if needed.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IReadOnlyList<AbstractType> GenericParameters { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnConcretEquals(ConcreteBaseType otherConcret)
        {
            return otherConcret is ConcreteType other &&
                   this.GenericParameters.SequenceEqual(other.GenericParameters);
        }

        /// <inheritdoc />
        protected override object OnConcreteGetHashCode()
        {
            return this.GenericParameters.Aggregate(0, (acc, g) => acc ^ g.GetHashCode());
        }

        #endregion
    }
}
