// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Conditions
{
    using Newtonsoft.Json;

    using System;
    using System.ComponentModel;

    [Serializable]
    [ImmutableObject(true)]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public sealed class ConditionExpressionDefinition : IEquatable<ConditionExpressionDefinition>
    {
        #region Fields

        public const string TypeDiscriminator = "cond";

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionExpressionDefinition"/> class.
        /// </summary>
        public ConditionExpressionDefinition(IEnumerable<ConditionParameterDefinition> parameters, ConditionBaseDefinition condition)
        {
            this.Parameters = parameters?.ToArray() ?? Array.Empty<ConditionParameterDefinition>();
            this.Condition = condition;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public IReadOnlyCollection<ConditionParameterDefinition> Parameters { get; }

        /// <summary>
        /// Gets the condition.
        /// </summary>
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public ConditionBaseDefinition Condition { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(ConditionExpressionDefinition x, ConditionExpressionDefinition y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(ConditionExpressionDefinition x, ConditionExpressionDefinition y)
        {
            return x?.Equals(y) ?? y is null;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Equals(ConditionExpressionDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            return this.Parameters.SequenceEqual(other.Parameters) &&
                   this.Condition.Equals(other.Condition);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return obj is ConditionExpressionDefinition expr && Equals(expr);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Parameters.Aggregate(0, (acc, p) => acc ^ p.GetHashCode()),
                                    this.Condition);
        }

        #endregion
    }
}