// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Conditions
{
    using Newtonsoft.Json;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using System.Text.Json.Serialization.Metadata;

    /// <summary>
    /// Base of all condition part
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$discriminator")]
    [JsonDerivedType(typeof(ConditionParameterDefinition), ConditionParameterDefinition.TypeDiscriminator)]
    [JsonDerivedType(typeof(ConditionCallDefinition), ConditionCallDefinition.TypeDiscriminator)]
    [JsonDerivedType(typeof(ConditionGroupDefinition), ConditionGroupDefinition.TypeDiscriminator)]
    [JsonDerivedType(typeof(ConditionMathOperationDefinition), ConditionMathOperationDefinition.TypeDiscriminator)]
    [JsonDerivedType(typeof(ConditionMemberAccessDefinition), ConditionMemberAccessDefinition.TypeDiscriminator)]
    [JsonDerivedType(typeof(ConditionOperandDefinition), ConditionOperandDefinition.TypeDiscriminator)]
    [JsonDerivedType(typeof(ConditionValueDefinition), ConditionValueDefinition.TypeDiscriminator)]
    [JsonDerivedType(typeof(ConditionConvertDefinition), ConditionConvertDefinition.TypeDiscriminator)]

    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]

    [KnownType(typeof(ConditionParameterDefinition))]
    [KnownType(typeof(ConditionCallDefinition))]
    [KnownType(typeof(ConditionGroupDefinition))]
    [KnownType(typeof(ConditionMathOperationDefinition))]
    [KnownType(typeof(ConditionMemberAccessDefinition))]
    [KnownType(typeof(ConditionOperandDefinition))]
    [KnownType(typeof(ConditionValueDefinition))]
    [KnownType(typeof(ConditionConvertDefinition))]

    public abstract class ConditionBaseDefinition : IEquatable<ConditionBaseDefinition>
    {
        #region Properties

        /// Initializes a new instance of the <see cref="ConditionBaseDefinition"/> class.
        /// </summary>
        public ConditionBaseDefinition()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Equals(ConditionBaseDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (other.GetType() != GetType())
                return false;

            return OnEquals(other);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.
        /// </returns>
        public sealed override bool Equals(object? obj)
        {
            return obj is ConditionBaseDefinition def && Equals(def);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(ConditionBaseDefinition x, ConditionBaseDefinition y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(ConditionBaseDefinition x, ConditionBaseDefinition y)
        {
            return x?.Equals(y) ?? y is null;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public sealed override int GetHashCode()
        {
            return OnGetHashCode();
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        protected abstract int OnGetHashCode();

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <remarks>
        ///     Type, Null and reference check already done
        /// </remarks>
        protected abstract bool OnEquals(ConditionBaseDefinition other);
        
        #endregion
    }
}
