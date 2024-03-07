// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace System
{
    using Democrite.Framework.Toolbox.Abstractions.Attributes;
    using Democrite.Framework.Toolbox.Abstractions.Models;
    using Democrite.Framework.Toolbox.Models;

    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extensions method used to create <see cref="AbstractType"/> and <see cref="AbstractMethod"/>
    /// </summary>
    public static class AbstractTypeExtensions
    {
        #region Fields

        private static readonly Dictionary<Type, AbstractType> s_abstractTypeCache;
        private static readonly ReaderWriterLockSlim s_abstractTypeCachedLocker;

        private static readonly Dictionary<MethodBase, AbstractMethod> s_abstractMethodCache;
        private static readonly ReaderWriterLockSlim s_abstractMethodCachedLocker;

        private static readonly Dictionary<string, MethodInfo> s_methodInfoFromAbstractCache;
        private static readonly ReaderWriterLockSlim s_methodInfoFromAbstractCacheLocker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AbstractTypeExtensions"/> class.
        /// </summary>
        static AbstractTypeExtensions()
        {
            s_abstractTypeCache = new Dictionary<Type, AbstractType>();
            s_abstractTypeCachedLocker = new ReaderWriterLockSlim();

            s_abstractMethodCache = new Dictionary<MethodBase, AbstractMethod>();
            s_abstractMethodCachedLocker = new ReaderWriterLockSlim();

            s_methodInfoFromAbstractCache = new Dictionary<string, MethodInfo>();
            s_methodInfoFromAbstractCacheLocker = new ReaderWriterLockSlim();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts to method.
        /// </summary>
        public static MethodBase ToMethod(this AbstractMethod method, Type type)
        {
            MethodInfo? mthd = null;

            s_methodInfoFromAbstractCacheLocker.EnterReadLock();
            try
            {
                if (s_methodInfoFromAbstractCache.TryGetValue(method.MethodUniqueId, out var cacheMthd))
                    return cacheMthd;
            }
            finally
            {
                s_methodInfoFromAbstractCacheLocker.ExitReadLock();
            }

            s_methodInfoFromAbstractCacheLocker.EnterWriteLock();
            try
            {
                if (s_methodInfoFromAbstractCache.TryGetValue(method.MethodUniqueId, out var cacheMthd))
                    return cacheMthd;

                mthd = type.GetAllMethodInfos(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                           .FirstOrDefault(m => method.IsEqualTo(m, true));

                if (mthd != null)
                {
                    if (mthd.IsGenericMethodDefinition && method.HasGenericArguments)
                    {
                        mthd = mthd.MakeGenericMethod(method.GenericArguments
                                                            .Select(g => g.ToType())
                                                            .ToArray());
                    }

                    if (!s_methodInfoFromAbstractCache.ContainsKey(method.MethodUniqueId))
                        s_methodInfoFromAbstractCache.Add(method.MethodUniqueId, mthd);
                }

                return mthd;
            }
            finally
            {
                s_methodInfoFromAbstractCacheLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Convert a <see cref="MethodBase"/> (Constructor or method classic) into serializable structure <see cref="AbstractMethod"/>
        /// </summary>
        public static AbstractMethod GetAbstractMethod(this MethodBase methodInfo)
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

            s_abstractTypeCachedLocker.EnterWriteLock();
            try
            {
                var buildedType = GetThreadSafeAbstractType(type);
                return buildedType!;
            }
            finally
            {
                s_abstractTypeCachedLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets all compatible abstract types (Parent, interfaces, ...)
        /// </summary>
        public static IReadOnlyCollection<AbstractType> GetAllCompatibleAbstractTypes(this Type type)
        {
            var resultAbstractTypes = new List<AbstractType>();
            var parentTypes = type.GetTypeInfoExtension().GetAllCompatibleTypes()
                                  .ToList();

            s_abstractTypeCachedLocker.EnterReadLock();
            try
            {
                for (var i = 0; i < parentTypes.Count; i++)
                {
                    var ptype = parentTypes[i];

                    if (s_abstractTypeCache.TryGetValue(ptype, out var abstractType))
                    {
                        resultAbstractTypes.Add(abstractType);
                        parentTypes.RemoveAt(i);
                        i--;
                    }
                }
            }
            finally
            {
                s_abstractTypeCachedLocker.ExitReadLock();
            }

            // Test if remain parent to be build
            if (parentTypes.Count > 0)
            {
                s_abstractTypeCachedLocker.EnterWriteLock();
                try
                {
                    // Test again to get from cache if some have been added since
                    for (var i = 0; i < parentTypes.Count; i++)
                    {
                        var ptype = parentTypes[i];

                        if (s_abstractTypeCache.TryGetValue(ptype, out var abstractType))
                        {
                            resultAbstractTypes.Add(abstractType);
                            parentTypes.RemoveAt(i);
                            i--;
                        }
                    }

                    foreach (var ptype in parentTypes)
                    {
                        var abstractType = GetThreadSafeAbstractType(ptype);
                        resultAbstractTypes.Add(abstractType);
                    }
                }
                finally
                {
                    s_abstractTypeCachedLocker.ExitWriteLock();
                }
            }

            return resultAbstractTypes;
        }

        /// <summary>
        /// Determines whether if <see cref="AbstractType"/> is equal to <see cref="Type"/>
        /// </summary>
        public static bool IsEqualTo(this AbstractType abstractType, Type type, bool checkhierachy = false)
        {
            var other = type.GetAbstractType();
            var match = abstractType?.Equals(other) ?? abstractType is null;

            if (!match && checkhierachy && abstractType is not null)
            {
                return type.GetAllCompatibleAbstractTypes()
                           .Any(m => abstractType.IsEqualTo(m!, checkhierachy: false));
            }

            return match;
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
        [ThreadSafe]
        private static AbstractType BuildAbstractType(Type type)
        {
            var extendInfo = type.GetTypeInfoExtension();
            if (type.IsGenericTypeDefinition || type.IsGenericTypeParameter)
            {
                return new GenericType(extendInfo.FullShortName,
                                       type.ContainsGenericParameters && type.IsGenericTypeParameter
                                            ? type.GetGenericParameterConstraints().Select(c => GetThreadSafeAbstractType(c))
                                            : Array.Empty<AbstractType>());
            }

            if (extendInfo.IsCollection && (type.IsArray || extendInfo.Trait.GetGenericArguments().Length > 0))
            {
                return new CollectionType(extendInfo.FullShortName,
                                          type.Namespace,
                                          type.AssemblyQualifiedName!,
                                          type.IsInterface,
                                          extendInfo.CollectionItemType!.GetThreadSafeAbstractType());
            }

            return new ConcretType(extendInfo.FullShortName,
                                    type.Namespace,
                                    type.AssemblyQualifiedName!,
                                    type.IsInterface,
                                    type.GetGenericArguments()
                                        .Select(x => GetThreadSafeAbstractType(x)));
        }

        private static AbstractMethod BuildAbstractMethod(MethodBase methodInfo)
        {
            ArgumentNullException.ThrowIfNull(methodInfo);

            AbstractType? returnType = null;

            if (methodInfo is MethodInfo mInfo)
                returnType = mInfo.ReturnType?.GetAbstractType() ?? typeof(void).GetAbstractType();

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
                                      methodInfo is ConstructorInfo,
                                      returnType,
                                      arguments,
                                      genericArgs);
        }

        /// <summary>
        /// Gets the type of the thread safe abstract.
        /// </summary>
        [ThreadSafe]
        private static AbstractType GetThreadSafeAbstractType(this Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            if (s_abstractTypeCache.TryGetValue(type, out var abstractType))
                return abstractType;

            var buildedType = BuildAbstractType(type);

            if (buildedType is not null &&
                buildedType.Category != AbstractTypeCategoryEnum.Generic &&
                buildedType.Category != AbstractTypeCategoryEnum.GenericRef)
            {
                if (s_abstractTypeCache.TryGetValue(type, out var newBuildedAbstractType))
                    return newBuildedAbstractType;
            }

            return buildedType!;
        }
        #endregion

        #endregion
    }
}
