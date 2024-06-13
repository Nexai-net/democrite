// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Configurations
{
    using Elvex.Toolbox.Models;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Immutable]
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public sealed class ConfigurationDirectDefinition<TData> : ConfigurationBaseDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationDirectDefinition{TData}"/> class.
        /// </summary>
        public ConfigurationDirectDefinition(Guid uid,
                                             string displayName,
                                             string configName,
                                             TData data,
                                             bool secureDataTansfert) 
            : base(uid, displayName, configName, (ConcretBaseType)typeof(TData).GetAbstractType(), secureDataTansfert)
        {
            this.Data = data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the data.
        /// </summary>
        [DataMember]
        public TData? Data { get; }

        #endregion
    }
}
