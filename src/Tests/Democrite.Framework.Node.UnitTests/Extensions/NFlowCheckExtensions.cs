// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.UnitTests.Extensions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Core.Diagnostics;
    using Democrite.UnitTests.ToolKit.Sequences;

    using Elvex.Toolbox;

    using NFluent;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Stage validator
    /// </summary>
    /// <param name="stepIndx">The step indx information; nested are display by a new version level.</param>
    /// <param name="stageExecutionInfo">The stage execution information.</param>
    /// <param name="stageDefintion">The stage defintion.</param>
    /// <returns></returns>
    public delegate void StageValidator(Version stepIndx, FlowStage stageExecutionInfo, SequenceStageDefinition stageDefintion);

    /// <summary>
    /// Stage validator
    /// </summary>
    /// <param name="stepIndx">The step indx information; nested are display by a new version level.</param>
    /// <param name="stageExecutionInfo">The stage execution information.</param>
    /// <param name="stageDefintion">The stage defintion.</param>
    /// <returns></returns>
    internal delegate void AutoStageValidator(Version stepIndx,
                                              FlowStage stageExecutionInfo,
                                              SequenceStageDefinition stageDefintion,
                                              int?[] level,
                                              int currentLevelIndex,
                                              IReadOnlyDictionary<Guid, StageValidator>? stageValidator,
                                              HashSet<FlowStage> flowStages);

    /// <summary>
    /// Extensions about <see cref="NFluent"/> to check <see cref="FlowValidator"/>
    /// </summary>
    public static class NFlowCheckExtensions
    {
        #region Fields

        private static readonly IReadOnlyDictionary<Type, AutoStageValidator> s_autoValidation;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="NFlowCheckExtensions"/> class.
        /// </summary>
        static NFlowCheckExtensions()
        {
            s_autoValidation = new Dictionary<Type, AutoStageValidator>()
            {
                [typeof(SequenceStageCallDefinition)] = AutoCallSequenceStageStepDefinition,
                [typeof(SequenceStageForeachDefinition)] = AutoForeachSequenceStageStepDefinition,
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check sequence based on <see cref="ISequenceStageDefinition"/>
        /// </summary>
        public static void FollowDefinition(this ICheck<FlowValidator> validator,
                                            SequenceDefinition definition,
                                            IReadOnlyDictionary<Guid, StageValidator>? stageValidator = null)
        {
            validator.IsNotNull();

            var flow = (validator as IWithValue<FlowValidator>)?.Value;

            Check.That(flow).IsNotNull();

            var flowStageConsumed = new HashSet<FlowStage>();

            ArgumentNullException.ThrowIfNull(flow);

            Check.That(flow.Roots).IsNotNull().And.CountIs(1);

            var workfExec = flow.Roots.Single();

            Check.That(workfExec.Call).IsNotNull().And
                                      .WhichMember(p => p!.TargetVGrainType).Verifies(p => p.IsNotNull().And.IsEqualTo(typeof(IGenericContextedExecutor<Guid>).AssemblyQualifiedName));

            if (definition.Output is not null)
            {
                var outputTask = string.Format("] {0}`1[{1}", typeof(Task).FullName, definition.Output?.FullDisplayName);

                Check.That(workfExec.Call).IsNotNull();
                Check.That(workfExec.Call!.TargetMethod).IsNotNull();
                Check.ThatCode(() => workfExec.Call!.TargetMethod!.Contains(outputTask)).IsNotNull();
            }

            Check.That(flowStageConsumed.Add(workfExec));

            StageProcessCheck(definition.Stages.FirstOrDefault()?.Uid,
                              definition,
                              workfExec.Children?.SingleOrDefault(),
                              new int?[5],
                              0,
                              stageValidator,
                              flowStageConsumed);
        }

        /// <summary>
        /// Check stage by stage with logs
        /// </summary>
        private static void StageProcessCheck(Guid? currentStageId,
                                              SequenceDefinition definition,
                                              FlowStage? currentFlow,
                                              int?[] level,
                                              int currentLevelIndex,
                                              IReadOnlyDictionary<Guid, StageValidator>? stageValidator,
                                              HashSet<FlowStage> flowStagesConsumed)
        {

            if (currentStageId == null)
            {
                Check.That(currentFlow).IsNull();
                return;
            }

            if (currentFlow != null &&
                currentFlow.AllLogs.Count == 1 &&
                currentFlow.AllLogs.OfType<ExecutionContextChangeDiagnosticLog>().Any() &&
                currentFlow.Children != null &&
                currentFlow.Children.Count == 1)
            {
                Check.That(flowStagesConsumed.Add(currentFlow));

                StageProcessCheck(currentStageId,
                                  definition,
                                  currentFlow.Children.First(),
                                  level,
                                  currentLevelIndex,
                                  stageValidator,
                                  flowStagesConsumed);
                return;
            }

            var stageDef = definition[currentStageId];

            Check.That(stageDef).IsNotNull();
            Check.That(currentFlow).IsNotNull();

            level[currentLevelIndex] = (level[currentLevelIndex] ?? 0) + 1;

            Array.Clear(level, currentLevelIndex + 1, level.Length - currentLevelIndex - 1);

            var currentVersion = Version.Parse(string.Join(".", level.TakeWhile(l => l != null)) + ".0");

            AutoStageValidation(currentVersion, currentFlow, stageDef!, level, currentLevelIndex, stageValidator, flowStagesConsumed);

            if (stageValidator != null && stageValidator.TryGetValue(stageDef!.Uid, out var validator))
                Check.ThatCode(() => validator(currentVersion, currentFlow!, stageDef)).DoesNotThrow();

            var nextStage = definition.GetNextStage(currentStageId);

            if (nextStage == null)
            {
                var allnodes = currentFlow.GetTreeValues(c => c?.Children).ToArray();
                Check.That(allnodes).ContainsOnlyElementsThatMatch(f => flowStagesConsumed.Contains(f!));

                return;
            }

            foreach (var child in (currentFlow?.Children) ?? EnumerableHelper<FlowStage>.ReadOnly)
            {
                StageProcessCheck(child.DefinitionCursorId,
                                  definition,
                                  child,
                                  level,
                                  currentLevelIndex,
                                  stageValidator,
                                  flowStagesConsumed);
            }
        }

        /// <summary>
        /// Automatics the stage validation when no custom is provided
        /// </summary>
        private static void AutoStageValidation(Version currentVersion,
                                                FlowStage? currentFlow,
                                                SequenceStageDefinition stageDef,
                                                int?[] level,
                                                int currentLevelIndex,
                                                IReadOnlyDictionary<Guid, StageValidator>? stageValidator,
                                                HashSet<FlowStage> flowStages)
        {
            Check.That(stageDef).IsNotNull();

            var monoStep = stageDef;

            Check.That(monoStep).IsNotNull();

            if (s_autoValidation.TryGetValue(monoStep.GetType(), out var validator))
            {
                validator(currentVersion, currentFlow!, stageDef, level, currentLevelIndex, stageValidator, flowStages);
                return;
            }

            throw new NotImplementedException("Auto validation not implemented");
        }

        /// <summary>
        /// Automatics validtion about call sequence stage step definition.
        /// </summary>
        private static void AutoCallSequenceStageStepDefinition(Version stepIndx,
                                                                FlowStage stageExecutionInfo,
                                                                SequenceStageDefinition stageDefintion,
                                                                int?[] level,
                                                                int currentLevelIndex,
                                                                IReadOnlyDictionary<Guid, StageValidator>? stageValidator,
                                                                HashSet<FlowStage> flowStagesConsumed)
        {
            var callDef = (SequenceStageCallDefinition)stageDefintion;

            Check.That(callDef).IsNotNull();

            Check.That(stageExecutionInfo.Call).IsNotNull();
            Check.That(stageExecutionInfo.Call?.TargetVGrainType).IsNotNull().And.IsEqualTo(callDef.VGrainType.AssemblyQualifiedName);
            Check.That(stageExecutionInfo.Call?.TargetMethod).IsNotNull().And.Contains(callDef.CallMethodDefinition.DisplayName);

            if (callDef.Output is null && callDef.Output != NoneType.Trait)
            {
                Check.That(stageExecutionInfo.ReturnArg).IsNotNull();
                Check.That(stageExecutionInfo.ReturnArg?.Error).IsNullOrEmpty();
                Check.That(stageExecutionInfo.ReturnArg?.InOut).IsNotNull();

                var results = stageExecutionInfo.ReturnArg!.InOut!.Flattern();
                Check.That(results).IsNotNull().And.CountIs(1).And.ContainsOnlyInstanceOfType(callDef.Output!.ToType());
            }

            if (callDef.Input is null && callDef.Input != NoneType.Trait)
            {
                Check.That(stageExecutionInfo.Parameters).IsNotNull();
                Check.That(stageExecutionInfo.Parameters?.Error).IsNullOrEmpty();
                Check.That(stageExecutionInfo.Parameters?.InOut).IsNotNull();

                var inputs = stageExecutionInfo.Parameters!.InOut!.Flattern();
                Check.That(inputs).IsNotNull().And.ContainsNoDuplicateItem().And.ContainsOnlyElementsThatMatch(o => o == null ||
                                                                                                                    o.GetType().IsAssignableTo(callDef.Input?.ToType()) ||
                                                                                                                    o is IExecutionContext);
            }

            Check.That(flowStagesConsumed.Add(stageExecutionInfo));
        }

        /// <summary>
        /// Automatics validtion about call sequence stage step definition.
        /// </summary>
        private static void AutoForeachSequenceStageStepDefinition(Version stepIndx,
                                              FlowStage stageExecutionInfo,
                                              SequenceStageDefinition stageDefintion,
                                              int?[] level,
                                              int currentLevelIndex,
                                              IReadOnlyDictionary<Guid, StageValidator>? stageValidator,
                                              HashSet<FlowStage> flowStagesConsumed)
        {
            var foreachDef = (SequenceStageForeachDefinition)stageDefintion;

            var innerFlowDef = foreachDef.InnerFlow;

            Check.That(innerFlowDef).IsNotNull();

            foreach (var child in stageExecutionInfo.Children)
            {
                StageProcessCheck(innerFlowDef.Stages.First().Uid, innerFlowDef, child, level, currentLevelIndex + 1, stageValidator, flowStagesConsumed);
            }

            Check.That(flowStagesConsumed.Add(stageExecutionInfo));
        }

        #endregion
    }
}
