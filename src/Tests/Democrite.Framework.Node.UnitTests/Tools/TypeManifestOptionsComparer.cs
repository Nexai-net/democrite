// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.UnitTests.Tools
{
    using Orleans.Serialization.Configuration;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public sealed class TypeManifestOptionsComparer : IEqualityComparer<TypeManifestOptions>, IEqualityComparer
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="TypeManifestOptionsComparer"/> class.
        /// </summary>
        static TypeManifestOptionsComparer()
        {
            Default = new TypeManifestOptionsComparer();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="TypeManifestOptionsComparer"/> class from being created.
        /// </summary>
        private TypeManifestOptionsComparer()
        {

        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static TypeManifestOptionsComparer Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(TypeManifestOptions? x, TypeManifestOptions? y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            return x.Activators.SequenceEqual(y.Activators) &&
                   x.AllowAllTypes == y.AllowAllTypes &&
                   //x.CompoundTypeAliases.GetHashCode() ^ recurse tree
                   x.AllowedTypes.SequenceEqual(y.AllowedTypes) &&
                   x.Converters.SequenceEqual(y.Converters) &&
                   x.Copiers.SequenceEqual(y.Copiers) &&
                   x.EnableConfigurationAnalysis == y.EnableConfigurationAnalysis &&
                   x.FieldCodecs.SequenceEqual(y.FieldCodecs) &&
                   x.InterfaceImplementations.SequenceEqual(y.InterfaceImplementations) &&
                   x.InterfaceProxies.SequenceEqual(y.InterfaceProxies) &&
                   x.Interfaces.SequenceEqual(y.Interfaces) &&
                   x.Serializers.SequenceEqual(y.Serializers) &&
                   x.WellKnownTypeAliases.SequenceEqual(y.WellKnownTypeAliases) &&
                   x.WellKnownTypeIds.SequenceEqual(y.WellKnownTypeIds);
        }

        /// <inheritdoc />
        bool IEqualityComparer.Equals(object? x, object? y)
        {
            return Equals(x as TypeManifestOptions, y as TypeManifestOptions);
        }

        /// <inheritdoc />
        public int GetHashCode([DisallowNull] TypeManifestOptions obj)
        {
            return obj.Activators.OrderBy(t => t.FullName).Aggregate(0, (acc, t) => acc ^ t.GetHashCode()) ^
                   obj.AllowAllTypes.GetHashCode() ^
                   //obj.CompoundTypeAliases.GetHashCode() ^ recurse tree
                   obj.AllowedTypes.OrderBy(t => t).Aggregate(0, (acc, t) => acc ^ t.GetHashCode()) ^
                   obj.Converters.OrderBy(t => t.FullName).Aggregate(0, (acc, t) => acc ^ t.GetHashCode()) ^
                   obj.Copiers.OrderBy(t => t.FullName).Aggregate(0, (acc, t) => acc ^ t.GetHashCode()) ^
                   (obj.EnableConfigurationAnalysis?.GetHashCode() ?? 0) ^
                   obj.FieldCodecs.OrderBy(t => t.FullName).Aggregate(0, (acc, t) => acc ^ t.GetHashCode()) ^
                   obj.InterfaceImplementations.OrderBy(t => t.FullName).Aggregate(0, (acc, t) => acc ^ t.GetHashCode()) ^
                   obj.InterfaceProxies.OrderBy(t => t.FullName).Aggregate(0, (acc, t) => acc ^ t.GetHashCode()) ^
                   obj.Interfaces.OrderBy(t => t.FullName).Aggregate(0, (acc, t) => acc ^ t.GetHashCode()) ^
                   obj.Serializers.OrderBy(t => t.FullName).Aggregate(0, (acc, t) => acc ^ t.GetHashCode()) ^
                   obj.WellKnownTypeAliases.OrderBy(t => t.Value.FullName).Aggregate(0, (acc, t) => acc ^ t.Key.GetHashCode() ^ t.Value.GetHashCode()) ^
                   obj.WellKnownTypeIds.OrderBy(t => t.Value.FullName).Aggregate(0, (acc, t) => acc ^ t.Key.GetHashCode() ^ t.Value.GetHashCode());
        }

        /// <inheritdoc />
        public int GetHashCode(object obj)
        {
            if (obj is TypeManifestOptions opt)
                return GetHashCode(opt);
            return 0;
        }

        #endregion
    }
}
