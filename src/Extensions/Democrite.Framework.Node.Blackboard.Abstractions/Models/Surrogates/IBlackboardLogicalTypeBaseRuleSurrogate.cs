// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Surrogates
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;

    using System.Collections.Concurrent;
    using System.Data;
    using System.Reflection;

    /// <summary>
    /// Base interface of rule surrogates
    /// </summary>
    public interface IBlackboardLogicalTypeBaseRuleSurrogate
    {
        /// <inheritdoc cref="BlackboardLogicalTypeBaseRule.LogicalTypePattern"/>
        string LogicalTypePattern { get; }
    }

    public static class BlackboardLogicalTypeBaseRuleSurrogateConverter
    {
        #region Fields

        private static readonly IReadOnlyDictionary<Predicate<BlackboardLogicalTypeBaseRule>, Func<BlackboardLogicalTypeBaseRule, IBlackboardLogicalTypeBaseRuleSurrogate>> s_convertToSurrogate;
        private static readonly IReadOnlyDictionary<Predicate<IBlackboardLogicalTypeBaseRuleSurrogate>, Func<IBlackboardLogicalTypeBaseRuleSurrogate, BlackboardLogicalTypeBaseRule>> s_convertFromSurrogate;

        private static readonly MethodInfo s_convertWithGenericFromSurrogateImpl;
        private static readonly MethodInfo s_convertWithGenericToSurrogateImpl;

        // First Key root converter and second key is the specialized cache
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, MethodInfo>> s_genericConverterAccess;
        private static readonly ConcurrentDictionary<Type, object> s_genericConverter;

        private static readonly BlackboardRemainOnSealedLogicalTypeRuleConverter s_remainCnv;
        private static readonly BlackboardMaxRecordLogicalTypeRuleConverter s_maxRecordCnv;
        private static readonly BlackboardTypeCheckLogicalTypeRuleConverter s_typeCheckCnv;
        private static readonly BlackboardStorageLogicalTypeRuleConverter s_storageCnv;
        private static readonly BlackboardLogicalTypeUniqueRuleConverter s_uniqueCnv;
        private static readonly BlackboardOrderLogicalTypeRuleConverter s_orderCnv;
        private static readonly BlackboardRegexLogicalTypeRuleConverter s_regexCnv;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardLogicalTypeBaseRuleSurrogateConverter"/> class.
        /// </summary>
        static BlackboardLogicalTypeBaseRuleSurrogateConverter()
        {
            s_convertWithGenericFromSurrogateImpl = typeof(BlackboardLogicalTypeBaseRuleSurrogateConverter).GetMethod(nameof(ConvertWithGenericFromSurrogateImpl), BindingFlags.Static | BindingFlags.NonPublic)!;
            s_convertWithGenericToSurrogateImpl = typeof(BlackboardLogicalTypeBaseRuleSurrogateConverter).GetMethod(nameof(ConvertWithGenericToRuleImpl), BindingFlags.Static | BindingFlags.NonPublic)!;

            s_genericConverterAccess = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, MethodInfo>>();
            s_genericConverter = new ConcurrentDictionary<Type, object>();

            s_maxRecordCnv = new BlackboardMaxRecordLogicalTypeRuleConverter();
            s_regexCnv = new BlackboardRegexLogicalTypeRuleConverter();
            s_typeCheckCnv = new BlackboardTypeCheckLogicalTypeRuleConverter();

            s_orderCnv = new BlackboardOrderLogicalTypeRuleConverter();
            s_storageCnv = new BlackboardStorageLogicalTypeRuleConverter();
            s_remainCnv = new BlackboardRemainOnSealedLogicalTypeRuleConverter();
            s_uniqueCnv = new BlackboardLogicalTypeUniqueRuleConverter();

            s_convertToSurrogate = new Dictionary<Predicate<BlackboardLogicalTypeBaseRule>, Func<BlackboardLogicalTypeBaseRule, IBlackboardLogicalTypeBaseRuleSurrogate>>()
            {
                { r => r is BlackboardMaxRecordLogicalTypeRule, (r) => s_maxRecordCnv.ConvertToSurrogate((BlackboardMaxRecordLogicalTypeRule)r) },
                { r => r is BlackboardRegexLogicalTypeRule, (r) => s_regexCnv.ConvertToSurrogate((BlackboardRegexLogicalTypeRule)r) },
                { r => r is BlackboardTypeCheckLogicalTypeRule, (r) => s_typeCheckCnv.ConvertToSurrogate((BlackboardTypeCheckLogicalTypeRule)r) },
                { r => IsNumberRule(r), r => ConvertWithGenericToRule<BlackboardNumberRangeLogicalTypeRuleConverter<int>>(r) },

                { r => r is BlackboardOrderLogicalTypeRule, (r) => s_orderCnv.ConvertToSurrogate((BlackboardOrderLogicalTypeRule)r) },
                { r => r is BlackboardStorageLogicalTypeRule, (r) => s_storageCnv.ConvertToSurrogate((BlackboardStorageLogicalTypeRule)r) },
                { r => r is BlackboardRemainOnSealedLogicalTypeRule, (r) => s_remainCnv.ConvertToSurrogate((BlackboardRemainOnSealedLogicalTypeRule)r) },
                { r => r is BlackboardLogicalTypeUniqueRule, (r) => s_uniqueCnv.ConvertToSurrogate((BlackboardLogicalTypeUniqueRule)r) },
            };

            s_convertFromSurrogate = new Dictionary<Predicate<IBlackboardLogicalTypeBaseRuleSurrogate>, Func<IBlackboardLogicalTypeBaseRuleSurrogate, BlackboardLogicalTypeBaseRule>>()
            {
                { r => r is BlackboardMaxRecordLogicalTypeRuleSurrogate, (r) => s_maxRecordCnv.ConvertFromSurrogate((BlackboardMaxRecordLogicalTypeRuleSurrogate)r) },
                { r => r is BlackboardRegexLogicalTypeRuleSurrogate, (r) => s_regexCnv.ConvertFromSurrogate((BlackboardRegexLogicalTypeRuleSurrogate)r) },
                { r => r is BlackboardTypeCheckLogicalTypeRuleSurrogate, (r) => s_typeCheckCnv.ConvertFromSurrogate((BlackboardTypeCheckLogicalTypeRuleSurrogate)r) },
                { r => IsNumberRuleSurrogate(r), r => ConvertWithGenericFromSurrogate<BlackboardNumberRangeLogicalTypeRuleConverter<int>>(r) },

                { r => r is BlackboardOrderLogicalTypeRuleSurrogate, (r) => s_orderCnv.ConvertFromSurrogate((BlackboardOrderLogicalTypeRuleSurrogate)r) },
                { r => r is BlackboardStorageLogicalTypeRuleSurrogate, (r) => s_storageCnv.ConvertFromSurrogate((BlackboardStorageLogicalTypeRuleSurrogate)r) },
                { r => r is BlackboardRemainOnSealedLogicalTypeRuleSurrogate, (r) => s_remainCnv.ConvertFromSurrogate((BlackboardRemainOnSealedLogicalTypeRuleSurrogate)r) },
                { r => r is BlackboardLogicalTypeUniqueRuleSurrogate, (r) => s_uniqueCnv.ConvertFromSurrogate((BlackboardLogicalTypeUniqueRuleSurrogate)r) },
            };
        }

        #endregion

        #region Methods

        /// <inheritdoc cref="IConverter{TValue, TSurrogate}.ConvertToSurrogate(in TValue)"/>
        public static IBlackboardLogicalTypeBaseRuleSurrogate ConvertToSurrogate(BlackboardLogicalTypeBaseRule rule)
        {
            return s_convertToSurrogate.First(kv => kv.Key.Invoke(rule)).Value(rule);
        }

        /// <inheritdoc cref="IConverter{TValue, TSurrogate}.ConvertToSurrogate(in TValue)"/>
        public static IEnumerable<IBlackboardLogicalTypeBaseRuleSurrogate> ConvertToSurrogate(in IEnumerable<BlackboardLogicalTypeBaseRule> rules)
        {
            return rules.Select(r => ConvertToSurrogate(r));
        }

        /// <inheritdoc cref="IConverter{TValue, TSurrogate}.ConvertFromSurrogate(in TSurrogate)"/>
        public static BlackboardLogicalTypeBaseRule ConvertFromSurrogate(IBlackboardLogicalTypeBaseRuleSurrogate rule)
        {
            return s_convertFromSurrogate.First(kv => kv.Key.Invoke(rule)).Value(rule);
        }

        /// <inheritdoc cref="IConverter{TValue, TSurrogate}.ConvertFromSurrogate(in TSurrogate)"/>
        public static IEnumerable<BlackboardLogicalTypeBaseRule> ConvertFromSurrogate(in IEnumerable<IBlackboardLogicalTypeBaseRuleSurrogate> rules)
        {
            return rules.Select(r => ConvertFromSurrogate(r));
        }

        #region Tools

        private static bool IsNumberRule(BlackboardLogicalTypeBaseRule r)
        {
            var trait = r.GetType();
            return trait.IsGenericType && trait.GetGenericTypeDefinition() == typeof(BlackboardNumberRangeLogicalTypeRule<>);
        }

        private static bool IsNumberRuleSurrogate(IBlackboardLogicalTypeBaseRuleSurrogate r)
        {
            var trait = r.GetType();
            return trait.IsGenericType && trait.GetGenericTypeDefinition() == typeof(BlackboardNumberRangeLogicalTypeRuleSurrogate<>);
        }

        /// <summary>
        /// Converts from <see cref="BlackboardLogicalTypeBaseRule"/> to <see cref="IBlackboardLogicalTypeBaseRuleSurrogate"/> with one generic parameter
        /// </summary>
        private static IBlackboardLogicalTypeBaseRuleSurrogate ConvertWithGenericToRule<TConverter>(BlackboardLogicalTypeBaseRule rule)
        {
            var ruleTrait = rule.GetType();
            var method = GetConvertGenericMethod<TConverter>(ruleTrait, s_convertWithGenericToSurrogateImpl);
            return (IBlackboardLogicalTypeBaseRuleSurrogate)method.Invoke(null, new object[] { rule })!;
        }

        /// <summary>
        /// Converts from <see cref="IBlackboardLogicalTypeBaseRuleSurrogate"/ to <see cref="BlackboardLogicalTypeBaseRule"/> with one generic parameter
        /// </summary>
        private static BlackboardLogicalTypeBaseRule ConvertWithGenericFromSurrogate<TConverter>(IBlackboardLogicalTypeBaseRuleSurrogate surrogate)
        {
            var ruleTrait = surrogate.GetType();

            var method = GetConvertGenericMethod<TConverter>(ruleTrait, s_convertWithGenericFromSurrogateImpl);
            return (BlackboardLogicalTypeBaseRule)method.Invoke(null, new object[] { surrogate })!;
        }

        /// <summary>
        /// Gets the convert generic method.
        /// </summary>
        private static MethodInfo GetConvertGenericMethod<TConverter>(Type ruleTrait, MethodInfo template)
        {
            var dedicatedStorage = s_genericConverterAccess.GetOrAdd(ruleTrait, t => new ConcurrentDictionary<Type, MethodInfo>());

            var method = dedicatedStorage.GetOrAdd(ruleTrait.GetGenericArguments().Single(),
                                                   (t) =>
                                                   {
                                                       var converterType = typeof(TConverter).GetGenericTypeDefinition().MakeGenericType(t);
                                                       var iconvType = converterType.GetTypeInfoExtension()
                                                                                    .GetAllCompatibleTypes()
                                                                                    .First(s => s.IsInterface &&
                                                                                                s.IsGenericType &&
                                                                                                s.GetGenericTypeDefinition() == typeof(IConverter<,>));

                                                       var sourceType = iconvType.GetGenericArguments().First();
                                                       var surrogateType = iconvType.GetGenericArguments().Last();

                                                       var mthd = template.MakeGenericMethod(converterType, sourceType, surrogateType);
                                                       return mthd;
                                                   });
            return method;
        }

        /// <summary>
        /// Converts from <see cref="IBlackboardLogicalTypeBaseRuleSurrogate"/> to <see cref="BlackboardLogicalTypeBaseRule"/>
        /// </summary>
        private static TSource ConvertWithGenericFromSurrogateImpl<TConverter, TSource, TSurrogate>(TSurrogate surrogate)
            where TSurrogate : struct
            where TConverter : IConverter<TSource, TSurrogate>, new()
        {
            var cnv = (TConverter)s_genericConverter.GetOrAdd(typeof(TConverter), (t) => new TConverter());
            return cnv.ConvertFromSurrogate(surrogate);
        }

        /// <summary>
        /// Converts from <see cref="BlackboardLogicalTypeBaseRule"/> to <see cref="IBlackboardLogicalTypeBaseRuleSurrogate"/>
        /// </summary>
        private static TSurrogate ConvertWithGenericToRuleImpl<TConverter, TSource, TSurrogate>(TSource rule)
            where TSurrogate : struct
            where TConverter : IConverter<TSource, TSurrogate>, new()
        {
            var cnv = (TConverter)s_genericConverter.GetOrAdd(typeof(TConverter), (t) => new TConverter());
            return cnv.ConvertToSurrogate(rule);
        }

        #endregion

        #endregion
    }
}
