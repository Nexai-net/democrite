// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace System
{
    using Democrite.Framework.Toolbox.Abstractions.Models;
    using Democrite.Framework.Toolbox.Models;

    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Extensions method used to create <see cref="AbstractType"/> and <see cref="AbstractMethod"/>
    /// </summary>
    public static class AbstractTypeExtensions
    {
        #region Fields

        private static readonly Dictionary<Type, AbstractType> s_abstractTypeCache;
        private static readonly ReaderWriterLockSlim s_abstractTypeCachedLocker;

        private static readonly Dictionary<MethodInfo, AbstractMethod> s_abstractMethodCache;
        private static readonly ReaderWriterLockSlim s_abstractMethodCachedLocker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AbstractTypeExtensions"/> class.
        /// </summary>
        static AbstractTypeExtensions()
        {
            s_abstractTypeCache = new Dictionary<Type, AbstractType>();
            s_abstractTypeCachedLocker = new ReaderWriterLockSlim();

            s_abstractMethodCache = new Dictionary<MethodInfo, AbstractMethod>();
            s_abstractMethodCachedLocker = new ReaderWriterLockSlim();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public static AbstractMethod GetAbstractMethod(this MethodInfo methodInfo)
        {
            ArgumentNullException.ThrowIfNull(methodInfo);

            s_abstractMethodCachedLocker.EnterReadLock();
            try
            {
                if (s_abstractMethodCache.TryGetValue(methodInfo, out var abstractType))
                    return abstractType;
            }
            finally
            {
                s_abstractMethodCachedLocker.ExitReadLock();
            }

            var buildedMethodInfo = BuildAbstractMethod(methodInfo);

            s_abstractMethodCachedLocker.EnterWriteLock();
            try
            {
                if (s_abstractMethodCache.TryGetValue(methodInfo, out var abstractType))
                    return abstractType;

                s_abstractMethodCache.Add(methodInfo, buildedMethodInfo);
            }
            finally
            {
                s_abstractMethodCachedLocker.ExitWriteLock();
            }

            return buildedMethodInfo!;
        }

        /// <summary>
        /// Gets the type of the abstract.
        /// </summary>
        public static AbstractType GetAbstractType(this Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            s_abstractTypeCachedLocker.EnterReadLock();
            try
            {
                if (s_abstractTypeCache.TryGetValue(type, out var abstractType))
                    return abstractType;
            }
            finally
            {
                s_abstractTypeCachedLocker.ExitReadLock();
            }

            var buildedType = BuildAbstractType(type);

            if (buildedType is not null &&
                buildedType.Category != AbstractTypeCategoryEnum.Generic &&
                buildedType.Category != AbstractTypeCategoryEnum.GenericRef)
            {
                s_abstractTypeCachedLocker.EnterWriteLock();
                try
                {
                    if (s_abstractTypeCache.TryGetValue(type, out var abstractType))
                        return abstractType;

                    s_abstractTypeCache.Add(type, buildedType);
                }
                finally
                {
                    s_abstractTypeCachedLocker.ExitWriteLock();
                }
            }

            return buildedType!;
        }

        /// <summary>
        /// Determines whether if <see cref="AbstractType"/> is equal to <see cref="Type"/>
        /// </summary>
        public static bool IsEqualTo(this AbstractType abstractType, Type type)
        {
            var other = type.GetAbstractType();
            return abstractType?.Equals(other) ?? abstractType is null;
        }

        /// <summary>
        /// Determines whether if <see cref="AbstractMethod"/> is equal to <see cref="MethodInfo"/>
        /// </summary>
        public static bool IsEqualTo(this AbstractMethod abstractMethod, MethodInfo method, bool trySpecialization = false)
        {
            ArgumentNullException.ThrowIfNull(abstractMethod);
            ArgumentNullException.ThrowIfNull(method);

            var other = method.GetAbstractMethod();
            var result = abstractMethod?.Equals(other) ?? abstractMethod is null;

            if (result == false && trySpecialization && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == abstractMethod!.GenericArguments.Count)
            {
                var specializedMethod = method.MakeGenericMethod(abstractMethod!.GenericArguments.Select(g => g.ToType()).ToArray());
                result = IsEqualTo(abstractMethod, specializedMethod, false);
            }

            return result;  
        }

        #region Tools

        /// <summary>
        /// Builds the type of the abstract.
        /// </summary>
        private static AbstractType BuildAbstractType(Type type)
        {
            var extendInfo = type.GetTypeIntoExtension();
            if (type.IsGenericTypeDefinition || type.IsGenericTypeParameter)
            {
                return new GenericType(extendInfo.FullShortName,
                                       type.ContainsGenericParameters && type.IsGenericTypeParameter
                                            ? type.GetGenericParameterConstraints().Select(c => GetAbstractType(c))
                                            : Array.Empty<AbstractType>());
            }

            if (extendInfo.IsCollection && (type.IsArray || extendInfo.Trait.GetGenericArguments().Length > 0))
            {
                return new CollectionType(extendInfo.FullShortName,
                                          type.Namespace,
                                          type.AssemblyQualifiedName!,
                                          type.IsInterface,
                                          extendInfo.CollectionItemType!.GetAbstractType());
            }

            return new ConcreteType(extendInfo.FullShortName,
                                    type.Namespace,
                                    type.AssemblyQualifiedName!,
                                    type.IsInterface,
                                    type.GetGenericArguments()
                                        .Select(x => GetAbstractType(x)));
        }

        private static AbstractMethod BuildAbstractMethod(MethodInfo methodInfo)
        {
            ArgumentNullException.ThrowIfNull(methodInfo);

            var returnType = methodInfo.ReturnType?.GetAbstractType() ?? typeof(void).GetAbstractType();

            var arguments = methodInfo.GetParameters()
                                      .Select(p => p.ParameterType.GetAbstractType())
                                      .ToArray();

            var genericArgs = Array.Empty<AbstractType>();

            if (methodInfo.IsGenericMethod)
            {
                genericArgs = methodInfo.GetGenericArguments()
                                        .Select(g => g.GetAbstractType())
                                        .ToArray();
            }

            var methodUniqueId = ReflectionExtensions.GetUniqueId(methodInfo);
            var methodDisplayName = ReflectionExtensions.GetDisplayName(methodInfo);

            return new AbstractMethod(methodDisplayName,
                                      methodInfo.Name,
                                      methodUniqueId,
                                      returnType,
                                      arguments,
                                      genericArgs);
        }

        #endregion

        #endregion
    }
}
