// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox
{
    using System.ComponentModel;

    /// <summary>
    /// Define a type used as none one
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public sealed class NoneType
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="NoneType"/> class.
        /// </summary>
        static NoneType()
        {
            Trait = typeof(NoneType);
            Instance = new NoneType();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="NoneType"/> class from being created.
        /// </summary>
        private NoneType()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the trait.
        /// </summary>
        public static Type Trait { get; }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static NoneType Instance { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether <typeparamref name="TType"/> is equal to <see cref="NoneType"/>.
        /// </summary>
        public static bool IsEqualTo<TType>()
        {
            return typeof(TType) == Trait;
        }

        #endregion
    }
}
