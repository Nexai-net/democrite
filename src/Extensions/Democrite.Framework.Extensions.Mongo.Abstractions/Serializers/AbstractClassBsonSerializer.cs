// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Abstractions.Serializers
{
    using Elvex.Toolbox.Models;

    using MongoDB.Bson;
    using MongoDB.Bson.IO;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Serializers;

    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Serializer used to ensure fallback on dynamic loading is discriminator resolution failed
    /// </summary>
    /// <seealso cref="MongoDB.Bson.Serialization.IBsonSerializer" />
    internal sealed class AbstractClassBsonSerializer<TType> : IBsonSerializer<TType>, IBsonDocumentSerializer, IBsonPolymorphicSerializer
    {
        #region Fields

        private static readonly Regex s_extractGenericCount = new Regex(@"(?<Root>[a-zA-Z0-9_]+)`(?<ParameterCount>[0-9]+)<<(?<Arguments>.*)>>");

        private static readonly StringIndexedContext s_parameterSeparators = StringIndexedContext.Create(new[] { ">>", "<<", "," }, EnumerableHelper<string>.ReadOnlyArray);

        private static readonly BsonClassMapSerializer<TType> s_default;
        private static readonly Type s_valueType;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractClassBsonSerializer{TType}"/> class.
        /// </summary>
        static AbstractClassBsonSerializer()
        {
            s_valueType = typeof(TType);
            s_default = new BsonClassMapSerializer<TType>(BsonClassMap.LookupClassMap(s_valueType));

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractClassBsonSerializer"/> class.
        /// </summary>
        public AbstractClassBsonSerializer()
        {
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public Type ValueType
        {
            get { return s_valueType; }
        }

        /// <inheritdoc />
        public bool IsDiscriminatorCompatibleWithObjectSerializer
        {
            get { return false; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return ((IBsonSerializer<TType>)this).Deserialize(context, args);
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <inheritdoc />
        void IBsonSerializer.Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            Serialize(context, args, (TType)value);
        }

        /// <inheritdoc />
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TType value)
        {
            var valueType = value?.GetType();

            if (valueType != null && args.SerializeAsNominalType == false && args.NominalType.IsGenericType == false && valueType.IsGenericType)
            {
                // Force to pass through <see cref="MapDataMemberConvention" /> that create the correct discriminator
                BsonClassMap.LookupClassMap(valueType);
            }

            //if (valueType != null && args.SerializeAsNominalType == false)
            //{
            //    var knownTypes = args.NominalType.GetCustomAttributes<KnownTypeAttribute>();

            //    var declared = knownTypes.Any(attr => attr.Type != null  && (attr.Type == valueType || (attr.Type.IsGenericType && valueType.IsGenericType && attr.Type == valueType.GetGenericTypeDefinition())));
            //    if (!declared)
            //    {

            //    }
            //}

            s_default.Serialize(context, value);
        }

        /// <inheritdoc />
#pragma warning disable CS8768 // Nullability of reference types in return type doesn't match implemented member (possibly because of nullability attributes).
        TType? IBsonSerializer<TType>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
#pragma warning restore CS8768 // Nullability of reference types in return type doesn't match implemented member (possibly because of nullability attributes).
        {
            var bookMark = context.Reader.GetBookmark();
#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                var result = s_default.Deserialize(context, args);
                return result;
            }
            catch (Exception _)
            {
                context.Reader.ReturnToBookmark(bookMark);

                var concretType = SearchCorrespondances<TType>(context.Reader, args.NominalType);

                if (concretType != null)
                    return (TType)BsonSerializer.Deserialize(context.Reader, concretType);

            }
#pragma warning restore CS0168 // Variable is declared but never used

            return default;
        }

        /// <inheritdoc />
        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            return s_default.TryGetMemberSerializationInfo(memberName, out serializationInfo);
        }

        #region Tools

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private static Type SearchCorrespondances<TSearchType>(IBsonReader bsonReader, Type nominalType)
        {
            var bookmark = bsonReader.GetBookmark();

            try
            {
                bsonReader.ReadStartDocument();
                var result = nominalType;

                if (bsonReader.FindElement("_t"))
                {
                    var context = BsonDeserializationContext.CreateRoot(bsonReader);
                    var bsonValue = BsonValueSerializer.Instance.Deserialize(context);

                    var discriminator = bsonValue as BsonString ?? throw new InvalidOperationException("Couldn't resolve type do deserialize from " + bsonValue);

                    var str = discriminator.AsString;

                    var foundType = SearchCorrespondanceForNominalType(nominalType, str);

                    if (foundType != null)
                        return foundType;

                    if (typeof(TType) != nominalType)
                    {
                        foundType = SearchCorrespondanceForNominalType(typeof(TType), str);

                        if (foundType != null)
                            return foundType;
                    }

                    if (typeof(TSearchType) != nominalType)
                    {
                        foundType = SearchCorrespondanceForNominalType(typeof(TType), str);

                        if (foundType != null)
                            return foundType;
                    }

                    throw new InvalidOperationException("Could not found type realated to discriminator '" + str + "' on nominal type " + nominalType);
                }

                return result;
            }
            finally
            {
                bsonReader.ReturnToBookmark(bookmark);
            }
        }

        /// <summary>
        /// Search for correspondance based on <paramref name="nominalType"/>
        /// </summary>
        public static Type? SearchCorrespondanceForNominalType(Type nominalType, string str)
        {
            var foundType = GetGenericTypeFromString(nominalType, str);

            if (foundType != null)
                return foundType;

            // Search through known types
            var knownTypes = nominalType.GetCustomAttributes<KnownTypeAttribute>(true);

            foreach (var knownType in knownTypes)
            {
                if (knownType.Type is null)
                    continue;

                foundType = GetGenericTypeFromString(knownType.Type, str);
                if (foundType != null)
                    return foundType;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        private static Type? GetGenericTypeFromString(Type nominalType, string str)
        {
            if (nominalType.Name == str)
                return nominalType;

            if (nominalType.IsGenericType && nominalType.IsGenericTypeDefinition == false)
            {
                var genericType = nominalType.GetGenericTypeDefinition();

                if (genericType.Name == str)
                    return genericType;
            }

            try
            {
                var foundType = BsonSerializer.LookupActualType(nominalType, str);
                return foundType;
            }
            catch
            {
            }

            var match = s_extractGenericCount.Match(str);

            if (match.Success)
            {
                var root = match.Groups["Root"].Value;
                var argumentsCount = match.Groups["ParameterCount"].Value;

                var rootType = GetGenericTypeFromString(nominalType, root + "`" + argumentsCount);

                if (rootType is not null)
                {
                    var arguments = match.Groups["Arguments"].Value;
                    var args = arguments.OptiSplit(StringIncludeSeparatorMode.Isolated,
                                                   StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries,
                                                   s_parameterSeparators);

                    var types = ResolvedArguments(new Queue<string>(args)).ToArray();

                    Debug.Assert(rootType.IsGenericTypeDefinition);
                    return rootType.MakeGenericType(types);
                }
            }

            return null;
        }

        /// <summary>
        /// Consume all the argument part split 
        /// </summary>
        private static IEnumerable<Type> ResolvedArguments(Queue<string> arguments)
        {
            while (arguments.Count > 0)
            {
                var current = arguments.Dequeue();

                if (current == ",")
                    continue;

                if (arguments.Count > 0 && arguments.Peek() == "<<")
                {
                    arguments.Dequeue();

                    // SolveRemain until ">>"
                    var other = ResolvedArguments(arguments).ToArray();

                    var genericType = current + "`" + other.Length;
                    var source = ResolveArgumentType(genericType) ?? throw new InvalidOperationException("Could not result discriminator '" + genericType + "'");

                    Debug.Assert(source.IsGenericTypeDefinition);

                    yield return source.MakeGenericType(other);
                }

                if (current == ">>")
                    yield break;

                var currentType = ResolveArgumentType(current) ?? throw new InvalidOperationException("Could not result discriminator '" + current + "'");
                yield return currentType;
            }
        }

        private static Type? ResolveArgumentType(string part)
        {
            var result = Type.GetType(part);

            result ??= Type.GetType(nameof(System) + "." + part);
            result ??= Type.GetType(nameof(System) + "." + nameof(System.Collections) + "." + part);
            result ??= Type.GetType(nameof(System) + "." + nameof(System.Collections) + "." + nameof(System.Collections.Generic) + part);

            if (result is not null)
                return result;

            try
            {
                var type = BsonSerializer.LookupActualType(typeof(object), BsonValue.Create(part));
                if (type != null)
                    return type;
            }
            catch
            {
            }

            return null;
        }

        #endregion

        #endregion
    }
}
