// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define the default controller behavior
    /// </summary>
    public interface IControllerOptions
    {
    }

    /// <summary>
    /// Option dedicated to storage
    /// </summary>
    /// <seealso cref="IControllerOptions" />
    public interface IControllerStorageOptions : IControllerOptions
    {
    }

    /// <summary>
    /// Option dedicated to event
    /// </summary>
    /// <seealso cref="IControllerOptions" />
    public interface IControllerEventOptions : IControllerOptions
    {
    }

    /// <summary>
    /// Option dedicated to State
    /// </summary>
    /// <seealso cref="IControllerOptions" />
    public interface IControllerStateOptions : IControllerOptions
    {
    }

    /// <summary>
    /// Define the default controller behavior
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]

    [KnownType(typeof(DefaultControllerOptions))]
    
    public abstract class ControllerBaseOptions : IEquatable<ControllerBaseOptions>, IControllerOptions
    {
        #region Methods

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return OnGetHashCode();
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is ControllerBaseOptions other)
                return Equals(other);
            return false;
        }

        /// <inheritdoc />
        public bool Equals(ControllerBaseOptions? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this)) 
                return true;

            return OnEquals(other);
        }

        #region Tools

        /// <inheritdoc cref="object.Equals(object?)" />
        protected abstract bool OnEquals([NotNull] ControllerBaseOptions other);

        /// <inheritdoc cref="object.GetHashCode" />
        protected abstract int OnGetHashCode();

        #endregion

        #endregion
    }
}
