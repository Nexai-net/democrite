// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Conditions
{
    using Democrite.Framework.Toolbox.Models;

    using Newtonsoft.Json;

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Store convert operation
    /// </summary>
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public sealed class ConditionConvertDefinition : ConditionBaseDefinition
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    {
        #region Fields

        public const string TypeDiscriminator = "convert";

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionValueDefinition"/> class.
        /// </summary>
        public ConditionConvertDefinition(ConditionBaseDefinition from, AbstractType to)
        {
            ArgumentNullException.ThrowIfNull(from);
            ArgumentNullException.ThrowIfNull(to);

            this.From = from;
            this.To = to;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets source of the convertion
        /// </summary>
        [DataMember(IsRequired = true)]
        public ConditionBaseDefinition From { get; }

        /// <summary>
        /// Gets target type of the convertion
        /// </summary>
        [DataMember(IsRequired = true)]
        public AbstractType To { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(ConditionConvertDefinition x, ConditionConvertDefinition y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(ConditionConvertDefinition x, ConditionConvertDefinition y)
        {
            return x?.Equals(y) ?? y is null;
        }

        /// <inheritdoc />
        protected override bool OnEquals(ConditionBaseDefinition other)
        {
            return other is ConditionConvertDefinition val &&
                   this.From.Equals(val.From) &&
                   this.To.Equals(val.To);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.From, this.To);
        }

        /// <summary>
        /// Builds the comparer.
        /// </summary>
        private static IEqualityComparer BuildComparer(Type type)
        {
            // OPTIMIZE : cache comparer by type
            return (IEqualityComparer)(typeof(EqualityComparer<>).MakeGenericType(type)
                                                                 .GetProperty(nameof(EqualityComparer<int>.Default), BindingFlags.Public | BindingFlags.Static)!
                                                                 .GetValue(null))!;
        }
        
        #endregion
    }
}
