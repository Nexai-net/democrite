// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Tests
{
    using Democrite.Framework.Core.Abstractions;

    using System;
    using System.Reflection;

    /// <summary>
    /// Base clase of grain implementation
    /// </summary>
    /// <typeparam name="TGrainImpl">The type of the grain implementation.</typeparam>
    public abstract class VGrainBaseUTest<TGrainImpl>
        where TGrainImpl : IVGrain, IGrainBase
    {
        #region Fields

        private static readonly PropertyInfo? s_stateAccess;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="VGrainBaseUTest{TGrainImpl}"/> class.
        /// </summary>
        static VGrainBaseUTest()
        {
            var stateAccess = typeof(TGrainImpl).GetProperty("State", BindingFlags.NonPublic | BindingFlags.Instance);

            if (stateAccess != null)
                s_stateAccess = stateAccess;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the state of the grain.
        /// </summary>
        public static TState? GetGrainState<TState>(TGrainImpl grainImpl)
        {
            if (s_stateAccess == null)
                throw new NotSupportedException(typeof(TGrainImpl) + " doesn't have a state");

            return (TState?)s_stateAccess.GetValue(grainImpl);
        }

        #endregion
    }
}
