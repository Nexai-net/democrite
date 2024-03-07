// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Expressions
{
    using Democrite.Framework.Toolbox.Abstractions.Models;
    using Democrite.Framework.Toolbox.Models;

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    /// <summary>
    /// Member binding based on nested init value
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    [DebuggerDisplay("Ini: {ToDebugDisplayName()}")]
    public sealed class MemberInputNestedInitBindingDefinition : MemberBindingDefinition
    {
        #region Ctor

        /// <summary>
        /// Initialize a new instance of the class <see cref="MemberInputNestedInitBindingDefinition"/>
        /// </summary>
        public MemberInputNestedInitBindingDefinition(bool isCtorParameter,
                                                     string memberName,
                                                     AbstractType newType,
                                                     AbstractMethod? ctor,
                                                     IEnumerable<MemberBindingDefinition> bindings)
            : base(isCtorParameter, memberName)
        {
            this.Ctor = ctor;
            this.Bindings = bindings?.ToArray() ?? Array.Empty<MemberBindingDefinition>();
            this.NewType = newType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ctor.
        /// </summary>
        [DataMember]
        public AbstractMethod? Ctor { get; }

        /// <summary>
        /// Gets the bindings.
        /// </summary>
        [DataMember]
        public IReadOnlyCollection<MemberBindingDefinition> Bindings { get; }

        /// <summary>
        /// Creates new type.
        /// </summary>
        [DataMember]
        public AbstractType NewType { get; }

        #endregion

        #region Methods

        /// <inheritdoc cref="IEquatable{T}.Equals(T?)" />
        protected override bool OnEquals(MemberBindingDefinition other)
        {
            return other is MemberInputNestedInitBindingDefinition nested &&
                   (nested.Ctor?.Equals(this.Ctor) ?? this.Ctor is null) &&
                   nested.NewType.Equals(this.NewType) &&
                   nested.Bindings.SequenceEqual(this.Bindings);
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.Ctor,
                                    this.NewType,
                                    this.Bindings.Aggregate(0, (acc, b) => acc ^ b.GetHashCode()));
        }

        /// <summary>
        /// Converts to debugdisplayname.
        /// </summary>
        public override string ToDebugDisplayName()
        {
            return $"{base.MemberName} = {this.NewType} [ {string.Join(',', this.Bindings.Select(b => b.ToDebugDisplayName()))} ]";
        }

        #endregion
    }
}
