// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Repositories
{
    using Democrite.Framework.Core.Abstractions.Repositories;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    using Newtonsoft.Json;

    using Orleans.Serialization;
    using Orleans.Serialization.Configuration;

    using System;
    using System.Buffers;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    public sealed class DemocriteSerializer : IDemocriteSerializer
    {
        #region Fields

        private static readonly Type s_genericConverterGenTraits;
        private static readonly MethodInfo s_genericDeserializer;

        private readonly IReadOnlyDictionary<Type, GenericConverter> _converterLinks;
        private readonly Dictionary<Type, IGenericConverter?> _converterCached;
        private readonly ReaderWriterLockSlim _converterCacheLocker;
        private readonly JsonSerializerSettings _jsonSettings;
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DemocriteSerializer"/> class.
        /// </summary>
        static DemocriteSerializer()
        {
            Expression<Func<DemocriteSerializer, string, int>> deserialize = (s, json) => s.Deserialize<int>(json);
            s_genericDeserializer = ((MethodCallExpression)deserialize.Body).Method.GetGenericMethodDefinition();

            s_genericConverterGenTraits = typeof(GenericConverter<,,>);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteSerializer"/> class.
        /// </summary>
        public DemocriteSerializer(IOptions<OrleansJsonSerializerOptions> serializationOptions,
                                   IOptions<TypeManifestOptions> manifests,
                                   IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;

            var options = serializationOptions.Value.JsonSerializerSettings;

            this._jsonSettings = new JsonSerializerSettings()
            {
                Converters = options.Converters,
                ContractResolver = options.ContractResolver,
                Formatting = Formatting.None,

                MissingMemberHandling = MissingMemberHandling.Ignore,
                SerializationBinder = options.SerializationBinder,
                Error = options.Error,
                NullValueHandling = options.NullValueHandling,
                TraceWriter = options.TraceWriter,
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
                ReferenceResolverProvider = options.ReferenceResolverProvider,
                ReferenceLoopHandling = options.ReferenceLoopHandling,
                ObjectCreationHandling = options.ObjectCreationHandling
            };

            this._converterCacheLocker = new ReaderWriterLockSlim();

            this._converterCached = new Dictionary<Type, IGenericConverter?>();

            this._converterLinks = manifests.Value.Converters
                                            .Select(cnv => (Converter: cnv, ConverterInterface: cnv.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConverter<,>))))
                                            .Where(kv => kv.ConverterInterface is not null)
                                            .SelectMany(kv => new[]
                                            {
                                                (Type: kv.ConverterInterface.GetGenericArguments().First(),
                                                 Source: kv.ConverterInterface.GetGenericArguments().First(),
                                                 Surrogate: kv.ConverterInterface.GetGenericArguments().Last(),
                                                 Cnv: kv.Converter,
                                                 ToSurrogate: true),

                                                (Type: kv.ConverterInterface.GetGenericArguments().Last(),
                                                 Source: kv.ConverterInterface.GetGenericArguments().First(),
                                                 Surrogate: kv.ConverterInterface.GetGenericArguments().Last(),
                                                 Cnv: kv.Converter,
                                                 ToSurrogate: false),

                                            })

                                            .Select(kv =>
                                            {
                                                var converterIsGeneric = kv.Cnv.IsGenericType;

                                                return (Type: converterIsGeneric && kv.Type.IsGenericType ? kv.Type.GetGenericTypeDefinition() : kv.Type,
                                                        Source: converterIsGeneric && kv.Source.IsGenericType ? kv.Source.GetGenericTypeDefinition() : kv.Source,
                                                        Surrogate: converterIsGeneric && kv.Surrogate.IsGenericType ? kv.Surrogate.GetGenericTypeDefinition() : kv.Surrogate,
                                                        Cnv: kv.Cnv.IsGenericType ? kv.Cnv.GetGenericTypeDefinition() : kv.Cnv,
                                                        ToSurrogate: kv.ToSurrogate);
                                            })

                                            .ToFrozenDictionary(lnk => lnk.Type,
                                                                lnk => new GenericConverter(lnk.Source, lnk.Cnv, lnk.Surrogate, lnk.ToSurrogate));
        }

        #endregion

        #region Nested

        private record class GenericConverter(Type Source,
                                              Type Converter,
                                              Type Surrogate,
                                              bool ToSurrogate);

        private interface IGenericConverter
        {
            object Convert(object value);
        }

        private sealed class GenericConverter<TConverter, TSource, TSurrogate> : IGenericConverter//: GenericConverter
            where TSurrogate : struct
            where TConverter : IConverter<TSource, TSurrogate>
        {
            #region Fields

            private readonly TConverter _converter;

            #endregion

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="GenericConverter{TConverter, TSource, TSurrogate}"/> class.
            /// </summary>
            public GenericConverter(bool toSurrogate, IServiceProvider serviceProvider)
            {
                this.ToSurrogate = toSurrogate;
                this._converter = ActivatorUtilities.CreateInstance<TConverter>(serviceProvider);
            }

            #endregion

            #region Properties

            /// <summary>
            /// Converts to surrogate.
            /// </summary>
            public bool ToSurrogate { get; }

            #endregion

            #region Methods

            public object Convert(object value)
            {
                if (this.ToSurrogate)
                    return ConvertToSurrogate(value);
                return ConvertFromSurrogate(value);
            }

            public object ConvertFromSurrogate(object value)
            {
                return this._converter.ConvertFromSurrogate((TSurrogate)value)!;
            }

            public object ConvertToSurrogate(object value)
            {
                return this._converter.ConvertToSurrogate((TSource)value)!;
            }

            #endregion 
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ReadOnlyMemory<byte> SerializeToBinary<TObj>(in TObj obj)
        {
            if (obj is null)
                return default;

            var serializable = ToSerializableObject(obj);

            if (serializable is null)
                return default;

            //var data = this._orleansJsonSerializer.Serialize(serializable, serializable?.GetType() ?? typeof(TObj));
            var data = JsonConvert.SerializeObject(serializable, this._jsonSettings);
            return new BinaryData(data);
        }

        /// <inheritdoc />
        public object? ToSerializableObject<TObj>(in TObj obj)
        {
            if (obj is null)
                return default;

            object result = obj;
            var converter = TryGetConvert(obj);
            if (converter is not null)
                result = converter.Convert(obj);

            return result;
        }

        /// <inheritdoc />
        public byte[] Serialize<TObject>(TObject obj)
        {
            var memoryBytes = SerializeToBinary(obj);
            return memoryBytes.ToArray();
        }

        /// <inheritdoc />
        public object? Deserialize(string str, Type returnType)
        {
            return s_genericDeserializer.MakeGenericMethodWithCache(returnType).Invoke(this, new object[] { str });
        }

        /// <inheritdoc />
        public object? Deserialize(in ReadOnlySpan<byte> str, Type returnType)
        {
            return s_genericDeserializer.MakeGenericMethodWithCache(returnType).Invoke(this, new object[] { Encoding.UTF8.GetString(str) });
        }

        /// <inheritdoc />
        public object? Deserialize(Stream stream, Type returnType)
        {
            using (var reader = new StreamReader(stream))
            {
                return s_genericDeserializer.MakeGenericMethodWithCache(returnType).Invoke(this, new object[] { reader.ReadToEnd() });
            }
        }

        /// <inheritdoc />
        public TResult? Deserialize<TResult>(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return Deserialize<TResult>(reader.ReadToEnd());
            }
        }

        /// <inheritdoc />
        public TObj? Deserialize<TObj>(string json)
        {
            var obj = JsonConvert.DeserializeObject(json, this._jsonSettings)!;

            if (obj is TObj correctCastObj)
                return correctCastObj;

            var converter = TryGetConvert(obj);
            if (converter is not null)
                return (TObj)converter.Convert(obj);

            throw new InvalidCastException("Could not convert data type " + obj.GetType() + " to " + typeof(TObj));
        }

        /// <inheritdoc />
        public TResult? Deserialize<TResult>(in ReadOnlySpan<byte> bytes)
        {
            return Deserialize<TResult>(Encoding.UTF8.GetString(bytes));
        }

        /// <inheritdoc />
        public TObj Deserialize<TObj>(in ReadOnlyMemory<byte> serializeobj)
        {
            return Deserialize<TObj>(Encoding.UTF8.GetString(serializeobj.ToArray()))!;
        }

        #region Tools

        private IGenericConverter? TryGetConvert<TObj>(TObj obj)
        {
            var trait = obj!.GetType();

            this._converterCacheLocker.EnterReadLock();
            try
            {
                if (this._converterCached.TryGetValue(trait, out var converter))
                    return converter;
            }
            finally
            {
                this._converterCacheLocker?.ExitReadLock();
            }

            this._converterCacheLocker.EnterWriteLock();
            try
            {
                if (this._converterCached.TryGetValue(trait, out var converter))
                    return converter;

                GenericConverter? cnv = null;
                IGenericConverter? cnvResult = null;

                if (this._converterLinks.TryGetValue(trait, out cnv) || (trait.IsGenericType && this._converterLinks.TryGetValue(trait.GetGenericTypeDefinition(), out cnv)))
                {
                    var converterType = cnv.Converter;
                    var sourceType = cnv.Source;
                    var surrogateType = cnv.Surrogate;

                    if (trait.IsGenericType && converterType.IsGenericType)
                        converterType = converterType.MakeGenericType(trait.GetGenericArguments());

                    if (trait.IsGenericType && sourceType.IsGenericType)
                        sourceType = sourceType.MakeGenericType(trait.GetGenericArguments());

                    if (trait.IsGenericType && surrogateType.IsGenericType)
                        surrogateType = surrogateType.MakeGenericType(trait.GetGenericArguments());

                    var cnvType = s_genericConverterGenTraits.MakeGenericType(converterType, sourceType, surrogateType);

                    cnvResult = (IGenericConverter?)ActivatorUtilities.CreateInstance(this._serviceProvider, cnvType, cnv.ToSurrogate);
                }

                this._converterCached.Add(trait, cnvResult);

                return cnvResult;
            }
            finally
            {
                this._converterCacheLocker?.ExitWriteLock();
            }
        }

        private static TSurrogate ConvertToSurrogate<TConverter, TSource, TSurrogate>(in TSource source, TConverter converter)
            where TSurrogate : struct
            where TConverter : IConverter<TSource, TSurrogate>
        {
            return converter.ConvertToSurrogate(source);
        }

        private static TSource ConvertFomSurrogate<TConverter, TSource, TSurrogate>(in TSurrogate surrogate, TConverter converter)
            where TSurrogate : struct
            where TConverter : IConverter<TSource, TSurrogate>
        {
            return converter.ConvertFromSurrogate(surrogate);
        }

        #endregion

        #endregion
    }
}