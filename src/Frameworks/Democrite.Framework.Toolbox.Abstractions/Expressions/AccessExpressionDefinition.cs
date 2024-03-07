// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Expressions
{
    using Democrite.Framework.Toolbox.Abstractions.Models;
    using Democrite.Framework.Toolbox.Abstractions.Supports;
    using Democrite.Framework.Toolbox.Models;
    using Newtonsoft.Json;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Converts object properties to their runtime type with type information.
    /// Ensure <c>TypeNameHandling</c> is set to <c>TypeNameHandling.All</c>
    /// </summary>
    public class RuntimeTypeConverter : JsonConverter
    {
        #region Overrides of JsonConverter

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader);
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object);
        }

        #endregion
    }

    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class AccessExpressionDefinition : ISupportDebugDisplayName
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessExpressionDefinition"/> class.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="directObject">The direct object.</param>
        /// <param name="chainCall">The chain call.</param>
        /// <param name="memberInit">The member initialize.</param>
        public AccessExpressionDefinition(ConcretBaseType targetType,
                                          TypedArgument? directObject,
                                          string? chainCall,
                                          MemberInitializationDefinition? memberInit)
        {
            this.TargetType = targetType;
            this.DirectObject = directObject;
            this.ChainCall = chainCall;
            this.MemberInit = memberInit;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        [DataMember]
        public ConcretBaseType TargetType { get; }

        /// <summary>
        /// Gets the direct object.
        /// </summary>
        [DataMember]
        public TypedArgument? DirectObject { get; }

        /// <summary>
        /// Gets the chain call.
        /// </summary>
        [DataMember]
        public string? ChainCall { get; }

        /// <summary>
        /// Gets the member initialize.
        /// </summary>
        [DataMember]
        public MemberInitializationDefinition? MemberInit { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            if (this.MemberInit is not null)
                return $"Access {this.TargetType.DisplayName} " + this.MemberInit;

            if (string.IsNullOrEmpty(this.ChainCall))
                return $"Access {this.TargetType.DisplayName} ChainCall : " + this.ChainCall;

            return $"Access {this.TargetType.DisplayName} Direct : " + this.DirectObject;

        }
        #endregion
    }
}
