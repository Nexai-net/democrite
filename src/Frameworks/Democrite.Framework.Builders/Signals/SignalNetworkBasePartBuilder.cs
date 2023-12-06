// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Signals
{
    /// <inheritdoc />
    internal abstract class SignalNetworkBasePartBuilder<TWizard> : ISignalNetworkBasePartBuilder<TWizard>
        where TWizard : ISignalNetworkBasePartBuilder<TWizard>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalNetworkBasePartBuilder{TWizard, TDefinition}"/> class.
        /// </summary>
        protected SignalNetworkBasePartBuilder(string name, Guid? uid = null)
        {
            this.Name = name;
            this.Uid = uid ?? Guid.NewGuid();
        }

        #endregion

        #region Properties

        /// <inheritdoc cref="ISignalNetworkBasePartBuilder{TWizard, TDefinition}.Group(string)"/>
        public string? GroupName { get; private set; }

        /// <summary>
        /// Gets unique signal name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the unique id.
        /// </summary>
        public Guid Uid { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public TWizard Group(string groupName)
        {
            this.GroupName = groupName;
            return GetWiazrd();
        }

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
