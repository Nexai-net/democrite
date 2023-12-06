// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Implementations.Triggers
{
    using Democrite.Framework.Builders.Triggers;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Toolbox.Helpers;

    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Trigger input from a static collection
    /// </summary>
    /// <typeparam name="TTriggerOutput">The type of the trigger output.</typeparam>
    /// <seealso cref="ITriggerStaticCollectionInputBuilder{TTriggerOutput}" />
    internal sealed class TriggerStaticCollectionInputBuilder<TTriggerOutput> : ITriggerStaticCollectionInputBuilder<TTriggerOutput>
    {
        #region Fields

        private readonly IReadOnlyCollection<TTriggerOutput> _staticCollection;
        private PullModeEnum? _pullMode;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerStaticCollectionInputBuilder{TTriggerOutput}"/> class.
        /// </summary>
        public TriggerStaticCollectionInputBuilder(IEnumerable<TTriggerOutput> staticCollection)
        {
            this._staticCollection = staticCollection?.ToArray() ?? EnumerableHelper<TTriggerOutput>.ReadOnlyArray;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public InputSourceDefinition Build()
        {
            return new InputSourceStaticCollectionDefinition<TTriggerOutput>(this._staticCollection, this._pullMode ?? PullModeEnum.Circling);
        }

        /// <inheritdoc />
        public ITriggerStaticCollectionInputBuilder<TTriggerOutput> PullMode(PullModeEnum mode)
        {
            this._pullMode = mode;
            return this;
        }

        #endregion
    }
}
