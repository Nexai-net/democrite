// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;

    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define the default controller behavior
    /// </summary>
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class DefaultControllerOptions : ControllerBaseOptions, IControllerStorageOptions, IControllerStateOptions
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DefaultControllerOptions"/> class.
        /// </summary>
        static DefaultControllerOptions()
        {
            Default = new DefaultControllerOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultControllerOptions"/> class.
        /// </summary>
        public DefaultControllerOptions()
            : this(BlackboardProcessingResolutionLimitTypeEnum.Reject,
                   BlackboardProcessingResolutionRemoveTypeEnum.Remove)
        {
                
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultControllerOptions"/> class.
        /// </summary>
        public DefaultControllerOptions(BlackboardProcessingResolutionLimitTypeEnum limitResolutionPreference = BlackboardProcessingResolutionLimitTypeEnum.Reject,
                                        BlackboardProcessingResolutionRemoveTypeEnum removeResolutionPreference = BlackboardProcessingResolutionRemoveTypeEnum.Remove)
        {
            this.LimitResolutionPreference = limitResolutionPreference;
            this.RemoveResolutionPreference = removeResolutionPreference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static DefaultControllerOptions Default { get; }

        /// <summary>
        /// Gets the limit conflict resolution modes by order of preferece resolution.
        /// </summary>
        [Id(0)]
        public BlackboardProcessingResolutionLimitTypeEnum LimitResolutionPreference { get; }

        [Id(1)]
        public BlackboardProcessingResolutionRemoveTypeEnum RemoveResolutionPreference { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals([NotNull] ControllerBaseOptions other)
        {
            return other is DefaultControllerOptions defOption &&
                   this.LimitResolutionPreference == defOption.LimitResolutionPreference &&
                   this.RemoveResolutionPreference == defOption.RemoveResolutionPreference;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.LimitResolutionPreference, this.RemoveResolutionPreference);
        }

        #endregion
    }
}
