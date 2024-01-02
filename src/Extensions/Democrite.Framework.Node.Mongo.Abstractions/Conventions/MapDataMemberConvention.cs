// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Mongo.Abstractions.Conventions
{
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Bson.Serialization.Conventions;

    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Align map properties using <see cref="DataMemberAttribute"/> not just the one related to constructor
    /// </summary>
    /// <seealso cref="ConventionBase" />
    /// <seealso cref="IClassMapConvention" />
    internal sealed class MapDataMemberConvention : ConventionBase, IClassMapConvention
    {
        /// <inheritdoc />
        public void Apply(BsonClassMap classMap)
        {
            Debug.WriteLine("***** Map convention discrimintator to " + classMap.ClassType);

            var classType = classMap.ClassType;
            foreach (var prop in classType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                          .Where(p => p.DeclaringType == classType &&
                                                      p.CanRead &&
                                                      p.GetCustomAttribute<IgnoreDataMemberAttribute>() == null &&
                                                      p.GetCustomAttribute<BsonIgnoreAttribute>() == null))
            {
                var member = classMap.MapProperty(prop.Name);

                var dataMemberInfo = prop.GetCustomAttribute<DataMemberAttribute>();
                if (dataMemberInfo != null)
                {
                    if (dataMemberInfo.IsRequired)
                        member.SetIsRequired(true);

                    if (dataMemberInfo.Order > -1)
                        member.SetOrder(dataMemberInfo.Order);

                    if (!string.IsNullOrEmpty(dataMemberInfo.Name))
                        member.SetElementName(dataMemberInfo.Name);

                    if (dataMemberInfo.EmitDefaultValue == false)
                        member.SetIgnoreIfDefault(true);
                }

                var defaultDataMemberInfo = prop.GetCustomAttribute<DefaultValueAttribute>();
                if (defaultDataMemberInfo != null)
                    member.SetDefaultValue(defaultDataMemberInfo.Value);

                if (!BsonClassMap.IsClassMapRegistered(member.MemberType) && member.MemberType.IsClass && classType != typeof(object))
                    BsonClassMap.LookupClassMap(member.MemberType);
            }

            classMap.SetIgnoreExtraElements(true);
            classMap.SetIgnoreExtraElementsIsInherited(true);

            if (classType.IsInterface == false && classType.IsAbstract == false && classType.IsGenericType && !classType.IsGenericTypeDefinition)
            {
                var disciminatorRoot = classType.Name;

                var parts = classType.GetGenericArguments()
                                     .Select(x => (x, BsonClassMap.LookupClassMap(x)?.Discriminator))
                                     .Select(kv => string.IsNullOrEmpty(kv.Discriminator) ? kv.x.Name : kv.Discriminator)
                                     .ToArray();

                classMap.SetDiscriminator(disciminatorRoot + "<<" + string.Join(", ", parts) + ">>");
            }

            var knownTypes = classType.GetCustomAttributes<KnownTypeAttribute>(false);
            foreach (var knownType in knownTypes.Where(k => k.Type?.ContainsGenericParameters == false))
                classMap.AddKnownType(knownType.Type);

            //if (classMap.ClassType.IsClass && BsonSerializer.IsTypeDiscriminated(classMap.ClassType) && classMap.ClassType != typeof(object))
            //{
            //    try
            //    {
            //        Debug.WriteLine("***** Map convention discrimintator to " + classMap.ClassType);
            //        BsonSerializer.RegisterDiscriminatorConvention(classMap.ClassType, DiscriminatorGenericConvention.Default);
            //    }
            //    catch (Exception _)
            //    {
            //    }
            //}

            //if (!classMap.CreatorMaps.Any() && !classMap.ClassType.IsAbstract && !classMap.ClassType.IsGenericTypeDefinition)
            //{
            //    var ctors = classMap.ClassType.GetConstructors();
            //    var ctor = ctors.FirstOrDefault(c => c.GetCustomAttribute<System.Text.Json.Serialization.JsonConstructorAttribute>() != null ||
            //                                         c.GetCustomAttribute<Newtonsoft.Json.JsonConstructorAttribute>() != null);

            //    if (ctor == null && ctors.Any())
            //    {
            //        ctor = ctors.OrderByDescending(c => c.GetParameters().Length)
            //                    .FirstOrDefault();
            //    }

            //    if (ctor != null)
            //        classMap.MapConstructor(ctor);
            //}
        }
    }
}
