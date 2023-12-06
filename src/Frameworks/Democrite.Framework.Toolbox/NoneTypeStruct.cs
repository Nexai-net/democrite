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
    public readonly struct NoneTypeStruct
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="NoneType"/> class.
        /// </summary>
        static NoneTypeStruct()
        {
            Trait = typeof(NoneTypeStruct);
            Instance = new NoneTypeStruct();
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
        public static NoneTypeStruct Instance { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether <typeparamref name="TType"/> is equal to <see cref="NoneType"/>.
        /// </summary>
        public static bool IsEqualTo<TType>()
            where TType : struct
        {
            return typeof(TType) == Trait;
        }

        #endregion
    }
}
