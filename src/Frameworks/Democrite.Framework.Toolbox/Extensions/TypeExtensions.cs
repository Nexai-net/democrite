// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Extensions
{
    using Democrite.Framework.Toolbox.Extensions.Types;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    public static class TypeExtensions
    {
        #region Fields

        private static readonly Type s_typeInfoExensionTrait = typeof(TypeInfoExtension<>);

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
        public static ITypeInfoExtension GetTypeIntoExtension(this Type type)
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

                var extendInfo = (ITypeInfoExtension)Activator.CreateInstance(s_typeInfoExensionTrait.MakeGenericType(type))!;
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
            return GetTypeIntoExtension(type)?.IsCollection ?? false;
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

            return GetTypeIntoExtension(type)?.CollectionItemType;
        }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        [Obsolete("Use GetIntoExtension(type)?.Default")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetDefaultValue(this Type type)
        {
            return GetTypeIntoExtension(type)?.Default;
        }

        /// <summary>
        /// Get a simple full readable name
        /// </summary>
        [Obsolete("Use GetTypeIntoExtension(type)?.FullShortName ?? string.Empty")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FullShortName(this Type type)
        {
            return GetTypeIntoExtension(type)?.FullShortName ?? string.Empty;
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
            if (!(type.GenericTypeArguments?.Any() ?? false))
                return type.Name;

            var builder = new StringBuilder();

            int quoteIndx = type.Name.IndexOf('`');
            if (quoteIndx > -1)
                builder.Append(type.Name, 0, quoteIndx);
            else
                builder.Append(type.Name);

            builder.Append('<');
            builder.AppendJoin(", ", type.GenericTypeArguments.Select(t => FullShortName(t)));
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
    file sealed class TypeInfoExtension<TType> : ITypeInfoExtension
    {
        #region Fields

        private static readonly ImmutableDictionary<Type, ITypeInfoExtensionEnhancer> s_enhancerCache;

        private static readonly Type s_trait;

        private static readonly Type? s_collectionItemType;
        private static readonly bool s_isCollection;
        private static readonly object? s_default;
        private static readonly string s_fullShortName;
        private static readonly bool s_isValueTask;
        private static readonly bool s_isTask;

        #endregion

        #region Ctor

        /// <summary>
        /// .cctors this instance.
        /// </summary>
        static TypeInfoExtension()
        {
            var enhancerCache = new Dictionary<Type, ITypeInfoExtensionEnhancer>();
            s_trait = typeof(TType);

            var collectionTraits = typeof(IEnumerable);

            s_isCollection = s_trait.IsAssignableTo(collectionTraits);

            if (s_isCollection)
                s_collectionItemType = TypeInfoExtensionBuilder.GetCollectionItemType(s_trait);

            s_default = default(TType);
            s_fullShortName = TypeInfoExtensionBuilder.FullShortName(s_trait);

            if (s_isCollection == false)
            {
                s_isValueTask = TypeInfoExtensionBuilder.IsValueTask(s_trait);

                if (s_isValueTask)
                    enhancerCache.Add(typeof(IValueTaskTypeInfoEnhancer), ValueTaskTypeInfoEnhancer.Create(s_trait));
            }

            if (!s_isCollection && !s_isValueTask)
            {
                s_isTask = s_trait.IsAssignableTo(typeof(Task));

                if (s_isTask)
                    enhancerCache.Add(typeof(ITaskTypeInfoEnhancer), TaskTypeInfoEnhancer.Create(s_trait));
            }

            s_enhancerCache = enhancerCache.ToImmutableDictionary();
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsCollection
        {
            get { return s_isCollection; }
        }

        /// <inheritdoc />
        public Type? CollectionItemType
        {
            get { return s_collectionItemType; }
        }

        /// <inheritdoc />
        public object? Default
        {
            get { return s_default; }
        }

        /// <inheritdoc />
        public string FullShortName
        {
            get { return s_fullShortName; }
        }

        /// <inheritdoc />
        public bool IsValueTask
        {
            get { return s_isValueTask; }
        }

        /// <inheritdoc />
        public Type Trait
        {
            get { return s_trait; }
        }

        /// <inheritdoc />
        public bool IsTask
        {
            get { return s_isTask; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a <see cref="ITypeInfoExtensionEnhancer"/> attached to this type information
        /// </summary>
        public TTypeInfoExtensionEnhancer GetSpecifcTypeExtend<TTypeInfoExtensionEnhancer>()
            where TTypeInfoExtensionEnhancer : ITypeInfoExtensionEnhancer
        {
            if (s_enhancerCache.TryGetValue(typeof(TTypeInfoExtensionEnhancer), out var enhancer))
                return (TTypeInfoExtensionEnhancer)enhancer;

            throw new InvalidOperationException("Enhancer type " + typeof(TTypeInfoExtensionEnhancer) + " not founded on type " + s_trait);
        }

        #endregion
    }
}
