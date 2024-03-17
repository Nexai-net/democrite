// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Surrogates
{
    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Models;

    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.Json.Serialization;

    [JsonDerivedType(typeof(CollectionConcretTypeSurrogate), "Collection")]
    [JsonDerivedType(typeof(ConcretTypeSurrogate), "Concret")]
    public interface IConcretTypeSurrogate : ISupportDebugDisplayName
    {
        string DisplayName { get; }
        string? NamespaceName { get; }
        string AssemblyQualifiedName { get; }
        bool IsInterface { get; }
    }

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct CollectionConcretTypeSurrogate(string DisplayName,
                                                        string? NamespaceName,
                                                        string AssemblyQualifiedName,
                                                        bool IsInterface,
                                                        // MOST of the time only one dependence but to prevent cycle issue in struct we use a collection
                                                        IReadOnlyCollection<IConcretTypeSurrogate>? ItemCollectionType) : IEquatable<ConcretTypeSurrogate>, ISupportDebugDisplayName, IConcretTypeSurrogate
    {
        /// <inheritdoc />
        public bool Equals(ConcretTypeSurrogate other)
        {
            return this.IsInterface == other.IsInterface &&
                   this.AssemblyQualifiedName == other.AssemblyQualifiedName;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.AssemblyQualifiedName);
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return this.AssemblyQualifiedName;
        }
    }

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct ConcretTypeSurrogate(string DisplayName,
                                              string? NamespaceName,
                                              string AssemblyQualifiedName,
                                              bool IsInterface,
                                              IReadOnlyCollection<IConcretTypeSurrogate> GenericParameters) : IEquatable<ConcretTypeSurrogate>, ISupportDebugDisplayName, IConcretTypeSurrogate
    {
        /// <inheritdoc />
        public bool Equals(ConcretTypeSurrogate other)
        {
            return this.IsInterface == other.IsInterface &&
                   this.AssemblyQualifiedName == other.AssemblyQualifiedName;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.AssemblyQualifiedName);
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return this.AssemblyQualifiedName;
        }
    }

    /// <summary>
    /// Convert toolbox <see cref="ConcretType"/> to <see cref="ConcretTypeSurrogate"/> and back
    /// </summary>
    /// <seealso cref="IConverter{ConcretType, ConcretTypeSurrogate}" />
    public static class ConcretBaseTypeConverter
    {
        #region Fields

        private static readonly Dictionary<IConcretTypeSurrogate, ConcretBaseType> s_fromSurrogateCache;
        private static readonly ReaderWriterLockSlim s_surrogateCacheLocker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcretTypeConverter"/> class.
        /// </summary>
        static ConcretBaseTypeConverter()
        {
            s_surrogateCacheLocker = new ReaderWriterLockSlim();
            s_fromSurrogateCache = new Dictionary<IConcretTypeSurrogate, ConcretBaseType>();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public static ConcretBaseType ConvertFromSurrogate(in IConcretTypeSurrogate surrogate)
        {
            s_surrogateCacheLocker.EnterReadLock();
            try
            {
                if (s_fromSurrogateCache.TryGetValue(surrogate, out var concreteResult))
                    return concreteResult;
            }
            finally
            {
                s_surrogateCacheLocker.ExitReadLock();
            }

            ConcretBaseType newResult;

            if (surrogate is CollectionConcretTypeSurrogate collectionSurrogate)
            {
                newResult = new CollectionType(collectionSurrogate.DisplayName,
                                               collectionSurrogate.NamespaceName,
                                               collectionSurrogate.AssemblyQualifiedName,
                                               collectionSurrogate.IsInterface,
                                               collectionSurrogate.ItemCollectionType?.Select(s => ConvertFromSurrogate(s)).Single() ?? typeof(object).GetAbstractType());
            }
            else if (surrogate is ConcretTypeSurrogate concret)
            {
                newResult = new ConcretType(concret.DisplayName,
                                            concret.NamespaceName,
                                            concret.AssemblyQualifiedName,
                                            concret.IsInterface,
                                            concret.GenericParameters.Select(p => ConvertFromSurrogate(p)));
            }
            else
            {
                throw new NotSupportedException("Surrogate type not managed " + surrogate);
            }

            s_surrogateCacheLocker.EnterWriteLock();
            try
            {
                s_fromSurrogateCache[surrogate] = newResult;
                return newResult;
            }
            finally
            {
                s_surrogateCacheLocker.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        public static IConcretTypeSurrogate ConvertToSurrogate(in ConcretBaseType value)
        {
            if (value is ConcretType concret)
            {
                Debug.Assert(concret.GenericParameters == null || concret.GenericParameters.Any() == false || concret.GenericParameters.All(g => g is ConcretBaseType), "Serialization support ONLY ConcretType or CollectionType");
                return new ConcretTypeSurrogate()
                {
                    DisplayName = value.DisplayName,
                    NamespaceName = value.NamespaceName,
                    AssemblyQualifiedName = value.AssemblyQualifiedName,
                    IsInterface = value.IsInterface,
                    GenericParameters = concret.GenericParameters?.Cast<ConcretBaseType>().Select(c => ConvertToSurrogate(c)).ToArray() ?? EnumerableHelper<IConcretTypeSurrogate>.ReadOnlyArray
                };
            }

            if (value is CollectionType collectionType)
            {
                Debug.Assert(collectionType.ItemAbstractType is ConcretBaseType, "Serialization support ONLY ConcretType or CollectionType as dependence");

                return new CollectionConcretTypeSurrogate()
                {
                    DisplayName = value.DisplayName,
                    NamespaceName = value.NamespaceName,
                    AssemblyQualifiedName = value.AssemblyQualifiedName,
                    IsInterface = value.IsInterface,
                    ItemCollectionType = ConvertToSurrogate((ConcretBaseType)collectionType.ItemAbstractType).AsEnumerable().ToReadOnly()
                };
            }

            throw new NotSupportedException("TO Surrogate type not managed " + value);
        }

        #endregion
    }

    [RegisterConverter]
    public sealed class ConcretTypeConverter : IConverter<ConcretType, ConcretTypeSurrogate>
    {
        /// <inheritdoc />
        public ConcretType ConvertFromSurrogate(in ConcretTypeSurrogate surrogate)
        {
            return (ConcretType)ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate);
        }

        /// <inheritdoc />
        public ConcretTypeSurrogate ConvertToSurrogate(in ConcretType value)
        {
            return (ConcretTypeSurrogate)ConcretBaseTypeConverter.ConvertToSurrogate(value);
        }
    }

    [RegisterConverter]
    public sealed class CollectionTypeConverter : IConverter<CollectionType, CollectionConcretTypeSurrogate>
    {
        /// <inheritdoc />
        public CollectionType ConvertFromSurrogate(in CollectionConcretTypeSurrogate surrogate)
        {
            return (CollectionType)ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate);
        }

        /// <inheritdoc />
        public CollectionConcretTypeSurrogate ConvertToSurrogate(in CollectionType value)
        {
            return (CollectionConcretTypeSurrogate)ConcretBaseTypeConverter.ConvertToSurrogate(value);
        }
    }
}