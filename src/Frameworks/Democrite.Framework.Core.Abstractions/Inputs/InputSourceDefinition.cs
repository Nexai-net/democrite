// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Inputs
{
    using Democrite.Framework.Toolbox.Models;

    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Definition of sequence input source
    /// </summary>
    [Immutable]
    [Serializable]
    [ImmutableObject(true)]
    [DataContract]
    
    [KnownType(typeof(InputSourceStaticCollectionDefinition<>))]
    public abstract class InputSourceDefinition : IEquatable<InputSourceDefinition>
    {
        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSourceDefinition"/> class.
        /// </summary>
        protected InputSourceDefinition(InputSourceTypeEnum InputSourceType, AbstractType inputType)
        {
            this.InputSourceType = InputSourceType;
            this.InputType = inputType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of the input source.
        /// </summary>
        [IgnoreDataMember]
        public InputSourceTypeEnum InputSourceType { get; }

        /// <summary>
        /// Gets the type of the input.
        /// </summary>
        [IgnoreDataMember]
        public AbstractType InputType { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(InputSourceDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            return this.InputSourceType == other.InputSourceType &&
                   this.InputType == other.InputType &&
                   OnEquals(other);
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is InputSourceDefinition other)
                return Equals(other);

            return false;
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return this.InputSourceType.GetHashCode() ^
                   this.InputType.GetHashCode() ^
                   OnGetHashCode();
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected abstract int OnGetHashCode();

        /// <inheritdoc cref="IEquatable{T}.Equals(T)" />
        /// <remarks>
        ///     Null check and reference check are already done
        /// </remarks>
        protected abstract bool OnEquals(InputSourceDefinition other);

        #endregion
    }
}
