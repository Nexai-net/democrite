// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Orleans
{
    using Democrite.Framework.Core.Abstractions;

    using Elvex.Toolbox.Models;

    using Orleans.Runtime;
    using Orleans.Services;

    using System;

    public static class DemocriteGrainExtensions
    {
        #region Fields

        private static readonly Type s_serviceTrait;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DemocriteGrainExtensions"/> class.
        /// </summary>
        static DemocriteGrainExtensions()
        {
            s_serviceTrait = typeof(IGrainService);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the dedicated grain identifier.
        /// </summary>
        public static DedicatedGrainId<TType> GetDedicatedGrainId<TType>(this IAddressable grain)
        {
            ArgumentNullException.ThrowIfNull(grain);

            if (grain is not TType)
                throw new InvalidCastException("Grain must implement the type requested " + typeof(TType));

            var expectedTypeTrait = typeof(TType);

            var grainId = grain.GetGrainId();

            var isGrainService = grain is IGrainService;

            IEnumerable<Type> hierarchyType = grain.GetType()
                                                   .GetTypeInfoExtension()
                                                   .GetAllCompatibleTypes();

            if (isGrainService)
                hierarchyType.Where(t => t.IsAssignableTo(s_serviceTrait));

            var grainType = hierarchyType.Where(h => h.IsInterface && h.IsAssignableTo(expectedTypeTrait)).FirstOrDefault();

            if (grainType is null)
                throw new InvalidCastException("Grain interface must implement the type requested " + typeof(TType));

            var dedicated = new DedicatedGrainId<TType>(grainId, grainId.Type.IsGrainService(), (ConcretType)grainType.GetAbstractType());
            return dedicated;
        }

        #endregion
    }
}
