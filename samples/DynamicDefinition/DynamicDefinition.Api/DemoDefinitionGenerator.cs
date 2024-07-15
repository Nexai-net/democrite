namespace DynamicDefinition.Api
{
    using Democrite.Framework.Bag.DebugTools;
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;

    using DynamicDefinition.Api.VGrains;

    using Elvex.Toolbox.Models;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public sealed class DemoDefinitionGenerator
    {
        #region Fields

        private readonly IDynamicDefinitionHandler _dynamicDefinitionHandler;

        #endregion

        #region Ctor

        /// <summary>
        /// Initialize a new instance of the class <see cref="DemoDefinitionGenerator"/>
        /// </summary>
        public DemoDefinitionGenerator(IDynamicDefinitionHandler dynamicDefinitionHandler)
        {
            this._dynamicDefinitionHandler = dynamicDefinitionHandler;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generates a new sequence and push it on the dynamic definitin provider
        /// </summary>
        public async Task<Guid> GenerateAndPushNewAsync(string template, GenerationModeEnum mode)
        {
            ArgumentException.ThrowIfNullOrEmpty(template);

            var parts = template.OptiSplit(StringIncludeSeparatorMode.None,
                                           StringComparison.OrdinalIgnoreCase,
                                           StringSplitOptions.RemoveEmptyEntries,
                                           " ",
                                           Environment.NewLine).ToList();

            switch (mode)
            {
                case GenerationModeEnum.Revert:
                    parts.Reverse();
                    break;

                case GenerationModeEnum.Ascending:
                    parts = parts.OrderBy(p => p.Length).ToList();
                    break;

                case GenerationModeEnum.Descending:
                    parts = parts.OrderByDescending(p => p.Length).ToList();
                    break;

                case GenerationModeEnum.Random:
                    parts = parts.OrderByDescending(p => Random.Shared.Next(0, 42)).ToList();
                    break;

                default:
                case GenerationModeEnum.Normal:
                    break;

            }

            var seqDefinition = GenerateSequence(parts, string.Join("", parts));

            var result = await this._dynamicDefinitionHandler.PushDefinitionAsync(seqDefinition, true, null!, default);
            if (result is false)
                throw new InvalidOperationException($"Could not push new definition {seqDefinition.Uid}:{seqDefinition.DisplayName}");

            return seqDefinition.Uid;
        }

        /// <summary>
        /// Generates the trigger and push new asynchronous.
        /// </summary>
        internal async Task<Guid> GenerateTriggerAndPushNewAsync(string cron, Guid sequence)
        {
            var trigger = Trigger.Cron(cron, "CronSeq" + sequence)
                                 .AddTargetSequence(sequence)
                                 .Build();

            var result = await this._dynamicDefinitionHandler.PushDefinitionAsync(trigger, true, null!, default);
            if (result is false)
                throw new InvalidOperationException($"Could not push new definition {trigger.Uid}:{trigger.DisplayName}");

            return trigger.Uid;
        }

        #region Tools

        /// <summary>
        /// Generates the sequence.
        /// </summary>
        private SequenceDefinition GenerateSequence(List<string> parts, string displayName)
        {
            var sequenceBuilder = Sequence.Build(displayName)
                                          .NoInput()
                                          .Select("");

            Debug.Assert(parts.Count >= 1);

            foreach (var part in parts)
                sequenceBuilder = sequenceBuilder.Use<ITextActionVGrain>().Configure(part).Call((g, i, ctx) => g.Concat(i, ctx)).Return;

            sequenceBuilder = sequenceBuilder.Use<IDisplayInfoVGrain>().Call((g, i, ctx) => g.DisplayCallInfoAsync(i, ctx)).Return;

            return sequenceBuilder.Build();
        }

        #endregion

        #endregion
    }
}
