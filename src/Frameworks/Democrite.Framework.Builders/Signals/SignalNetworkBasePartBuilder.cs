// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Signals
{
    using Democrite.Framework.Core.Abstractions;

    /// <inheritdoc />
    internal abstract class SignalNetworkBasePartBuilder<TWizard> : ISignalNetworkBasePartBuilder<TWizard>
        where TWizard : ISignalNetworkBasePartBuilder<TWizard>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalNetworkBasePartBuilder{TWizard, TDefinition}"/> class.
        /// </summary>
        protected SignalNetworkBasePartBuilder(string simpleNameIdentifier, string? displayName, Guid? uid = null, Action<IDefinitionMetaDataBuilder>? metaDataBuilder = null)
        {
            this.DisplayName = displayName;
            this.SimpleNameIdentifier = simpleNameIdentifier;
            this.Uid = uid ?? Guid.NewGuid();

            this.DefinitionMetaData = DefinitionMetaDataBuilder.Execute(metaDataBuilder);
        }

        #endregion

        #region Properties

        /// <inheritdoc cref="ISignalNetworkBasePartBuilder{TWizard, TDefinition}.Group(string)"/>
        public string? GroupName { get; private set; }

        /// <summary>
        /// Gets unique signal name
        /// </summary>
        public string SimpleNameIdentifier { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string? DisplayName { get; }

        /// <summary>
        /// Gets the unique id.
        /// </summary>
        public Guid Uid { get; }

        /// <summary>
        /// Gets the meta data.
        /// </summary>
        public DefinitionMetaData? DefinitionMetaData { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the wiazrd.
        /// </summary>
        protected virtual TWizard GetWiazrd()
        {
            if (this is TWizard localWizard)
                return localWizard;

            throw new NotImplementedException("Plz override the method " + nameof(GetWiazrd) + " for type " + GetType());
        }

        #endregion
    }
}
