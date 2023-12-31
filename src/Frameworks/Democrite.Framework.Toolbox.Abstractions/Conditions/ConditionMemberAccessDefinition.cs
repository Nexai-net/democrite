﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Conditions
{
    using System;
    using System.ComponentModel;

    [Serializable]
    [ImmutableObject(true)]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public sealed class ConditionMemberAccessDefinition : ConditionBaseDefinition
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionMemberAccessDefinition"/> class.
        /// </summary>
        public ConditionMemberAccessDefinition(ConditionBaseDefinition? instance, string memberName)
        {
            this.Instance = instance;
            this.MemberName = memberName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public ConditionBaseDefinition? Instance { get; }

        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        public string MemberName { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(ConditionMemberAccessDefinition x, ConditionMemberAccessDefinition y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(ConditionMemberAccessDefinition x, ConditionMemberAccessDefinition y)
        {
            return x?.Equals(y) ?? y is null;
        }

        /// <inheritdoc />
        protected override bool OnEquals(ConditionBaseDefinition other)
        {
            return other is ConditionMemberAccessDefinition member &&
                   (this.Instance?.Equals(member.Instance) ?? member.Instance is null) &&
                   this.MemberName == member.MemberName;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return (this.Instance?.GetHashCode() ?? 0) ^ (this.MemberName?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
