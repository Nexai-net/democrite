// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Surrogates
{
    using Elvex.Toolbox.Abstractions.Conditions;
    using Elvex.Toolbox.Abstractions.Enums;
    using Elvex.Toolbox.Models;

    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /*
     * 
     * [KnownType(typeof(ConditionParameterDefinition))]
     * [KnownType(typeof(ConditionCallDefinition))]
     * [KnownType(typeof(ConditionGroupDefinition))]
     * [KnownType(typeof(ConditionMathOperationDefinition))]
     * [KnownType(typeof(ConditionMemberAccessDefinition))]
     * [KnownType(typeof(ConditionOperandDefinition))]
     * [KnownType(typeof(ConditionValueDefinition))]
     * [KnownType(typeof(ConditionConvertDefinition))]
     */

    [JsonDerivedType(typeof(ConditionParameterDefinitionSurrogate), "Param")]
    [JsonDerivedType(typeof(ConditionCallDefinitionSurrogate), "Call")]
    [JsonDerivedType(typeof(ConditionGroupDefinitionSurrogate), "Group")]
    [JsonDerivedType(typeof(ConditionMathOperationDefinitionSurrogate), "MathOperation")]
    [JsonDerivedType(typeof(ConditionMemberAccessDefinitionSurrogate), "Member")]
    [JsonDerivedType(typeof(ConditionOperandDefinitionSurrogate), "Operand")]
    [JsonDerivedType(typeof(ConditionValueDefinitionSurrogate), "Value")]
    [JsonDerivedType(typeof(ConditionConvertDefinitionSurrogate), "Converter")]
    public interface IConditionDefinitionPart
    {
    }

    /* ConditionExpressionDefinition */

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct ConditionExpressionDefinitionSurrogate(IReadOnlyCollection<ConditionParameterDefinitionSurrogate> Parameters,
                                                                IConditionDefinitionPart Condition) : IConditionDefinitionPart;

    [RegisterConverter]
    public sealed class ConditionExpressionDefinitionConverter : IConverter<ConditionExpressionDefinition, ConditionExpressionDefinitionSurrogate>
    {
        /// <inheritdoc />
        public ConditionExpressionDefinition ConvertFromSurrogate(in ConditionExpressionDefinitionSurrogate surrogate)
        {
            return new ConditionExpressionDefinition(surrogate.Parameters.Select(p => ConditionPartConverterHelper.FromSurrogate<ConditionParameterDefinition>(p)!).ToReadOnly() ?? EnumerableHelper<ConditionParameterDefinition>.ReadOnly,
                                                     ConditionPartConverterHelper.FromSurrogate<ConditionBaseDefinition>(surrogate.Condition)!);
        }

        /// <inheritdoc />
        public ConditionExpressionDefinitionSurrogate ConvertToSurrogate(in ConditionExpressionDefinition value)
        {
            return new ConditionExpressionDefinitionSurrogate(value.Parameters?.Select(p => ConditionPartConverterHelper.ToSurrogate<ConditionParameterDefinitionSurrogate>(p)).ToReadOnly() ?? EnumerableHelper<ConditionParameterDefinitionSurrogate>.ReadOnly,
                                                              ConditionPartConverterHelper.ToSurrogate<IConditionDefinitionPart>(value.Condition)!);
        }
    }

    /* ConditionParameterDefinition */
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct ConditionParameterDefinitionSurrogate(Guid Uid,
                                                               string Name,
                                                               IConcretTypeSurrogate Type,
                                                               ushort Order) : IConditionDefinitionPart;

    [RegisterConverter]
    public sealed class ConditionParameterDefinitionConverter : IConverter<ConditionParameterDefinition, ConditionParameterDefinitionSurrogate>
    {
        /// <inheritdoc />
        public ConditionParameterDefinition ConvertFromSurrogate(in ConditionParameterDefinitionSurrogate surrogate)
        {
            return new ConditionParameterDefinition(surrogate.Uid,
                                                    surrogate.Name,
                                                    ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate.Type),
                                                    surrogate.Order);
        }

        /// <inheritdoc />
        public ConditionParameterDefinitionSurrogate ConvertToSurrogate(in ConditionParameterDefinition value)
        {
            return new ConditionParameterDefinitionSurrogate(value.Uid,
                                                             value.Name,
                                                             ConcretBaseTypeConverter.ConvertToSurrogate((ConcretBaseType)value.Type),
                                                             value.Order);
        }
    }

    /* ConditionCallDefinition */

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct ConditionCallDefinitionSurrogate(IConditionDefinitionPart? Instance,
                                                          string MethodName,
                                                          IReadOnlyCollection<IConditionDefinitionPart> Arguments) : IConditionDefinitionPart;

    [RegisterConverter]
    public sealed class ConditionCallDefinitionConverter : IConverter<ConditionCallDefinition, ConditionCallDefinitionSurrogate>
    {
        /// <inheritdoc />
        public ConditionCallDefinition ConvertFromSurrogate(in ConditionCallDefinitionSurrogate surrogate)
        {
            return new ConditionCallDefinition(ConditionPartConverterHelper.FromSurrogate<ConditionBaseDefinition>(surrogate.Instance),
                                               surrogate.MethodName,
                                               surrogate.Arguments?.Select(a => ConditionPartConverterHelper.FromSurrogate<ConditionBaseDefinition>(a)!).ToArray() ?? EnumerableHelper<ConditionBaseDefinition>.ReadOnlyArray);
        }

        /// <inheritdoc />
        public ConditionCallDefinitionSurrogate ConvertToSurrogate(in ConditionCallDefinition value)
        {
            return new ConditionCallDefinitionSurrogate(ConditionPartConverterHelper.ToSurrogate<IConditionDefinitionPart>(value.Instance),
                                                        value.MethodName,
                                                        value.Arguments?.Select(a => ConditionPartConverterHelper.ToSurrogate<IConditionDefinitionPart>(a)!).ToArray() ?? EnumerableHelper<IConditionDefinitionPart>.ReadOnlyArray);
        }
    }

    /* ConditionGroupDefinition */
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct ConditionGroupDefinitionSurrogate(LogicEnum Logic,
                                                           IReadOnlyCollection<IConditionDefinitionPart> Conditions) : IConditionDefinitionPart;

    [RegisterConverter]
    public sealed class ConditionGroupDefinitionConverter : IConverter<ConditionGroupDefinition, ConditionGroupDefinitionSurrogate>
    {
        /// <inheritdoc />
        public ConditionGroupDefinition ConvertFromSurrogate(in ConditionGroupDefinitionSurrogate surrogate)
        {
            return new ConditionGroupDefinition(surrogate.Logic,
                                                surrogate.Conditions?.Select(a => ConditionPartConverterHelper.FromSurrogate<ConditionBaseDefinition>(a)!).ToArray() ?? EnumerableHelper<ConditionBaseDefinition>.ReadOnlyArray);
        }

        /// <inheritdoc />
        public ConditionGroupDefinitionSurrogate ConvertToSurrogate(in ConditionGroupDefinition value)
        {
            return new ConditionGroupDefinitionSurrogate(value.Logic,
                                                         value.Conditions?.Select(a => ConditionPartConverterHelper.ToSurrogate<IConditionDefinitionPart>(a)!).ToArray() ?? EnumerableHelper<IConditionDefinitionPart>.ReadOnlyArray);
        }
    }

    /* ConditionMathOperationDefinition */
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct ConditionMathOperationDefinitionSurrogate(IConditionDefinitionPart? Left,
                                                                   MathOperatorEnum MathOperator,
                                                                   IConditionDefinitionPart Right) : IConditionDefinitionPart;

    [RegisterConverter]
    public sealed class ConditionMathOperationDefinitionConverter : IConverter<ConditionMathOperationDefinition, ConditionMathOperationDefinitionSurrogate>
    {
        /// <inheritdoc />
        public ConditionMathOperationDefinition ConvertFromSurrogate(in ConditionMathOperationDefinitionSurrogate surrogate)
        {
            return new ConditionMathOperationDefinition(ConditionPartConverterHelper.FromSurrogate<ConditionBaseDefinition>(surrogate.Left),
                                                        surrogate.MathOperator,
                                                        ConditionPartConverterHelper.FromSurrogate<ConditionBaseDefinition>(surrogate.Right)!);
        }

        /// <inheritdoc />
        public ConditionMathOperationDefinitionSurrogate ConvertToSurrogate(in ConditionMathOperationDefinition value)
        {
            return new ConditionMathOperationDefinitionSurrogate(ConditionPartConverterHelper.ToSurrogate<IConditionDefinitionPart>(value.Left),
                                                                 value.MathOperator,
                                                                 ConditionPartConverterHelper.ToSurrogate<IConditionDefinitionPart>(value.Right)!);
        }
    }

    /* ConditionMemberAccessDefinition */
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct ConditionMemberAccessDefinitionSurrogate(IConditionDefinitionPart? Instance,
                                                                  string MemberName) : IConditionDefinitionPart;

    [RegisterConverter]
    public sealed class ConditionMemberAccessDefinitionConverter : IConverter<ConditionMemberAccessDefinition, ConditionMemberAccessDefinitionSurrogate>
    {
        /// <inheritdoc />
        public ConditionMemberAccessDefinition ConvertFromSurrogate(in ConditionMemberAccessDefinitionSurrogate surrogate)
        {
            return new ConditionMemberAccessDefinition(ConditionPartConverterHelper.FromSurrogate<ConditionBaseDefinition>(surrogate.Instance),
                                                       surrogate.MemberName);
        }

        /// <inheritdoc />
        public ConditionMemberAccessDefinitionSurrogate ConvertToSurrogate(in ConditionMemberAccessDefinition value)
        {
            return new ConditionMemberAccessDefinitionSurrogate(ConditionPartConverterHelper.ToSurrogate<IConditionDefinitionPart>(value.Instance),
                                                                value.MemberName);
        }
    }

    /* ConditionOperandDefinition */
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct ConditionOperandDefinitionSurrogate(IConditionDefinitionPart? Left,
                                                             OperandEnum Operand,
                                                             IConditionDefinitionPart Right) : IConditionDefinitionPart;

    [RegisterConverter]
    public sealed class ConditionOperandDefinitionConverter : IConverter<ConditionOperandDefinition, ConditionOperandDefinitionSurrogate>
    {
        /// <inheritdoc />
        public ConditionOperandDefinition ConvertFromSurrogate(in ConditionOperandDefinitionSurrogate surrogate)
        {
            return new ConditionOperandDefinition(ConditionPartConverterHelper.FromSurrogate<ConditionBaseDefinition>(surrogate.Left),
                                                  surrogate.Operand,
                                                  ConditionPartConverterHelper.FromSurrogate<ConditionBaseDefinition>(surrogate.Right)!);
        }

        /// <inheritdoc />
        public ConditionOperandDefinitionSurrogate ConvertToSurrogate(in ConditionOperandDefinition value)
        {
            return new ConditionOperandDefinitionSurrogate(ConditionPartConverterHelper.ToSurrogate<IConditionDefinitionPart>(value.Left),
                                                           value.Operand,
                                                           ConditionPartConverterHelper.ToSurrogate<IConditionDefinitionPart>(value.Right)!);
        }
    }

    /* ConditionValueDefinition */
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct ConditionValueDefinitionSurrogate(IConcretTypeSurrogate Type,
                                                           object? Value) : IConditionDefinitionPart;

    [RegisterConverter]
    public sealed class ConditionValueDefinitionConverter : IConverter<ConditionValueDefinition, ConditionValueDefinitionSurrogate>
    {
        /// <inheritdoc />
        public ConditionValueDefinition ConvertFromSurrogate(in ConditionValueDefinitionSurrogate surrogate)
        {
            return new ConditionValueDefinition(ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate.Type),
                                                surrogate.Value);
        }

        /// <inheritdoc />
        public ConditionValueDefinitionSurrogate ConvertToSurrogate(in ConditionValueDefinition value)
        {
            return new ConditionValueDefinitionSurrogate(ConcretBaseTypeConverter.ConvertToSurrogate((ConcretBaseType)value.Type),
                                                         value.Value);
        }
    }

    /* ConditionConvertDefinition */
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct ConditionConvertDefinitionSurrogate(IConditionDefinitionPart From,
                                                             IConcretTypeSurrogate To,
                                                             bool StrictCast) : IConditionDefinitionPart;

    [RegisterConverter]
    public sealed class ConditionConvertDefinitionConverter : IConverter<ConditionConvertDefinition, ConditionConvertDefinitionSurrogate>
    {
        /// <inheritdoc />
        public ConditionConvertDefinition ConvertFromSurrogate(in ConditionConvertDefinitionSurrogate surrogate)
        {
            return new ConditionConvertDefinition(ConditionPartConverterHelper.FromSurrogate<ConditionBaseDefinition>(surrogate.From)!,
                                                  ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate.To),
                                                  surrogate.StrictCast);
        }

        /// <inheritdoc />
        public ConditionConvertDefinitionSurrogate ConvertToSurrogate(in ConditionConvertDefinition value)
        {
            return new ConditionConvertDefinitionSurrogate(ConditionPartConverterHelper.ToSurrogate<IConditionDefinitionPart>(value.From)!,
                                                           ConcretBaseTypeConverter.ConvertToSurrogate((ConcretBaseType)value.To),
                                                           value.StrictCast);
        }
    }

    /* Generic */
    public static class ConditionPartConverterHelper
    {
        #region Fields

        private static readonly FrozenDictionary<Type, Func<IConditionDefinitionPart, object>> s_fromSurrogate;
        private static readonly FrozenDictionary<Type, Func<object, IConditionDefinitionPart>> s_toSurrogate;

        private static readonly ConditionExpressionDefinitionConverter s_expressionConverter;
        private static readonly ConditionParameterDefinitionConverter s_parameterConverter;
        private static readonly ConditionCallDefinitionConverter s_callConverter;
        private static readonly ConditionGroupDefinitionConverter s_groupConverter;
        private static readonly ConditionMathOperationDefinitionConverter s_mathOperatorConverter;
        private static readonly ConditionMemberAccessDefinitionConverter s_memberAccessConverter;
        private static readonly ConditionOperandDefinitionConverter s_operandConverter;
        private static readonly ConditionValueDefinitionConverter s_valueConverter;
        private static readonly ConditionConvertDefinitionConverter s_convertConverter;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionPartConverterHelper"/> class.
        /// </summary>
        static ConditionPartConverterHelper()
        {
            s_expressionConverter = new ConditionExpressionDefinitionConverter();
            s_parameterConverter = new ConditionParameterDefinitionConverter();
            s_callConverter = new ConditionCallDefinitionConverter();
            s_groupConverter = new ConditionGroupDefinitionConverter();
            s_mathOperatorConverter = new ConditionMathOperationDefinitionConverter();
            s_memberAccessConverter = new ConditionMemberAccessDefinitionConverter();
            s_operandConverter = new ConditionOperandDefinitionConverter();
            s_valueConverter = new ConditionValueDefinitionConverter();
            s_convertConverter = new ConditionConvertDefinitionConverter();

            s_fromSurrogate = new Dictionary<Type, Func<IConditionDefinitionPart, object>>()
            {
                { typeof(ConditionExpressionDefinitionSurrogate), (o) => s_expressionConverter.ConvertFromSurrogate((ConditionExpressionDefinitionSurrogate)o) },
                { typeof(ConditionParameterDefinitionSurrogate), (o) => s_parameterConverter.ConvertFromSurrogate((ConditionParameterDefinitionSurrogate)o) },
                { typeof(ConditionCallDefinitionSurrogate), (o) => s_callConverter.ConvertFromSurrogate((ConditionCallDefinitionSurrogate)o) },
                { typeof(ConditionGroupDefinitionSurrogate), (o) => s_groupConverter.ConvertFromSurrogate((ConditionGroupDefinitionSurrogate)o) },
                { typeof(ConditionMathOperationDefinitionSurrogate), (o) => s_mathOperatorConverter.ConvertFromSurrogate((ConditionMathOperationDefinitionSurrogate)o) },
                { typeof(ConditionMemberAccessDefinitionSurrogate), (o) => s_memberAccessConverter.ConvertFromSurrogate((ConditionMemberAccessDefinitionSurrogate)o) },
                { typeof(ConditionOperandDefinitionSurrogate), (o) => s_operandConverter.ConvertFromSurrogate((ConditionOperandDefinitionSurrogate)o) },
                { typeof(ConditionValueDefinitionSurrogate), (o) => s_valueConverter.ConvertFromSurrogate((ConditionValueDefinitionSurrogate)o) },
                { typeof(ConditionConvertDefinitionSurrogate), (o) => s_convertConverter.ConvertFromSurrogate((ConditionConvertDefinitionSurrogate)o) },

            }.ToFrozenDictionary();

            s_toSurrogate = new Dictionary<Type, Func<object, IConditionDefinitionPart>>()
            {
                { typeof(ConditionExpressionDefinition), (o) => s_expressionConverter.ConvertToSurrogate((ConditionExpressionDefinition)o) },
                { typeof(ConditionParameterDefinition), (o) => s_parameterConverter.ConvertToSurrogate((ConditionParameterDefinition)o) },
                { typeof(ConditionCallDefinition), (o) => s_callConverter.ConvertToSurrogate((ConditionCallDefinition)o) },
                { typeof(ConditionGroupDefinition), (o) => s_groupConverter.ConvertToSurrogate((ConditionGroupDefinition)o) },
                { typeof(ConditionMathOperationDefinition), (o) => s_mathOperatorConverter.ConvertToSurrogate((ConditionMathOperationDefinition)o) },
                { typeof(ConditionMemberAccessDefinition), (o) => s_memberAccessConverter.ConvertToSurrogate((ConditionMemberAccessDefinition)o) },
                { typeof(ConditionOperandDefinition), (o) => s_operandConverter.ConvertToSurrogate((ConditionOperandDefinition)o) },
                { typeof(ConditionValueDefinition), (o) => s_valueConverter.ConvertToSurrogate((ConditionValueDefinition)o) },
                { typeof(ConditionConvertDefinition), (o) => s_convertConverter.ConvertToSurrogate((ConditionConvertDefinition)o) },
            }.ToFrozenDictionary();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts to surrogate.
        /// </summary>
        public static TSurrogate? ToSurrogate<TSurrogate>(object? definition)
            where TSurrogate : IConditionDefinitionPart
        {
            if (definition is not null && s_toSurrogate.TryGetValue(definition.GetType(), out var convertor))
                return (TSurrogate)convertor(definition);
            return default;
        }

        /// <summary>
        /// Froms the surrogate.
        /// </summary>
        public static TDef? FromSurrogate<TDef>(in IConditionDefinitionPart? surrogate)
        {
            if (surrogate is not null && s_fromSurrogate.TryGetValue(surrogate.GetType(), out var convertor))
                return (TDef)convertor(surrogate);
            return default;
        }

        #endregion
    }
}
