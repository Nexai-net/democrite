// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Models
{
    using Democrite.Framework.Toolbox.Abstractions.Models;
    using Democrite.Framework.Toolbox.Disposables;

    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <inheritdoc cref="IObjectConverter" />
    public sealed class ObjectConverter : SafeDisposable, IObjectConverter
    {
        #region Fields

        private readonly IReadOnlyDictionary<Type, IReadOnlyCollection<IDedicatedObjectConverter>> _converters;

        private readonly Dictionary<(Type, Type), IDedicatedObjectConverter> _convertersCache;
        private readonly ReaderWriterLockSlim _converterCacheLocker;
        private readonly ObjectConverterOptions _objectConverterOptions;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectConverter"/> class.
        /// </summary>
        public ObjectConverter(IEnumerable<IDedicatedObjectConverter> converters, ObjectConverterOptions? converterOptions = null)
        {
            this._convertersCache = new Dictionary<(Type, Type), IDedicatedObjectConverter>();
            this._converterCacheLocker = new ReaderWriterLockSlim();

            this._objectConverterOptions = converterOptions ?? new ObjectConverterOptions();

            this._converters = converters.SelectMany(c => c.ManagedSourceTypes
                                                           .SelectMany(t => t.GetTypeIntoExtension().GetAllCompatibleTypes())
                                                           .Select(t => (Type: t, Converter: c)))
                                         .Distinct()
                                         .GroupBy(kv => kv.Type)
                                         .ToImmutableDictionary(kv => kv.Key, kv => kv.Select(s => s.Converter).ToReadOnly());
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool TryConvert<TConvertedObject>(in object? source, out TConvertedObject? result)
        {
            if (TryConvert(source, typeof(TConvertedObject), out var resultObj))
            {
                result = (TConvertedObject?)resultObj;
                return true;
            }

            result = (TConvertedObject?)typeof(TConvertedObject).GetTypeIntoExtension().Default;
            return false;
        }

        /// <inheritdoc />
        public bool TryConvert(in object? source, Type targetType, out object? result)
        {
            result = targetType.GetTypeIntoExtension().Default;

            if (source is null)
                return true;

            var sourceType = source.GetType();

            if (this._objectConverterOptions.ReuseConverter)
            {
                if (TryReuseConverter(source, sourceType, targetType, out var fromReuse))
                {
                    result = fromReuse;
                    return true;
                }
            }

            IReadOnlyCollection<IDedicatedObjectConverter>? converters = null;

            if (!this._converters.TryGetValue(sourceType, out converters) || converters == null)
                return false;

            foreach (var converter in converters)
            {
                if (converter.TryConvert(source, targetType, out var dedicatedResult))
                {
                    if (this._objectConverterOptions.ReuseConverter)
                    {
                        this._converterCacheLocker.EnterWriteLock();
                        try
                        {
                            this._convertersCache.TryAdd((sourceType, targetType), converter);
                        }
                        finally
                        {
                            this._converterCacheLocker.ExitWriteLock();
                        }
                    }

                    result = dedicatedResult;
                    return true;
                }
            }

            // Try convert using operator cast method
            // Try convert using Convert Attribute

            return false;
        }

        #region Tools

        /// <summary>
        /// Tries to reuse the last converter use to convert <paramref name="sourceType"/> to <paramref name="targetType"/>
        /// </summary>
        private bool TryReuseConverter(in object source, Type sourceType, Type targetType, out object? result)
        {
            result = null;

            IDedicatedObjectConverter? cacheConcerter = null;
            this._converterCacheLocker.EnterReadLock();
            try
            {
                if (this._convertersCache.TryGetValue((sourceType, targetType), out var converter))
                    cacheConcerter = converter;
            }
            finally
            {
                this._converterCacheLocker?.ExitReadLock();
            }

            if (cacheConcerter != null && cacheConcerter.TryConvert(source, targetType, out var resultFromCache))
            {
                result = resultFromCache;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Call at the end of the dispose process
        /// </summary>
        protected override void DisposeEnd()
        {
            this._converterCacheLocker.Dispose();
            base.DisposeEnd();
        }

        #endregion

        #endregion
    }
}
