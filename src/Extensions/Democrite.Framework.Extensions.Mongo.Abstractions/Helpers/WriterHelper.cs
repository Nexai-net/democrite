// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Abstractions.Helpers
{
    using MongoDB.Bson.Serialization;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public static class WriterHelper
    {
        #region Methods

        /// <summary>
        /// Writes the class member without open/close brasket and discriminator
        /// </summary>
        public static void WriteClass(BsonSerializationContext context, BsonClassMap map, object value)
        {
            var writer = context.Writer;

            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            var type = value.GetType();

            if (map.IdMemberMap != null)
            {
                writer.WriteName("_id");
                WriteMember(context, map.IdMemberMap, value);
            }

            foreach (var member in map.AllMemberMaps.Where(m => map.IdMemberMap == null || m.MemberInfo != map.IdMemberMap.MemberInfo))
            {
                var propValue = member.Getter(value);

                if (member.IgnoreIfNull && value is null)
                    continue;

                if (member.IgnoreIfDefault)
                {
                    if (propValue == member.DefaultValue)
                        continue;

                    var typeExtendInfo = member.MemberType.GetTypeInfoExtension();
                    if (typeExtendInfo.IsCollection && value is IEnumerable collection && !collection.AsEnumerable().Any())
                        continue;
                }

                writer.WriteName(member.ElementName);

                WriteMember(context, member, propValue);
            }
        }

        private static void WriteMember(BsonSerializationContext context, BsonMemberMap member, object value)
        {
            var serializer = member.GetSerializer();
            serializer.Serialize(context, value ?? member.DefaultValue);
        }


        #endregion
    }
}
