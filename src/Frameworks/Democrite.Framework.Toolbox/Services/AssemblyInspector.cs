// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Services
{
    using Democrite.Framework.Toolbox.Abstractions.Services;

    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Inspector used to navigate to <see cref="Assembly"/> executed and satelite
    /// </summary>
    /// <seealso cref="IAssemblyInspector" />
    public sealed class AssemblyInspector : IAssemblyInspector
    {
        #region Fields

        private readonly ReaderWriterLockSlim _assemblyLock;
        private IImmutableDictionary<string, Assembly> _assemblies;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyInspector"/> class.
        /// </summary>
        public AssemblyInspector()
        {
            this._assemblies = ImmutableDictionary<string, Assembly>.Empty;
            this._assemblyLock = new ReaderWriterLockSlim();

            RegisterAssemblyAndDependencies(Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(), true);
            RegisterAssemblyAndDependencies(Assembly.GetCallingAssembly(), true);
            RegisterAssemblyAndDependencies(Assembly.GetExecutingAssembly(), true);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IReadOnlyCollection<TAttribute> GetAssemblyAttributes<TAttribute>(Assembly assembly)
            where TAttribute : Attribute
        {
            return assembly.GetCustomAttributes()
                           .Where(a => a is TAttribute)
                           .Select(a => (TAttribute)a)
                           .ToArray();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<(Assembly assembly, TAttribute attribute)> SearchAssembliesWithAttribute<TAttribute>(params Assembly[] excludes)
            where TAttribute : Attribute
        {
            var assemblies = GetRegisterAssemblies();

            var excludeIndexed = new HashSet<Assembly>(excludes);

            return (from a in assemblies
                    where !excludeIndexed.Contains(a)
                    let attr = a.GetCustomAttribute<TAttribute>()
                    where attr != null
                    select (assembly: a, attribute: attr)).ToArray();
        }

        /// <inheritdoc />
        public void RegisterAssemblyAndDependencies(Assembly assembly, bool loadDependencies = false)
        {
            this._assemblyLock.EnterWriteLock();
            try
            {
                var assemblies = this._assemblies.ToDictionary(k => k.Key.ToString(), v => v.Value);
                var countAssemblies = assemblies.Count;

                AssembliesDependencyMappingBuilder(assembly.GetName(), assemblies, loadDependencies);

                if (countAssemblies != assemblies.Count)
                    this._assemblies = assemblies.ToImmutableDictionary(k => k.Key, v => v.Value);
            }
            finally
            {
                this._assemblyLock.ExitWriteLock();
            }
        }

        #region Tools

        /// <summary>
        /// Gets the register assemblies.
        /// </summary>
        private IEnumerable<Assembly> GetRegisterAssemblies()
        {
            this._assemblyLock.EnterReadLock();
            try
            {
                return this._assemblies.Values;
            }
            finally
            {
                this._assemblyLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Map all the running assemblies, prevent multiple inspection
        /// </summary>
        private static void AssembliesDependencyMappingBuilder(AssemblyName assemblyName,
                                                               IDictionary<string, Assembly> assembliesInspect,
                                                               bool loadDependencies)
        {
            if (assembliesInspect.ContainsKey(assemblyName.ToString()))
                return;

            var assemblyRoot = Assembly.Load(assemblyName);
            assembliesInspect.Add(assemblyName.ToString(), assemblyRoot);

            if (!loadDependencies)
                return;

            var assemblies = assemblyRoot.GetReferencedAssemblies();

            foreach (var assembly in assemblies.Where(a => a != null))
                AssembliesDependencyMappingBuilder(assembly, assembliesInspect, loadDependencies);
        }

        #endregion

        #endregion
    }
}
