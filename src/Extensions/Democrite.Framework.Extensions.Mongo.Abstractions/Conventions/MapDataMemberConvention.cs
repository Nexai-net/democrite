// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Abstractions.Conventions
{
    using Democrite.Framework.Core.Abstractions.Repositories;

    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Bson.Serialization.Conventions;

    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Align map properties using <see cref="DataMemberAttribute"/> not just the one related to constructor
    /// </summary>
    /// <seealso cref="ConventionBase" />
    /// <seealso cref="IClassMapConvention" />
    internal sealed class MapDataMemberConvention : ConventionBase, IClassMapConvention
    {
        #region Fields

        private static readonly HashSet<Type> s_mapTypes;
        private static readonly string[] s_preventNamespaces;

        #endregion

        #region Ctor        

        /// <summary>
        /// Initializes the <see cref="MapDataMemberConvention"/> class.
        /// </summary>
        static MapDataMemberConvention()
        {
            s_mapTypes = new HashSet<Type>();
            s_preventNamespaces = new string[]
            {
              nameof(MongoDB) + "."  + nameof(MongoDB.Bson),
              nameof(System),
              nameof(Orleans) + "." + nameof(Orleans.Providers) + "."
            };
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Apply(BsonClassMap classMap)
        {
            lock (s_mapTypes)
            {
                var type = classMap.ClassType;

                if (s_mapTypes.Add(type) == false)
                    return;

                if (s_preventNamespaces.Any(p => type.Namespace?.StartsWith(p, StringComparison.OrdinalIgnoreCase) ?? false))
                    return;
            }

            //Debug.WriteLine("***** Map convention discrimintator to " + classMap.ClassType);
            var classType = classMap.ClassType;

            var entityIdInterface = classType.GetTypeInfoExtension()
                                             .GetAllCompatibleTypes()
                                             .FirstOrDefault(f => f.IsGenericType && f.GetGenericTypeDefinition().IsAssignableTo(typeof(IEntityWithId<>)));

            if (entityIdInterface is not null)
            {
                var uidProp = classType.GetProperty(nameof(IEntityWithId<Guid>.Uid));

                if (uidProp is not null && uidProp.DeclaringType == classType)
                    classMap.MapIdProperty(uidProp.Name);
            }

            foreach (var prop in classType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                          .Where(p => p.DeclaringType == classType &&
                                                      p.CanRead))
            {
                if (prop.GetCustomAttribute<IgnoreDataMemberAttribute>() is not null || prop.GetCustomAttribute<BsonIgnoreAttribute>() is not null)
                {
                    classMap.UnmapProperty(prop.Name);
                    continue;
                }

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
                {
#pragma warning disable CS0168 // Variable is declared but never used
                    try
                    {
                        BsonClassMap.LookupClassMap(member.MemberType);
                    }
                    catch (Exception ex)
                    {
                    }
#pragma warning restore CS0168 // Variable is declared but never used
                }
            }

            classMap.SetIgnoreExtraElements(true);
            classMap.SetIgnoreExtraElementsIsInherited(true);

            //if (classType.IsInterface == false && classType.IsAbstract == false && classType.IsGenericType && !classType.IsGenericTypeDefinition)
            //{
            //    var disciminatorRoot = classType.Name;

            //    var parts = classType.GetGenericArguments()
            //                         .Select(x => (x, BsonClassMap.LookupClassMap(x)?.Discriminator))
            //                         .Select(kv => string.IsNullOrEmpty(kv.Discriminator) ? kv.x.Name : kv.Discriminator)
            //                         .ToArray();

            //    classMap.SetDiscriminator(disciminatorRoot + "<<" + string.Join(", ", parts) + ">>");
            //}
            //else
            //{

            if (!classType.IsInterface)
            {
                classMap.SetDiscriminator(classType.AssemblyQualifiedName);
                classMap.SetDiscriminatorIsRequired(true);
            }

            //}

            var knownTypes = classType.GetCustomAttributes<KnownTypeAttribute>(false);
            foreach (var knownType in knownTypes.Where(k => k.Type?.ContainsGenericParameters == false))
                classMap.AddKnownType(knownType.Type);
        }

        #endregion
    }
}
