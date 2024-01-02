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

    [KnownType(typeof(CollectionType))]
    [KnownType(typeof(ConcreteType))]
    public abstract class ConcreteBaseType : AbstractType
    {
        #region Fields

        private Type? _cachedType;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcreteType"/> class.
        /// </summary>
        public ConcreteBaseType(string displayName,
                                string? namespaceName,
                                string assemblyQualifiedName,
                                bool isInterface,
                                bool isGenericComposed,
                                AbstractTypeCategoryEnum abstractTypeCategory)
            : base(displayName, namespaceName, abstractTypeCategory)
        {
            this.AssemblyQualifiedName = assemblyQualifiedName;
            this.IsInterface = isInterface;
            this.IsGenericComposed = isGenericComposed;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type <see cref="Type.AssemblyQualifiedName"/>
        /// </summary>
        [DataMember(IsRequired = true)]
        public string AssemblyQualifiedName { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is interface.
        /// </summary>
        [DataMember(IsRequired = true)]
        public bool IsInterface { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is generic composed.
        /// </summary>
        [DataMember(IsRequired = true)]
        public bool IsGenericComposed { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public sealed override Type ToType()
        {
            if (this._cachedType == null)
                this._cachedType = Type.GetType(this.AssemblyQualifiedName, true);
            return this._cachedType!;
        }

        /// <inheritdoc />
        protected sealed override bool OnEquals(AbstractType other)
        {
            return other is ConcreteBaseType otherConcret &&
                   this.AssemblyQualifiedName == otherConcret.AssemblyQualifiedName &&
                   this.IsInterface == otherConcret.IsInterface &&
                   this.IsGenericComposed == otherConcret.IsGenericComposed &&
                   OnConcretEquals(otherConcret);
        }

        /// <inheritdoc cref="AbstractType.OnEquals(AbstractType)" />
        protected abstract bool OnConcretEquals(ConcreteBaseType otherConcret);

        /// <inheritdoc />
        protected override object OnGetHashCode()
        {
            return HashCode.Combine(this.AssemblyQualifiedName,
                                    this.IsInterface,
                                    this.IsGenericComposed,
                                    OnConcreteGetHashCode());
        }

        /// <inheritdoc cref="AbstractType.OnGetHashCode" />
        protected abstract object OnConcreteGetHashCode();

        #endregion
    }
}
