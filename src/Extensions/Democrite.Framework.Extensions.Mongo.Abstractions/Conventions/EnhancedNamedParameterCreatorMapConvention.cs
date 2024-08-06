// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Abstractions.Conventions
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Conventions;

    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public sealed class EnhancedNamedParameterCreatorMapConvention : ConventionBase, ICreatorMapConvention
    {
        // public methods
        /// <summary>
        /// Applies a modification to the creator map.
        /// </summary>
        /// <param name="creatorMap">The creator map.</param>
        public void Apply(BsonCreatorMap creatorMap)
        {
            if (creatorMap.Arguments == null)
            {
                if (creatorMap.MemberInfo != null)
                {
                    var parameters = GetParameters(creatorMap.MemberInfo);
                    if (parameters != null)
                    {
                        var arguments = new List<MemberInfo>();

                        foreach (var parameter in parameters)
                        {
                            var argument = FindMatchingArgument(creatorMap.ClassMap.ClassType, parameter);
                            if (argument == null)
                                return;
                            arguments.Add(argument);
                        }

                        creatorMap.SetArguments(arguments);
                    }
                }
            }
        }

        // private methods
        private MemberInfo? FindMatchingArgument(Type classType, ParameterInfo parameter)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            MemberInfo argument;
            if ((argument = Match(classType, MemberTypes.Property, BindingFlags.Public, parameter)) != null)
                return argument;
            if ((argument = Match(classType, MemberTypes.Field, BindingFlags.Public, parameter)) != null)
                return argument;
            if ((argument = Match(classType, MemberTypes.Property, BindingFlags.NonPublic, parameter)) != null)
                return argument;
            if ((argument = Match(classType, MemberTypes.Field, BindingFlags.NonPublic, parameter)) != null)
                return argument;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            return null;
        }

        private Type GetMemberType(MemberInfo memberInfo)
        {
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                return fieldInfo.FieldType;

            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.PropertyType;

            // should never happen
            throw new BsonInternalException();
        }

        private IEnumerable<ParameterInfo>? GetParameters(MemberInfo memberInfo)
        {
            var constructorInfo = memberInfo as ConstructorInfo;
            if (constructorInfo != null)
                return constructorInfo.GetParameters();

            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null)
                return methodInfo.GetParameters();

            return null;
        }

        private MemberInfo? Match(Type classType, MemberTypes memberType, BindingFlags visibility, ParameterInfo parameter)
        {
            var classTypeInfo = classType.GetTypeInfo();
            var bindingAttr = BindingFlags.IgnoreCase | BindingFlags.Instance;
#pragma warning disable CS8604 // Possible null reference argument.
            var memberInfos = classTypeInfo.GetMember(parameter.Name, memberType, bindingAttr | visibility);
#pragma warning restore CS8604 // Possible null reference argument.
            if (memberInfos.Length == 1 &&
                (GetMemberType(memberInfos[0]).IsAssignableTo(parameter.ParameterType) ||
                 parameter.ParameterType.IsAssignableTo(GetMemberType(memberInfos[0]))))
            {
                return memberInfos[0];
            }
            return null;
        }
    }
}
