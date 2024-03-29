﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Keep : Democrite.Framework.Core.Abstractions.Customizations
namespace Democrite.Framework.Core.Abstractions.Customizations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ExecutionCustomizationDescriptionsExtensions
    {
        /// <summary>
        /// Merges <paramref name="local"/> customization from <paramref name="global"/> information
        /// </summary>
        public static ExecutionCustomizationDescriptions? Merge(this ExecutionCustomizationDescriptions? global, in ExecutionCustomizationDescriptions? local, Func<StageVGrainRedirectionDescription, Guid?>? stageKeyProvider = null)
        {
            if (global is null && local is null)
                return null;

            if (global is null && stageKeyProvider is null)
                return local;

            if (local is null && stageKeyProvider is null)
                return global;

            var globalVGrainRedirection = global?.VGrainRedirection ?? EnumerableHelper<StageVGrainRedirectionDescription>.ReadOnly;
            var localVGrainRedirection = local?.VGrainRedirection ?? EnumerableHelper<StageVGrainRedirectionDescription>.ReadOnly;

            var indexed = localVGrainRedirection.GroupBy(k => (stageKeyProvider is null ? k.StageUid : stageKeyProvider(k)) ?? Guid.Empty)
                                                .ToDictionary(k => k.Key, kv => kv.Select(v => v.RedirectionDefinition).ToList()) ?? new Dictionary<Guid, List<VGrainRedirectionDefinition>>();

            // Try insert global values if it doesn't conflict with a local value
            foreach (var redirection in globalVGrainRedirection)
            {
                var stageKey = (stageKeyProvider is not null ? stageKeyProvider(redirection) : redirection.StageUid) ?? Guid.Empty;

                if (!indexed.TryGetValue(stageKey, out var existings))
                {
                    existings = new List<VGrainRedirectionDefinition>();
                    indexed.Add(stageKey, existings);
                }

                if (existings.Any(e => e.Conflict(redirection.RedirectionDefinition)))
                {
                    // If conflict pass
                    continue;
                }

                existings.Add(redirection.RedirectionDefinition);
            }

            return new ExecutionCustomizationDescriptions(indexed.SelectMany(kv => kv.Value.Select(v => new StageVGrainRedirectionDescription((kv.Key == Guid.Empty) ? null : kv.Key, v))).ToReadOnly());
        }
    }
}
