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
    [GenerateSerializer]
    [ImmutableObject(true)]
    public class BlackboardTemplateControllerDefinition : IEquatable<BlackboardTemplateControllerDefinition>, IDefinition
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
                                                      ControllerBaseOptions options,
                                                      DefinitionMetaData? metaData)
        {
            this.Uid = uid;
            this.MetaData = metaData;
            this.ControllerType = controllerType;
            this.Options = options;
            this.AgentInterfaceType = agentInterfaceType;
            this.DisplayName = $"[{this.ControllerType:G}] => {this.AgentInterfaceType.DisplayName}";
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [DataMember]
        [Id(0)]
        public Guid Uid { get; }

        /// <inheritdoc />
        [DataMember]
        [Id(1)]
        public DefinitionMetaData? MetaData { get; }

        /// <inheritdoc />
        [DataMember]
        [Id(2)]
        public string DisplayName { get; }

        /// <summary>
        /// Gets the type of the controller.
        /// </summary>
        [DataMember]
        [Id(3)]
        public BlackboardControllerTypeEnum ControllerType { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        [DataMember]
        [Id(4)]
        public ControllerBaseOptions Options { get; }

        /// <summary>
        /// Gets the type of the agent interface.
        /// </summary>
        [DataMember]
        [Id(5)]
        public ConcretType AgentInterfaceType { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(BlackboardTemplateControllerDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return other.ControllerType == this.ControllerType &&
                   other.AgentInterfaceType.Equals(this.AgentInterfaceType) &&
                   other.DisplayName == this.DisplayName &&
                   other.MetaData == this.MetaData &&
                   other.Options == this.Options &&
                   OnTemplateEquals(other);   
        }

        /// <summary>
        /// Called when [template equals].
        /// </summary>
        protected virtual bool OnTemplateEquals(BlackboardTemplateControllerDefinition other)
        {
            return true;
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is BlackboardTemplateControllerDefinition def)
                return Equals(def);

            return false;
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.ControllerType,
                                    this.AgentInterfaceType,
                                    this.DisplayName,
                                    this.MetaData,
                                    this.Options,
                                    OnTemplateGetHashCode());
        }

        /// <summary>
        /// Called when [template get hash code].
        /// </summary>
        protected virtual int OnTemplateGetHashCode()
        {
            return 0;
        }

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
