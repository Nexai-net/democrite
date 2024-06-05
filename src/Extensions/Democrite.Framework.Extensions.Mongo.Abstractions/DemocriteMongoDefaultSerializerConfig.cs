// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Abstractions
{
    using Democrite.Framework.Extensions.Mongo.Abstractions.Conventions;
    using Democrite.Framework.Extensions.Mongo.Abstractions.Serializers;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Conventions;
    using MongoDB.Bson.Serialization.Serializers;

    using System.Collections.Immutable;
    using System.Linq;

    public static class DemocriteMongoDefaultSerializerConfig
    {
        #region Fields

        private static long s_lockCounter;

        #endregion

        /// <summary>
        /// Setups the serialization configuration.
        /// </summary>
        public static void SetupSerializationConfiguration()
        {
            if (Interlocked.Increment(ref s_lockCounter) > 1)
                return;

            ConventionRegistry.Remove("__defaults__");
            ConventionRegistry.Remove("__attributes__");

            var pack = new ConventionPack();
            var defaultConventions = DefaultConventionPack.Instance.Conventions;

            pack.AddRange(defaultConventions.Except(defaultConventions.OfType<NamedParameterCreatorMapConvention>()));
            
            if (!pack.Any(p => p.GetType() == typeof(MapDataMemberConvention)))
                pack.Add(new MapDataMemberConvention());

            if (!pack.Any(p => p.GetType() == typeof(EnhancedNamedParameterCreatorMapConvention)))
                pack.Add(new EnhancedNamedParameterCreatorMapConvention());

            ConventionRegistry.Register("__defaults__", pack, t => true);

            var attributePack = new ConventionPack();
            attributePack.AddRange(AttributeConventionPack.Instance.Conventions);

            ConventionRegistry.Register("__attributes__", attributePack, t => true);

            try
            {
                BsonSerializer.RegisterSerializationProvider(new AbstractClassBsonSerializationProvider());
            }
            catch
            {

            }

            try
            {
                BsonSerializer.TryRegisterSerializer(GrainIdSerializer.Instance);
            }
            catch
            {
            }

            try
            {
                BsonSerializer.TryRegisterSerializer(new TypeSerializer());
            }
            catch
            {
            }

            try
            {
                BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));
            }
            catch
            {
            }

            try
            {
                BsonSerializer.TryRegisterSerializer(NoneTypeStringConverter.Instance);
            }
            catch
            {
            }

            try
            {
                BsonSerializer.TryRegisterSerializer(new ObjectSerializer(t => true));
            }
            catch
            {

            }


            try
            {
                BsonSerializer.RegisterGenericSerializerDefinition(typeof(ImmutableArray<>), typeof(ImmutableArraySerializer<>));
            }
            catch
            {

            }
        }
    }
}
