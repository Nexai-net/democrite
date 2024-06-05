// Copyright (c) Nexai.
// This file is licenses to you under the MIT license.
// Produce by nexai, elvexoft & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Helpers
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Models;

    using System;

    /// <summary>
    /// Helper used to easy manipulate the <see cref="ExecutionContext"/>
    /// </summary>
    public static class ExecutionContextHelper
    {
        /// <summary>
        /// Creates a new <see cref="IExecutionContext{TConfiguration}"/> from <paramref name="config"/>
        /// </summary>
        public static IExecutionContext<TConfig> CreateNew<TConfig>(TConfig config, Guid? flowUid = null, Guid? parentUid = null)
        {
            return new ExecutionContextWithConfiguration<TConfig>(flowUid ?? Guid.NewGuid(), Guid.NewGuid(), parentUid, config);
        }
    }
}
