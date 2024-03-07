// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace System
{
    using Democrite.Framework.Toolbox.Abstractions.Extensions.Types;
    using Democrite.Framework.Toolbox.Abstractions.Models;

    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;

    public static class TypeExtensions
    {
        #region Fields

        private static readonly Dictionary<Type, ITypeInfoExtension> s_typeInfoExtension;
        private static readonly ReaderWriterLockSlim s_typeInfoExtensionLocker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="TypeExtensions"/> class.
        /// </summary>
        static TypeExtensions()
        {
            s_typeInfoExtension = new Dictionary<Type, ITypeInfoExtension>();
            s_typeInfoExtensionLocker = new ReaderWriterLockSlim();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets <see cref="ITypeInfoExtension"/> to extend information currently get by reflection
        /// </summary>
        public static ITypeInfoExtension GetTypeInfoExtension(this Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            s_typeInfoExtensionLocker.EnterReadLock();
            try
            {
                if (s_typeInfoExtension.TryGetValue(type, out var info))
                    return info;
            }
            finally
            {
                s_typeInfoExtensionLocker.ExitReadLock();
            }

            s_typeInfoExtensionLocker.EnterWriteLock();
            try
            {
                if (s_typeInfoExtension.TryGetValue(type, out var info))
                    return info;

                var extendInfo = new TypeInfoExtension(type);
                s_typeInfoExtension.Add(type, extendInfo);

                return extendInfo;
            }
            finally
            {
                s_typeInfoExtensionLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Determines whether this <see cref="Type"/> is a collection type.
        /// </summary>
        [Obsolete("Use GetIntoExtension().IsCollection")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCollection(this Type type)
        {
            return GetTypeInfoExtension(type)?.IsCollection ?? false;
        }

        /// <summary>
        /// Gets the type of the item collection.
        /// </summary>
        [Obsolete("Use GetIntoExtension(type)?.CollectionItemType")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type? GetItemCollectionType(this Type? type)
        {
            if (type is null)
                return null;

            return GetTypeInfoExtension(type)?.CollectionItemType;
        }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        [Obsolete("Use GetIntoExtension(type)?.Default")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetDefaultValue(this Type type)
        {
            return GetTypeInfoExtension(type)?.Default;
        }

        /// <summary>
        /// Get a simple full readable name
        /// </summary>
        [Obsolete("Use GetTypeInfoExtension(type)?.FullShortName ?? string.Empty")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FullShortName(this Type type)
        {
            return GetTypeInfoExtension(type)?.FullShortName ?? string.Empty;
        }

        /// <summary>
        /// Stream all method infos from a type interface, parents, parent interfaces
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<MethodInfo> GetAllMethodInfos(this Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
        {
            return type.GetTreeValues(t => t.BaseType)
                       .Append(type)
                       .Where(t => t != null && t != typeof(object))
                       .SelectMany(t => t.GetInterfaces()
                                         .Append(t))
                       .Distinct()
                       .SelectMany(t => t.GetMethods(flags))
                       .Distinct();
        }

        /// <summary>
        /// Stream all property infos from a type interface, parents, parent interfaces
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<PropertyInfo> GetAllPropertyInfos(this Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
        {
            return type.GetTreeValues(t => t.BaseType)
                       .Append(type)
                       .Where(t => t != null && t != typeof(object))
                       .SelectMany(t => t.GetInterfaces()
                                         .Append(t))
                       .Distinct()
                       .SelectMany(t => t.GetProperties(flags))
                       .Distinct();
        }

        /// <summary>
        /// Gets the value from property or field.
        /// </summary>
        public static object? GetValueFromPropertyOrField(this Type type,
                                                          object? inst,
                                                          string name,
                                                          out Type fieldType,
                                                          BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            var members = type.GetMember(name, flags);

            var member = members.FirstOrDefault(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property);

            ArgumentNullException.ThrowIfNull(member);

            if (member is PropertyInfo prop)
            {
                fieldType = prop.PropertyType;
                return prop.GetValue(inst);
            }

            if (member is FieldInfo info)
            {
                fieldType = info.FieldType;
                return info.GetValue(inst);
            }

            throw new NotSupportedException("Could not extract value from " + member);
        }

        #endregion
    }

    file static class TypeInfoExtensionBuilder
    {
        #region Fields

        private static readonly Type s_collectionGenericTraits = typeof(IEnumerable<>);

        #endregion

        #region Methods

        /// <summary>
        /// Gets the type of the collection item.
        /// </summary>
        public static Type? GetCollectionItemType(Type trait)
        {
            var collectionType = trait.GetInterfaces()
                                      .Append(trait)
                                      .FirstOrDefault(g => g.IsGenericType && g.GetGenericTypeDefinition() == s_collectionGenericTraits);

            if (collectionType != null)
                return collectionType.GetGenericArguments()?.FirstOrDefault();

            return null;
        }

        /// <summary>
        /// Get a simple full readable name
        /// </summary>
        public static string FullShortName(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            if (!(type.GenericTypeArguments?.Any() ?? false) && type.IsGenericType == false)
                return type.Name;

            var builder = new StringBuilder();

            int quoteIndx = type.Name.IndexOf('`');
            if (quoteIndx > -1)
                builder.Append(type.Name, 0, quoteIndx);
            else
                builder.Append(type.Name);

            builder.Append('<');
            builder.AppendJoin(", ", type.GenericTypeArguments?.Select(t => FullShortName(t)) ?? Array.Empty<string>());
            builder.Append('>');

            return builder.ToString();
        }

        /// <summary>
        /// Determines whether if object is a <see cref="ValueTask"/> or <see cref="ValueTask{TResult}"/>
        /// </summary>
        public static bool IsValueTask(Type valueTaskType)
        {
            if (valueTaskType is null)
                return false;

            if (valueTaskType.IsGenericType)
                return valueTaskType.GetGenericTypeDefinition() == typeof(ValueTask<>);

            return valueTaskType == typeof(ValueTask);
        }

        #endregion
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="$Program"/> class.
    /// </summary>
    file sealed class TypeInfoExtension : ITypeInfoExtension
    {
        #region Fields

        private readonly ImmutableDictionary<Type, ITypeInfoExtensionEnhancer> _enhancerCache;
        private HashSet<Type>? _cachedCompatibleTypes;

        #endregion

        #region Ctor

        /// <summary>
        /// .cctors this instance.
        /// </summary>
        internal TypeInfoExtension(Type trait)
        {
            var enhancerCache = new Dictionary<Type, ITypeInfoExtensionEnhancer>();
            this.Trait = trait;

            var collectionTraits = typeof(IEnumerable);

            this.IsCollection = this.Trait.IsAssignableTo(collectionTraits);

            if (this.IsCollection)
                this.CollectionItemType = TypeInfoExtensionBuilder.GetCollectionItemType(this.Trait);

            this.IsCSharpScalarType = CSharpTypeInfo.ScalarTypes.Contains(this.Trait);

            this.Default = trait.IsValueType && !trait.IsAbstract && !trait.IsByRefLike && trait != typeof(void)
                                ? Activator.CreateInstance(trait) 
                                : null;
            this.FullShortName = TypeInfoExtensionBuilder.FullShortName(this.Trait);

            if (this.IsCollection == false)
            {
                this.IsValueTask = TypeInfoExtensionBuilder.IsValueTask(this.Trait);

                if (this.IsValueTask)
                    enhancerCache.Add(typeof(IValueTaskTypeInfoEnhancer), ValueTaskTypeInfoEnhancer.Create(this.Trait));
            }

            if (!this.IsCollection && !this.IsValueTask)
            {
                this.IsTask = this.Trait.IsAssignableTo(typeof(Task));

                if (this.IsTask && !this.Trait.IsGenericParameter &&
                    (!this.Trait.ContainsGenericParameters || !this.Trait.GetGenericArguments().Any(t => t.IsGenericParameter)))
                {
                    enhancerCache.Add(typeof(ITaskTypeInfoEnhancer), TaskTypeInfoEnhancer.Create(this.Trait));
                }
            }

            this._enhancerCache = enhancerCache.ToImmutableDictionary();
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsCollection { get; }

        /// <inheritdoc />
        public bool IsCSharpScalarType { get; }

        /// <inheritdoc />
        public Type? CollectionItemType { get; }

        /// <inheritdoc />
        public object? Default { get; }

        /// <inheritdoc />
        public string FullShortName { get; }

        /// <inheritdoc />
        public bool IsValueTask { get; }

        /// <inheritdoc />
        public Type Trait { get; }

        /// <inheritdoc />
        public bool IsTask { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a <see cref="ITypeInfoExtensionEnhancer"/> attached to this type information
        /// </summary>
        public TTypeInfoExtensionEnhancer GetSpecifcTypeExtend<TTypeInfoExtensionEnhancer>()
            where TTypeInfoExtensionEnhancer : ITypeInfoExtensionEnhancer
        {
            if (this._enhancerCache.TryGetValue(typeof(TTypeInfoExtensionEnhancer), out var enhancer))
                return (TTypeInfoExtensionEnhancer)enhancer;

            throw new InvalidOperationException("Enhancer type " + typeof(TTypeInfoExtensionEnhancer) + " not founded on type " + this.Trait);
        }

        /// <summary>
        /// Gets all compatible types (Parent class, Interfaces, ...)
        /// </summary>
        public IReadOnlyCollection<Type> GetAllCompatibleTypes()
        {
            if (this._cachedCompatibleTypes == null)
            {
                var compatibleTypes = this.Trait.GetInterfaces()
                                                .Concat(this.Trait.GetTreeValues(t => t.BaseType).Where(t => t != null && t != typeof(object)))
                                                .Append(this.Trait)
                                                .Distinct()
                                                .ToHashSet();

                this._cachedCompatibleTypes = compatibleTypes;
            }
            return this._cachedCompatibleTypes;
        }

        #endregion
    }
}
