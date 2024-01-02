// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Steps
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Toolbox;
    using Democrite.Framework.Toolbox.Models;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Build a call step, it correspond to the vgrain entry point
    /// </summary>
    /// <seealso cref="ISequencePipelineInternalStageStep" />
    public sealed class CallStepBuilder : StepBaseBuilder
    {
        #region Fields

        private static readonly Type s_ctxTraitType = typeof(IExecutionContext);

        private readonly MethodInfo _mthd;
        private readonly Type _vgrainType;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CallStepBuilder"/> class.
        /// </summary>
        private CallStepBuilder(Type? input, Type vgrainType, MethodInfo mthd, Type? output)
            : base(input, output)
        {
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
                                                                              object? configuration,
                                                                              Type configurationType)
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

            var ctxArgs = new List<Type>()
            {
                s_ctxTraitType
            };

            var possibleInputs = new List<Type>()
            {
                s_ctxTraitType
            };

            if (input != null && input != NoneType.Trait)
                possibleInputs.Add(input);

            if (configurationType != null && configurationType != NoneType.Trait)
            {
                var ctxInfo = typeof(IExecutionContext<>).MakeGenericType(configurationType);
                possibleInputs.Add(ctxInfo);
                ctxArgs.Add(ctxInfo);
            }

            var parameters = mthd.GetParameters();

            foreach (var param in parameters)
            {
                CheckParameterType(param, possibleInputs);
                possibleInputs.Remove(param.ParameterType);
            }

            if (possibleInputs.Intersect(ctxArgs).Count() == ctxArgs.Count)
                throw new InvalidCastException(mthd + " MUST take at least IExcutionContext in argument");

            if (configurationType != null)
            {
                var validators = mthd.GetCustomAttributes()
                                     .OfType<IExecutionContextConfigurationValidator>()
                                     .ToArray();

                if (validators.Length > 0)
                {
                    foreach (var validator in validators)
                        validator.Validate(configuration, mthd);
                }
            }

            return new CallStepBuilder(input, vgrainTrait, mthd, output);
        }

        /// <summary>
        /// Checks the type of the parameter.
        /// </summary>
        /// <exception cref="System.InvalidCastException">Parameter (" + parameter.ParameterType + ") name '" + parameter.Name + "' must be of type " + string.Join(" or ", possibleInputs)</exception>
        private static void CheckParameterType(ParameterInfo parameter, IReadOnlyCollection<Type> possibleInputs)
        {
            if (parameter.ParameterType == NoneType.Trait)
                return;

            if (!possibleInputs.Contains(parameter.ParameterType))
            {
                throw new InvalidCastException("Parameter (" + parameter.ParameterType + ") name '" + parameter.Name + "' must be of type " + string.Join(" or ", possibleInputs));
            }
        }

        /// <summary>
        /// Converts to definition.
        /// </summary>
        public override SequenceStageBaseDefinition ToDefinition<TContext>(SequenceOptionStageDefinition? option,
                                                                           bool preventReturn,
                                                                           TContext? contextInfo = default)
                        where TContext : default
        {
            var def = this._mthd.GetAbstractMethod();

            return new SequenceStageCallDefinition(this.Input?.GetAbstractType(),
                                                   this._vgrainType?.GetAbstractType() as ConcreteType ?? throw new InvalidDataException("VGrain interface must not be null"),
                                                   def,
                                                   this.Output?.GetAbstractType(),
                                                   EqualityComparer<TContext>.Default.Equals(contextInfo, default) 
                                                        ? NoneType.AbstractTrait
                                                        : typeof(TContext).GetAbstractType(),
                                                   contextInfo,

                                                   options: option,
                                                   preventReturn,
                                                   option?.StageId);
        }

        #endregion
    }
}
