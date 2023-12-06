// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Manifests
{
    using Microsoft.Extensions.Options;

    using Orleans.Serialization.Configuration;
    using Orleans.Serialization.TypeSystem;

    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Provider in charge to declare manually vgrain implementation
    /// </summary>
    /// <seealso cref="ITypeManifestProvider" />
    /// <seealso cref="IConfigureOptions{TypeManifestOptions}" />
    internal sealed class DemocriteTypeManifestProvider : ITypeManifestProvider, IConfigureOptions<TypeManifestOptions>
    {
        #region Fields

        private readonly TypeManifestOptions _typeManifestOptions;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteTypeManifestProvider"/> class.
        /// </summary>
        public DemocriteTypeManifestProvider(TypeManifestOptions typeManifestOptions)
        {
            this._typeManifestOptions = typeManifestOptions;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        /// <remarks>
        ///     Register according to the option setup <see cref="ClusterNodeVGrainBuilder"/>
        /// </remarks>
        public void Configure(TypeManifestOptions options)
        {
            options.EnableConfigurationAnalysis = this._typeManifestOptions.EnableConfigurationAnalysis;

            var alias = this._typeManifestOptions.CompoundTypeAliases;
            var childrenFields = typeof(CompoundTypeAliasTree).GetField("_children", BindingFlags.NonPublic | BindingFlags.Instance);

            ArgumentNullException.ThrowIfNull(childrenFields);

            //var allFields = childrenFields.GetValue(alias) as IReadOnlyDictionary<object, CompoundTypeAliasTree>;

            if (childrenFields.GetValue(alias) is IReadOnlyDictionary<object, CompoundTypeAliasTree> allFields)
                childrenFields.SetValue(options.CompoundTypeAliases, allFields);

            ClearAndCopy(options.Activators, this._typeManifestOptions.Activators);

            options.AllowAllTypes = this._typeManifestOptions.AllowAllTypes;
            ClearAndCopy(options.AllowedTypes, this._typeManifestOptions.AllowedTypes);
            ClearAndCopy(options.Copiers, this._typeManifestOptions.Copiers);
            ClearAndCopy(options.Converters, this._typeManifestOptions.Converters);
            ClearAndCopy(options.FieldCodecs, this._typeManifestOptions.FieldCodecs);
            ClearAndCopy(options.InterfaceImplementations, this._typeManifestOptions.InterfaceImplementations);
            ClearAndCopy(options.InterfaceProxies, this._typeManifestOptions.InterfaceProxies);
            ClearAndCopy(options.Interfaces, this._typeManifestOptions.Interfaces);
            ClearAndCopy(options.Serializers, this._typeManifestOptions.Serializers);
            ClearAndCopy(options.WellKnownTypeAliases, this._typeManifestOptions.WellKnownTypeAliases);
            ClearAndCopy(options.WellKnownTypeIds, this._typeManifestOptions.WellKnownTypeIds);
        }

        /// <summary>
        /// Clears the and copy <paramref name="source"/> into <paramref name="destination"/>.
        /// </summary>
        private void ClearAndCopy<T>(ICollection<T> destination, ICollection<T> source)
        {
            destination.Clear();

            foreach (var src in source)
                destination.Add(src);
        }

        #endregion
    }
}
