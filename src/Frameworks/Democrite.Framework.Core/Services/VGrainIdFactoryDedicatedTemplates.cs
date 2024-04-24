// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Models;
    using Democrite.Framework.Core.Resources;
    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans.Runtime;

    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Factory implementation used to generate <see cref="GrainId"/> base on format <see cref="VGrainIdFormatAttribute"/>
    /// </summary>
    /// <seealso cref="IVGrainIdFactory" />
    public sealed class VGrainIdFactoryDedicatedTemplates : SafeDisposable, IVGrainIdDedicatedFactory
    {
        #region Fields

        private const string BEFORE = "before";
        private const string AFTER = "after";
        private const string RULE = "rule";
        private const string PROPS = "props";

        private static readonly Regex s_newRegex = new Regex("^(?<before>.*)(?<rule>{new})(?<after>.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex s_inputRegex = new Regex("^(?<before>.*)(?<rule>{input(?<props>[a-zA-Z.]+)*})(?<after>.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex s_executionContextRegex = new Regex("^(?<before>.*)(?<rule>{executionContext(?<props>[a-zA-Z.]+)*})(?<after>.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        private static readonly ConstantExpression s_expressionNullString;
        private static readonly PropertyInfo s_randomShared;

        private static readonly MethodInfo s_changeTypeMethod;
        private static readonly MethodInfo s_dynamicCall;
        private static readonly MethodInfo s_concatMthd;
        private static readonly MethodInfo s_guidParse;
        private static readonly MethodInfo s_longParse;
        private static readonly MethodInfo s_toString;
        private static readonly MethodInfo s_newGuid;
        private static readonly MethodInfo s_newLong;

        private readonly Dictionary<Type, (Func<object?, IExecutionContext?, object> primaryGenerator, Func<object?, IExecutionContext?, string?>? extensionGenerator)> _cachedBuilder;
        private readonly ReaderWriterLockSlim _locker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initialize the class <see cref="VGrainIdFactoryDedicatedTemplates"/>
        /// </summary>
        static VGrainIdFactoryDedicatedTemplates()
        {
            var expressionNullString = Expression.Constant(null, typeof(string));

            Debug.Assert(expressionNullString != null);
            s_expressionNullString = expressionNullString;

            var concatMthd = typeof(string).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                           .First(m => m.Name == nameof(string.Concat) &&
                                                       m.IsGenericMethod == false &&
                                                       m.GetParameters().Length == 1 &&
                                                       m.GetParameters().First().ParameterType == typeof(string[]) &&
                                                       m.GetParameters().First().GetCustomAttribute<ParamArrayAttribute>() != null);

            Debug.Assert(concatMthd != null);
            s_concatMthd = concatMthd;

            var newGuid = typeof(Guid).GetMethod(nameof(Guid.NewGuid), BindingFlags.Static | BindingFlags.Public)!;
            Debug.Assert(newGuid != null);
            s_newGuid = newGuid;

            var randomShared = typeof(Random).GetProperty(nameof(Random.Shared), BindingFlags.Static | BindingFlags.Public)!;
            Debug.Assert(randomShared != null);
            s_randomShared = randomShared;

            var newLong = typeof(Random).GetMethod(nameof(Random.NextDouble), BindingFlags.Instance | BindingFlags.Public)!;
            Debug.Assert(newLong != null);
            s_newLong = newLong;

            var toString = typeof(object).GetMethod(nameof(object.ToString), BindingFlags.Instance | BindingFlags.Public)!;
            Debug.Assert(toString != null);
            s_toString = toString;

            var guidParse = typeof(Guid).GetMethod(nameof(Guid.Parse),
                                                   BindingFlags.Static | BindingFlags.Public,
                                                   new Type[]
                                                   {
                                                        typeof(string)
                                                   })!;
            Debug.Assert(guidParse != null);
            s_guidParse = guidParse;

            var longParse = typeof(long).GetMethod(nameof(long.Parse),
                                                   BindingFlags.Static | BindingFlags.Public,
                                                   new Type[]
                                                   {
                                                        typeof(string)
                                                   })!;
            Debug.Assert(longParse != null);
            s_longParse = longParse;

            var dynamicCallExpectedCallArg = new[]
            {
                typeof(object),
                typeof(string),
                typeof(bool),
                typeof(bool)
            };

            var dynamicCall = typeof(DynamicCallHelper).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                       .First(m => m.Name == nameof(DynamicCallHelper.GetValueFrom) &&
                                                                   m.GetParameters().Select(p => p.ParameterType).SequenceEqual(dynamicCallExpectedCallArg));
            Debug.Assert(dynamicCall != null);
            s_dynamicCall = dynamicCall;

            var changeTypeMethod = typeof(Convert).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                  .FirstOrDefault(m => m.Name == nameof(Convert.ChangeType) &&
                                                                       m.GetParameters().Length == 2 &&
                                                                       m.GetParameters().First().ParameterType == typeof(object) &&
                                                                       m.GetParameters().Last().ParameterType == typeof(Type));

            Debug.Assert(changeTypeMethod != null);
            s_changeTypeMethod = changeTypeMethod;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdFactoryDedicatedTemplates"/> class.
        /// </summary>
        public VGrainIdFactoryDedicatedTemplates()
        {
            this._cachedBuilder = new Dictionary<Type, (Func<object?, IExecutionContext?, object> primaryGenerator, Func<object?, IExecutionContext?, string?>? extensionGenerator)>();
            this._locker = new ReaderWriterLockSlim();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool CanHandled(VGrainIdBaseFormatorAttribute attr)
        {
            return attr is VGrainIdFormatAttribute;
        }

        /// <inheritdoc />
        public IVGrainId BuildNewId(VGrainIdBaseFormatorAttribute attr,
                                    Type vgrainType,
                                    object? input,
                                    IExecutionContext? executionContext,
                                    ILogger? logger = null)
        {
            CheckAndThrowIfDisposed();

            logger ??= NullLogger.Instance;

            var formatAttribute = (VGrainIdFormatAttribute)attr;

            (Func<object?, IExecutionContext?, object> primaryGenerator, Func<object?, IExecutionContext?, string?>? extensionGenerator) builder = default;
            using (this._locker.LockRead())
            {
                if (this._cachedBuilder.TryGetValue(vgrainType, out var cachedBuilder))
                    builder = cachedBuilder;
            }

            if (builder.primaryGenerator == null)
            {
                CheckAndThrowIfDisposed();

                using (this._locker.LockWrite())
                {
                    if (this._cachedBuilder.TryGetValue(vgrainType, out var cachedBuilder))
                        builder = cachedBuilder;

                    if (builder.primaryGenerator == null)
                    {
                        bool useFallBack = false;

                        try
                        {
                            builder = GenerateBuilderFunctionBasedOnTemplate(vgrainType,
                                                                             formatAttribute,
                                                                             logger,
                                                                             input,
                                                                             executionContext,
                                                                             out useFallBack);
                        }
                        catch (DemocriteBaseException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            logger.OptiLog(LogLevel.Warning,
                                           DemocriteCoreLogSR.VGrainIdGenerationIssueTryFallback,
                                           vgrainType,
                                           ex);
                        }

                        if (builder.primaryGenerator != null && !useFallBack)
                            this._cachedBuilder.Add(vgrainType, builder);
                    }

                    CheckAndThrowIfDisposed();
                }
            }

            object? primary = null;
            string? extensionKey = null;
            try
            {
                primary = builder.primaryGenerator?.Invoke(input, executionContext);
                extensionKey = builder.extensionGenerator?.Invoke(input, executionContext);

                if (primary is null)
                    throw new VGrainIdGenerationException(vgrainType);
            }
            catch (Exception ex)
            {
                logger.OptiLog(LogLevel.Error, DemocriteCoreLogSR.VGrainIdGenerationIssue, vgrainType, ex);
                throw;
            }

            var vgrainId = new VGrainId(vgrainType, formatAttribute.FormatType, primary!, extensionKey);
            return vgrainId;
        }

        #region Tools

        /// <summary>
        /// Generate <see cref="GrainId"/> based on fallback information; null if not enought information are provided.
        /// </summary>
        private GrainId? GenerateFallback(Type vgrainType, VGrainIdFormatAttribute? formatAttribute)
        {
            if (formatAttribute is null)
                return null;

            if (string.IsNullOrEmpty(formatAttribute.FirstParameterFallback))
                throw new VGrainIdGenerationException(vgrainType, new ArgumentNullException(nameof(VGrainIdFormatAttribute) + "." + nameof(VGrainIdFormatAttribute.FirstParameterFallback)));

            var keyBuilder = new StringBuilder();

            keyBuilder.Append(formatAttribute.FirstParameterFallback);

            if (IsVGrainNeedMultiKeyValues(formatAttribute.FormatType))
            {
                if (string.IsNullOrEmpty(formatAttribute.SecondParameterFallback))
                    throw new VGrainIdGenerationException(vgrainType, new ArgumentNullException(nameof(VGrainIdFormatAttribute) + "." + nameof(VGrainIdFormatAttribute.SecondParameterFallback)));

                keyBuilder.Append(":");
                keyBuilder.Append(formatAttribute.SecondParameterFallback);
            }

            return GrainId.Create("", keyBuilder.ToString());
        }

        /// <summary>
        /// Generates the builder function based on template <see cref="VGrainIdFormatAttribute"/>.
        /// </summary>
        private (Func<object?, IExecutionContext?, object> primaryGenerator, Func<object?, IExecutionContext?, string?>? extensionGenerator) GenerateBuilderFunctionBasedOnTemplate(Type vgrainType,
                                                                                                                                                                                   VGrainIdFormatAttribute formatAttribute,
                                                                                                                                                                                   ILogger logger,
                                                                                                                                                                                   object? input,
                                                                                                                                                                                   IExecutionContext? executionContext,
                                                                                                                                                                                   out bool useFallback)
        {
            useFallback = false;

            var inputParam = Expression.Parameter(typeof(object), "input");
            var executionContextParam = Expression.Parameter(typeof(IExecutionContext), "executionContext");

            Expression? firstParamExpression;
            Expression? secondParamExpression = null;

            var keyType = formatAttribute.FormatType;
            var multiKey = IsVGrainNeedMultiKeyValues(keyType);

            firstParamExpression = SolvedTemplate(formatAttribute.FirstParameterTemplate,
                                                  formatAttribute.FirstParameterFallback,
                                                  expectGuid: keyType == IdFormatTypeEnum.Guid ||
                                                              keyType == IdFormatTypeEnum.CompositionGuidString,
                                                  expectLong: keyType == IdFormatTypeEnum.Long ||
                                                              keyType == IdFormatTypeEnum.CompositionLongString,
                                                  inputParam,
                                                  executionContextParam,
                                                  logger,
                                                  vgrainType,
                                                  out var useFallBackOnFirst);

            useFallback |= useFallBackOnFirst;

            if (multiKey)
            {
                secondParamExpression = SolvedTemplate(formatAttribute.SecondParameterTemplate,
                                                       formatAttribute.SecondParameterFallback,
                                                       expectGuid: false,
                                                       expectLong: false,
                                                       inputParam,
                                                       executionContextParam,
                                                       logger,
                                                       vgrainType,
                                                       out var useFallBackOnSecond);

                useFallback |= useFallBackOnSecond;

                secondParamExpression = Expression.Call(secondParamExpression, s_toString);
            }

            var primaryGenerator = Expression.Lambda<Func<object?, IExecutionContext?, object>>(Expression.Convert(firstParamExpression, typeof(object)), inputParam, executionContextParam);

            var extensionKeyGenerator = secondParamExpression is null
                                                 ? null
                                                 : Expression.Lambda<Func<object?, IExecutionContext?, string?>>(secondParamExpression, inputParam, executionContextParam).Compile();

            return (primaryGenerator.Compile(), extensionKeyGenerator);
        }

        /// <summary>
        /// Determines whether if the vgrain need a multi key values.
        /// </summary>
        private static bool IsVGrainNeedMultiKeyValues(IdFormatTypeEnum keyType)
        {
            return keyType == IdFormatTypeEnum.CompositionLongString ||
                   keyType == IdFormatTypeEnum.CompositionGuidString;
        }

        /// <summary>
        /// SOlve one paramter using template
        /// </summary>
        private Expression SolvedTemplate(string? template,
                                          string? fallback,
                                          bool expectGuid,
                                          bool expectLong,
                                          ParameterExpression input,
                                          ParameterExpression executionContext,
                                          ILogger logger,
                                          Type vgrainType,
                                          out bool useFallback)
        {
            useFallback = false;

            if (string.IsNullOrEmpty(template) && string.IsNullOrEmpty(fallback))
            {
                return SolvedTemplate("{new}",
                                      string.Empty,
                                      expectGuid,
                                      expectLong,
                                      input,
                                      executionContext,
                                      logger,
                                      vgrainType,
                                      out useFallback);
            }
            else if (string.IsNullOrEmpty(template) && !string.IsNullOrEmpty(fallback))
            {
                useFallback = true;
                return Expression.Constant(fallback, typeof(string));
            }

            var hasFallbackValue = !string.IsNullOrEmpty(fallback);

            try
            {
                Match? match = null;
                Expression? varGenExpression = null;
                var newMatch = s_newRegex.Match(template!);

                if (newMatch.Success)
                {
                    match = newMatch;
                    varGenExpression = expectGuid
                                          ? Expression.Call(null, s_newGuid)
                                          : (expectLong
                                                ? Expression.Call(Expression.Property(null, s_randomShared), s_newLong, Expression.Constant(0), Expression.Constant(long.MaxValue))
                                                : Expression.Call(Expression.Call(null, s_newGuid), s_toString));
                }

#pragma warning disable IDE0074 // Use compound assignment
                if (varGenExpression is null)
                {
                    varGenExpression = TryFetchValueFromExternalDynamicSource(s_inputRegex,
                                                                              input,
                                                                              template!,
                                                                              fallback,
                                                                              expectGuid,
                                                                              expectLong,
                                                                              hasFallbackValue,
                                                                              out match);
                }

                if (varGenExpression is null)
                {
                    varGenExpression = TryFetchValueFromExternalDynamicSource(s_executionContextRegex,
                                                                              executionContext,
                                                                              template!,
                                                                              fallback,
                                                                              expectGuid,
                                                                              expectLong,
                                                                              hasFallbackValue,
                                                                              out match);
                }
#pragma warning restore IDE0074 // Use compound assignment

                if (expectGuid && varGenExpression != null)
                    return Expression.Convert(varGenExpression, typeof(Guid));

                if (expectLong && varGenExpression != null)
                {
                    return Expression.Call(null,
                                           s_changeTypeMethod, 
                                           varGenExpression,
                                           Expression.Constant(typeof(long))); 
                }

                //return Expression.Convert(varGenExpression, typeof(long));

                if (match == null)
                    varGenExpression = Expression.Constant(template);

                var before = match?.Groups[BEFORE];
                var after = match?.Groups[AFTER];

                if ((before != null && before.Success && !string.IsNullOrEmpty(before.Value)) ||
                    (after != null && after.Success && !string.IsNullOrEmpty(after.Value)))
                {
                    var parameters = new List<Expression>();

                    if (before?.Success ?? false)
                        parameters.Add(Expression.Constant(before.Value, typeof(string)));

                    if (varGenExpression != null)
                        parameters.Add(varGenExpression);

                    if (after?.Success ?? false)
                        parameters.Add(Expression.Constant(after.Value, typeof(string)));

                    return Expression.Call(null, s_concatMthd, parameters.ToArray());
                }

                Debug.Assert(varGenExpression != null);

                return varGenExpression;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(fallback))
                {
                    logger.OptiLog(LogLevel.Warning, DemocriteCoreLogSR.VGrainIdGenerationIssueTryFallback, vgrainType, ex);
                    useFallback = true;
                    return Expression.Constant(fallback, typeof(string));
                }

                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static Expression? TryFetchValueFromExternalDynamicSource(Regex fetcherVariable,
                                                                          ParameterExpression parameterExpression,
                                                                          string template,
                                                                          string? fallback,
                                                                          bool expectGuid,
                                                                          bool expectLong,
                                                                          bool hasFallbackValue,
                                                                          out Match? resultMatch)
        {
            resultMatch = null;

            var match = fetcherVariable.Match(template!);
            if (match.Success)
            {
                resultMatch = match;

                var props = match.Groups[PROPS].Value.Trim('{', '}', '.');
                Expression varGenExpression;

                if (!string.IsNullOrWhiteSpace(props))
                {
                    varGenExpression = Expression.Call(null,
                                                       s_dynamicCall,
                                                       parameterExpression,
                                                       Expression.Constant(props),
                                                       Expression.Constant(false),
                                                       Expression.Constant(!hasFallbackValue));
                }
                else
                {
                    varGenExpression = parameterExpression;
                }

                if (hasFallbackValue)
                {
                    varGenExpression = Expression.Coalesce(varGenExpression, (expectGuid
                                                                                ? Expression.Call(s_guidParse, Expression.Constant(fallback))
                                                                                : (expectLong
                                                                                        ? Expression.Call(s_longParse, Expression.Constant(fallback))
                                                                                        : Expression.Constant(fallback))));
                }

                return varGenExpression;
            }

            return null;
        }

        /// <summary>
        /// Free cache lock resources
        /// </summary>
        protected override void DisposeEnd()
        {
            this._locker?.Dispose();
            base.DisposeEnd();
        }

        #endregion

        #endregion
    }
}
