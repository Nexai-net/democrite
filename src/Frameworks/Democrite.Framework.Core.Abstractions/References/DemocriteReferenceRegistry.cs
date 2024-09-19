// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.References
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Models;

    using Elvex.Toolbox;
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
        public DemocriteReferenceRegistry Register(RefTypeEnum type, string simpleNameIdentifier, string? @namespace, Type typeRef)
        {
            var target = new ReferenceTypeTarget(RefIdHelper.Generate(type, simpleNameIdentifier.Trim(), @namespace?.Trim()),
                                                 type,
                                                 typeRef.GetAbstractType());

            this._referenceTargets.Add(target);
            this._typeTargets.Add(typeRef, target);

            return this;
        }

        /// <summary>
        /// Registers the method SNI 
        /// </summary>
        public DemocriteReferenceRegistry RegisterMethod(string simpleNameIdentifier, string methodName, Type typeRef, int nbGenericArgs, params Type[] args)
        {
            var methods = typeRef.GetAllMethodInfos(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                 .Where(m => m.Name == methodName)
                                 .ToArray();

            var method = methods.FirstOrDefault();
            if (methods.Length > 1)
                method = methods.FirstOrDefault(m => (nbGenericArgs == 0 || (nbGenericArgs > 0 && m.IsGenericMethod && m.GetGenericArguments().Length == nbGenericArgs)) && ParameterTypeMatch(m.GetParameters(), args));

            if (method is null)
            {
                this._logger.OptiLog(LogLevel.Warning, "[RefId] type {Type} Method tag with SNI {sni} not founded", typeRef, simpleNameIdentifier);
                return this;
            }

            ReferenceTarget? typeTarget = null;
            if (!this._typeTargets.TryGetValue(typeRef, out typeTarget))
            {
                var refType = typeRef.IsAssignableTo(typeof(IVGrain))
                                        ? typeRef.IsInterface ? RefTypeEnum.VGrain : RefTypeEnum.VGrainImplementation
                                        : RefTypeEnum.Type;

                Register(refType, typeRef.Name.ToLowerWithSeparator('-'), null, typeRef);

                typeTarget = this._typeTargets[typeRef];
            }

            var refId = RefIdHelper.WithMethod(typeTarget!.RefId, simpleNameIdentifier.Trim());

            var target = new ReferenceTypeMethodTarget(refId, RefTypeEnum.Method, typeRef.GetAbstractType(), method.GetAbstractMethod());

            var added = this._referenceTargets.Add(target);

            if (added == false)
                throw new InvalidOperationException("Failed to add the target method, another already exist with the same ref-Id: '" + refId + "' or the same method '" + method + "' found");

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
        /// Parameters the types match the type arrays
        /// </summary>
        private bool ParameterTypeMatch(ParameterInfo[] parameterInfos, Type[] types)
        {
            if (types.Length != parameterInfos.Length)
                return false;

            if (types.Length == 0)
                return true;

            for (int i = 0; i < parameterInfos.Length; ++i)
            {
                var pType = parameterInfos[i].ParameterType;
                var type = types[i];

                if (pType == type)
                    continue;

                if (AnyType.Trait == type && pType.IsTypeDefinition == false && pType.IsGenericMethodParameter)
                    continue;

                if (pType.IsGenericType && type.IsGenericType && pType.GetGenericTypeDefinition() == type.GetGenericTypeDefinition())
                    continue;

                return false;
            }

            return true;
        }

        #endregion

        #endregion
    }
}
