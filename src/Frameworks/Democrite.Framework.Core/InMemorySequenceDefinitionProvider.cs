// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;

    using System;

    /// <summary>
    /// <see cref="SequenceDefinition"/> provider register in memory
    /// </summary>
    /// <seealso cref="ISequenceDefinitionSourceProvider" />
    public sealed class InMemorySequenceDefinitionProvider : InMemoryBaseDefinitionProvider<SequenceDefinition>, ISequenceDefinitionSourceProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemorySequenceDefinitionProvider"/> class.
        /// </summary>
        public InMemorySequenceDefinitionProvider(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        #endregion
    }
}
