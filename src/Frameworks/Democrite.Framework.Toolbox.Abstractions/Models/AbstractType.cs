// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Models
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Abstraction related to a <see cref="Type"/> Concret or Generic
    /// </summary>
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]

    [KnownType(typeof(CollectionType))]
    [KnownType(typeof(ConcretType))]
    [KnownType(typeof(GenericRefType))]
    [KnownType(typeof(GenericType))]
    public abstract class AbstractType : IEquatable<AbstractType>, IEquatable<Type>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractType"/> class.
        /// </summary>
        protected AbstractType(string displayName,
                               string? namespaceName,
                               AbstractTypeCategoryEnum category)
        {
            this.DisplayName = displayName;
            this.NamespaceName = namespaceName;
            this.Category = category;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the display name.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string DisplayName { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string? NamespaceName { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        [IgnoreDataMember]
        public string FullDisplayName
        {
            get 
            { 
                return string.IsNullOrEmpty(this.NamespaceName) 
                                ? this.DisplayName 
                                : this.NamespaceName + "." + this.DisplayName; 
            }
        }

        /// <summary>
        /// Gets the category.
        /// </summary>
        [DataMember(IsRequired = true)]
        public AbstractTypeCategoryEnum Category { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Convert <see cref="AbstractType"/> to <see cref="Type"/>
        /// </summary>
        public abstract Type ToType();

        /// <inheritdoc />
        public bool Equals(Type? other)
        {
            if (other is null)
                return false;

            return AbstractTypeExtensions.IsEqualTo(this, other);
        }

        /// <inheritdoc />
        public bool Equals(AbstractType? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            return this.DisplayName == other.DisplayName &&
                   this.Category == other.Category &&
                   OnEquals(other);
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is AbstractType abstractType)
                return Equals(abstractType);

            if (obj is Type type)
                return Equals(type);

            return false;
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.DisplayName,
                                    this.Category,
                                    OnGetHashCode());
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected abstract object OnGetHashCode();

        /// <summary>
        /// Called to check equality with <paramref name="other"/>
        /// </summary>
        /// <remarks>
        ///     Null and reference have already been checked
        /// </remarks>
        protected abstract bool OnEquals(AbstractType other);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(AbstractType? abstractType, AbstractType? other)
        {
            return !(abstractType == other);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(AbstractType? abstractType, AbstractType? other)
        {
            return abstractType?.Equals(other) ?? abstractType is null;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(AbstractType? abstractType, Type? other)
        {
            return !(abstractType == other);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(AbstractType? abstractType, Type? other)
        {
            return abstractType?.Equals(other) ?? abstractType is null;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Nullable{AbstractType}"/> to <see cref="System.Nullable{Type}"/>.
        /// </summary>
        public static implicit operator Type?(AbstractType? abstractType)
        {
            if (abstractType is null)
                return null;

            return abstractType.ToType();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Nullable{Type}"/> to <see cref="System.Nullable{AbstractType}"/>.
        /// </summary>
        public static implicit operator AbstractType?(Type? type)
        {
            if (type is null)
                return null;

            return type.GetAbstractType();
        }

        #endregion
    }
}
