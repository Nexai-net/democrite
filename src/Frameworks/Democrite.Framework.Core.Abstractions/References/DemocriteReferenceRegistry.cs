// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.References
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Models;

    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    public sealed class DemocriteReferenceRegistry
    {
        #region Fields

        private readonly Dictionary<Type, ReferenceTarget> _typeTargets;
        private readonly HashSet<ReferenceTarget> _referenceTargets;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteReferenceRegistry"/> class.
        /// </summary>
        public DemocriteReferenceRegistry(ILogger logger)
        {
            this._logger = logger;
            this._typeTargets = new Dictionary<Type, ReferenceTarget>();
            this._referenceTargets = new HashSet<ReferenceTarget>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers the specified type with it's simple name identifier (SNI)
        /// </summary>
        public DemocriteReferenceRegistry Register<T>(RefTypeEnum type, string simpleNameIdentifier, string? @namespace = null)
        {
            var target = new ReferenceTypeTarget(RefIdHelper.Generate(type, simpleNameIdentifier.Trim(), @namespace.Trim()),
                                                 type,
                                                 (ConcretType)typeof(T).GetAbstractType());

            this._referenceTargets.Add(target);
            this._typeTargets.Add(typeof(T), target);

            return this;
        }

        /// <summary>
        /// Registers the method SNI 
        /// </summary>
        public DemocriteReferenceRegistry RegisterMethod<T>(string simpleNameIdentifier, string methodName)
        {
            var methods = typeof(T).GetAllMethodInfos(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                   .Where(m => m.Name == methodName)
                                   .ToArray();

            var method = methods.FirstOrDefault();
            if (methods.Length > 1)
                method = methods.FirstOrDefault(m => m.GetParameters().Length == 0);

            RegisterMethod<T>(simpleNameIdentifier, method);

            return this;
        }

        /// <summary>
        /// Registers the method SNI 
        /// </summary>
        public DemocriteReferenceRegistry RegisterMethod<TArg1, T>(string simpleNameIdentifier, string methodName)
        {
            var methods = typeof(T).GetAllMethodInfos(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                   .Where(m => m.Name == methodName)
                                   .ToArray();

            var method = methods.FirstOrDefault();
            if (methods.Length > 1)
            {
                method = methods.FirstOrDefault(m => m.GetParameters().Length == 1 &&
                                   ParameterTypeMatch(m.GetParameters(), new[] { typeof(TArg1) }));
            }

            RegisterMethod<T>(simpleNameIdentifier, method);

            return this;
        }

        /// <summary>
        /// Registers the method SNI 
        /// </summary>
        public DemocriteReferenceRegistry RegisterMethod<TArg1, TArg2, T>(string simpleNameIdentifier, string methodName)
        {
            var methods = typeof(T).GetAllMethodInfos(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                   .Where(m => m.Name == methodName)
                                   .ToArray();

            var method = methods.FirstOrDefault();
            if (methods.Length > 1)
            {
                method = methods.FirstOrDefault(m => m.GetParameters().Length == 2 &&
                                   ParameterTypeMatch(m.GetParameters(), new[] { typeof(TArg1), typeof(TArg2) }));
            }

            RegisterMethod<T>(simpleNameIdentifier, method);

            return this;
        }

        #region Tools

        /// <summary>
        /// Gets the references.
        /// </summary>
        internal IReadOnlyCollection<ReferenceTarget> GetReferences()
        {
            return this._referenceTargets;
        }

        /// <summary>
        /// Registers the method.
        /// </summary>
        private void RegisterMethod<T>(string simpleNameIdentifier, MethodInfo? method)
        {
            var typeSource = typeof(T);
            if (method is null)
            {
                this._logger.OptiLog(LogLevel.Warning, "[RefId] type {Type} Method tag with SNI {sni} not founded", typeSource, simpleNameIdentifier);
                return;
            }

            ReferenceTarget? typeTarget = null;
            if (!this._typeTargets.TryGetValue(typeSource, out typeTarget))
            {
                var refType = typeSource.IsAssignableTo(typeof(IVGrain))
                                        ? typeSource.IsInterface ? RefTypeEnum.VGrain : RefTypeEnum.VGrainImplementation
                                        : RefTypeEnum.Type;

                Register<T>(refType, typeSource.Name.ToLowerWithSeparator('-'));

                typeTarget = this._typeTargets[typeSource];
            }

            var refId = RefIdHelper.WithMethod(typeTarget!.RefId, simpleNameIdentifier.Trim());

            var target = new ReferenceTypeMethodTarget(refId, RefTypeEnum.Method, (ConcretType)typeSource.GetAbstractType(), method.GetAbstractMethod());

            this._referenceTargets.Add(target);
        }

        /// <summary>
        /// Parameters the types match the type arrays
        /// </summary>
        private bool ParameterTypeMatch(ParameterInfo[] parameterInfos, Type[] types)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}
