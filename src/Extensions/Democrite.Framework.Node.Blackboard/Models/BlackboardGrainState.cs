// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Models
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class BlackboardGrainState
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardGrainState"/> class.
        /// </summary>
        public BlackboardGrainState(BlackboardTemplateDefinition? templateCopy,
                                    BlackboardId blackboardId,
                                    string name,
                                    BlackboardRecordRegistryState blackboardRecordRegistryState)
        {
            this.TemplateCopy = templateCopy;
            this.BlackboardId = blackboardId;
            this.Name = name;
            this.Registry = blackboardRecordRegistryState;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is build.
        /// </summary>
        public bool IsBuild
        {
            get { return this.TemplateCopy is not null; }
        }

        /// <summary>
        /// Gets a copy of the template used during the first initialization
        /// </summary>
        public BlackboardTemplateDefinition? TemplateCopy { get; private set; }

        /// <summary>
        /// Gets the blackboard identifier.
        /// </summary>
        public BlackboardId BlackboardId { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the registry.
        /// </summary>
        public BlackboardRecordRegistryState Registry { get;  }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the using template.
        /// </summary>
        internal void BuildUsingTemplate(BlackboardTemplateDefinition tmpl, BlackboardId blackboardId)
        {
            this.TemplateCopy = tmpl;
            this.BlackboardId = blackboardId;
        }

        #endregion
    }
}
