// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// 
    /// </summary>
    public abstract class Equatable<TEntity> : IEquatable<TEntity>
    {
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(TEntity? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

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
            if (obj is TEntity entity)
                return Equals(entity);

            return false;
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
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(Equatable<TEntity> x, Equatable<TEntity> y)
        {
            if (x is null)
                return y is null;

            return x.Equals(y);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(Equatable<TEntity> x, Equatable<TEntity> y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        protected abstract bool OnEquals([NotNull] TEntity other);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        protected abstract int OnGetHashCode();
    }
}
