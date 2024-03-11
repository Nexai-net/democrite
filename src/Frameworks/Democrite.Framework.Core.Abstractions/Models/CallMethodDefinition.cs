// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Models
{
    using Elvex.Toolbox.Helpers;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Definition used to identify a method and how to call it.
    /// </summary>
    /// <seealso cref="IEquatable{ModelDefinition}" />
    /// <seealso cref="IEquatable{MethodInfo}" />
    [Serializable]
    [Immutable]
    [DataObject]
    [ImmutableObject(true)]
    public sealed class CallMethodDefinition : IEquatable<CallMethodDefinition>, IEquatable<MethodInfo>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CallMethodDefinition"/> class.
        /// </summary>
        public CallMethodDefinition(string name,
                                    Type returnType,
                                    IEnumerable<Type> arguments,
                                    Type? declaringType,
                                    IEnumerable<Type>? genericImplementationTypes)
        {
            this.Name = name;
            this.ReturnType = returnType;
            this.Arguments = arguments?.ToArray() ?? EnumerableHelper<Type>.ReadOnlyArray;
            this.DeclaringType = declaringType;
            this.GenericImplementationTypes = genericImplementationTypes?.ToArray() ?? EnumerableHelper<Type>.ReadOnlyArray;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the method name.
        /// </summary>
        [DataMember]
        public string Name { get; }

        /// <summary>
        /// Gets the type of method's return.
        /// </summary>
        /// <remarks>
        ///     Could be generic params
        /// </remarks>
        [DataMember]
        public Type ReturnType { get; }

        /// <summary>
        /// Gets method's arguments types.
        /// </summary>
        [DataMember]
        public IReadOnlyCollection<Type> Arguments { get; }

        /// <summary>
        /// Gets the type of declaring the method.
        /// </summary>
        [DataMember]
        public Type? DeclaringType { get; }

        /// <summary>
        /// Gets collection of generic implementation values
        /// </summary>
        [DataMember]
        public Type[]? GenericImplementationTypes { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(CallMethodDefinition? other)
        {
            if (other is null)
                return false;

            return this.Name == other.Name &&
                   this.ReturnType == other.ReturnType &&
                   this.DeclaringType == other.DeclaringType &&
                   this.Arguments.SequenceEqual(other.Arguments) &&
                   ((this.GenericImplementationTypes == null && other.GenericImplementationTypes == null) ||
                    (this.GenericImplementationTypes?.SequenceEqual(other.GenericImplementationTypes ?? EnumerableHelper<Type>.ReadOnlyArray) ?? false));
        }

        /// <inheritdoc />
        public bool Equals(MethodInfo? other)
        {
            if (other is null)
                return false;

            return this.Name == other.Name &&
                   this.ReturnType == other.ReturnType &&
                   this.DeclaringType == other.DeclaringType &&
                   this.Arguments.SequenceEqual(other.GetParameters().Select(p => p.ParameterType));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^
                   this.ReturnType.GetHashCode() ^
                   (this.DeclaringType?.GetHashCode() ?? 0) ^
                   this.Arguments.Aggregate(0, (acc, t) => t.GetHashCode() ^ acc) ^
                   (this.GenericImplementationTypes?.Aggregate(0, (acc, t) => t.GetHashCode() ^ acc) ?? 0);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is CallMethodDefinition def)
                return Equals(def);

            if (obj is MethodInfo info)
                return Equals(info);

            return false;
        }

        #endregion
    }
}
