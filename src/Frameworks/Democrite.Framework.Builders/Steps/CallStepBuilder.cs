﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Steps
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Expressions;
    using Elvex.Toolbox.Models;

    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Get a call step, it correspond to the vgrain entry point
    /// </summary>
    /// <seealso cref="ISequencePipelineInternalStageStep" />
    public sealed class CallStepBuilder : StepBaseBuilder
    {
        #region Fields

        private static readonly Type s_ctxTraitType = typeof(IExecutionContext);

        private readonly SequenceStageCallParameterDefinition[] _parameterAccessExpressions;
        private readonly MethodInfo _mthd;
        private readonly Type _vgrainType;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CallStepBuilder"/> class.
        /// </summary>
        private CallStepBuilder(Type? input, SequenceStageCallParameterDefinition[] parameterAccessExpressions, Type vgrainType, MethodInfo mthd, Type? output)
            : base(input, output)
        {
            this._parameterAccessExpressions = parameterAccessExpressions;
            this._vgrainType = vgrainType;
            this._mthd = mthd;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Analyze the <paramref name="expression"/> to build a <see cref="CallStepBuilder"/>
        /// </summary>
        public static CallStepBuilder FromExpression<TSequenceVGrain, TOutput>(Expression expression,
                                                                               Type? input,
                                                                               Type configurationType,
                                                                               AccessExpressionDefinition? configuration)
             where TSequenceVGrain : IVGrain
        {
            var vgrainTrait = typeof(TSequenceVGrain);

            ArgumentNullException.ThrowIfNull(expression);

            var lambda = expression as LambdaExpression;
            ArgumentNullException.ThrowIfNull(lambda);

            var methodCall = lambda.Body as MethodCallExpression;
            ArgumentNullException.ThrowIfNull(methodCall);

            var mthd = methodCall.Method;

            var output = typeof(TOutput);
            if (output == NoneType.Trait)
                output = null;

            if ((mthd.DeclaringType?.IsAssignableFrom(vgrainTrait) ?? false) == false)
                throw new InvalidCastException(mthd.Name + " must be callable from " + vgrainTrait.FullName + " not from " + mthd.DeclaringType?.FullName);

            if (!typeof(Task).IsAssignableFrom(mthd.ReturnParameter.ParameterType))
                throw new InvalidCastException(expression + " must return a task");

            if (typeof(TOutput) != NoneType.Trait && !typeof(Task<TOutput>).IsAssignableFrom(mthd.ReturnParameter.ParameterType))
                throw new InvalidCastException(expression + " must return a Task<" + typeof(TOutput) + ">");

            var parameters = mthd.GetParameters();
            var lambdaParameters = parameters.Where(p => !p.ParameterType.IsAssignableTo(typeof(IVGrain)))
                                             .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                                             .ToArray();

            var parameterAccessExpressions = parameters.Where(p => !p.ParameterType.IsAssignableTo(typeof(IVGrain)) && !p.ParameterType.IsAssignableTo(s_ctxTraitType))
                                                       .Select(param =>
                                                       {
                                                           var setParamExpression = methodCall.Arguments[param.Position];
                                                           var access = ExpressionExtensions.CreateAccess(Expression.Lambda(setParamExpression, lambdaParameters));
                                                           return new SequenceStageCallParameterDefinition(param.Position, param.Name ?? string.Empty, access);
                                                       })
                                                       .ToArray();

            if (!parameters.Any(p => p.ParameterType.IsAssignableTo(s_ctxTraitType)))
                throw new InvalidCastException(mthd + " MUST take at least IExcutionContext in argument");

            if (configurationType != null && configuration != null && configuration.DirectObject is not null)
            {
                var validators = mthd.GetCustomAttributes()
                                     .OfType<IExecutionContextConfigurationValidator>()
                                     .ToArray();

                if (validators.Length > 0)
                {
                    var cfg = configuration.DirectObject.GetValue();
                    foreach (var validator in validators)
                        validator.Validate(cfg, mthd);
                }
            }

            return new CallStepBuilder(input, parameterAccessExpressions, vgrainTrait, mthd, output);
        }

        /// <summary>
        /// Converts to definition.
        /// </summary>
        public override SequenceStageBaseDefinition ToDefinition<TContext>(SequenceOptionStageDefinition? option,
                                                                           bool preventReturn,
                                                                           AccessExpressionDefinition? configurationAccess,
                                                                           ConcretType? configurationFromContextDataType)
            where TContext : default
        {
            var def = this._mthd.GetAbstractMethod();

            return new SequenceStageCallDefinition(this.Input?.GetAbstractType(),
                                                   this._vgrainType?.GetAbstractType() as ConcretType ?? throw new InvalidDataException("VGrain interface must not be null"),
                                                   def,
                                                   this.Output?.GetAbstractType(),
                                                   configurationAccess,
                                                   configurationFromContextDataType,
                                                   this._parameterAccessExpressions,
                                                   options: option,
                                                   preventReturn,
                                                   option?.StageId);
        }

        #endregion
    }
}
