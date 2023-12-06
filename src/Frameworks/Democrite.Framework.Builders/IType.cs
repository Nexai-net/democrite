// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    /// <summary>
    /// Inference type tool use to provide a type container
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class IType<T>
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="IType{T}"/> class.
        /// </summary>
        static IType()
        {
            Default = default;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="IType{T}"/> class from being created.
        /// </summary>
        private IType()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static T? Default { get; }

        #endregion
    }

    /// <summary>
    /// Inference type tool use to provide a type container
    /// </summary>
    public static class IType
    {
        /// <summary>
        /// Allow to declare a specific type used for inference
        /// </summary>
        public static TType? From<TType>()
        {
            return IType<TType>.Default;
        }
    }
}
