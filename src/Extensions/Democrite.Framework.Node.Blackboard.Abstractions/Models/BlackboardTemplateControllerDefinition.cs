// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains.Controllers;
    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Controller dedicated to specific control action on the 
    /// </summary>
    /// <seealso cref="IDefinition" />
    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public class BlackboardTemplateControllerDefinition : IDefinition
    {
        #region Fields

        private static readonly Type s_storageControllerType = typeof(IBlackboardStorageControllerGrain);

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardTemplateControllerDefinition"/> class.
        /// </summary>
        public BlackboardTemplateControllerDefinition(Guid uid,
                                                      BlackboardControllerTypeEnum controllerType,
                                                      ConcretType agentInterfaceType,
                                                      ControllerBaseOptions options)
        {
            this.Uid = uid;
            this.ControllerType = controllerType;
            this.Options = options;
            this.AgentInterfaceType = agentInterfaceType;
            this.DisplayName = $"[{this.ControllerType:G}] => {this.AgentInterfaceType.DisplayName}";
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [DataMember]
        public Guid Uid { get; }

        /// <inheritdoc />
        [DataMember]
        public string DisplayName { get; }

        /// <summary>
        /// Gets the type of the controller.
        /// </summary>
        [DataMember]
        public BlackboardControllerTypeEnum ControllerType { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        [DataMember]
        public ControllerBaseOptions Options { get; }

        /// <summary>
        /// Gets the type of the agent interface.
        /// </summary>
        [DataMember]
        public ConcretType AgentInterfaceType { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual string ToDebugDisplayName()
        {
            return $"{this.DisplayName} {(this.Options as ISupportDebugDisplayName)?.ToDebugDisplayName()}";
        }

        /// <inheritdoc />
        public bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            if (this.AgentInterfaceType.IsInterface == false)
            {
                logger.OptiLog(LogLevel.Critical, "{agent} must be an interface", this.AgentInterfaceType.DisplayName);
                return false;
            }

            var controllerInterfaceType = this.AgentInterfaceType.ToType();

            if (this.ControllerType == BlackboardControllerTypeEnum.None)
            {
                logger.OptiLog(LogLevel.Critical, "{ControllerType} must not be 'None'", this.AgentInterfaceType.DisplayName);
                return false;
            }

            bool result = true;

            if ((this.ControllerType & BlackboardControllerTypeEnum.Storage) == BlackboardControllerTypeEnum.Storage &&
                !controllerInterfaceType.IsAssignableTo(s_storageControllerType))
            {
                logger.OptiLog(LogLevel.Critical, "Storage controller {ControllerType} must inherite from {}", this.AgentInterfaceType.DisplayName, s_storageControllerType);
                result = false;
            }

            // TODO : Filter event and state type

            return result;
        }

        #endregion
    }
}
