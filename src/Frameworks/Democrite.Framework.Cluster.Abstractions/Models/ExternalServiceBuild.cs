// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Models
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Define <see cref="IClusterBuilderDemocriteExternalServiceBuilder{TOption, }"/> result
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public class ExternalServiceBuild<TOption>
        where TOption : class
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalServiceBuild"/> class.
        /// </summary>
        public ExternalServiceBuild(string? connectionString = null,
                                    TOption? option = null)
        {
            this.ConnectionString = connectionString;
            this.Option = option;
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        public string? ConnectionString { get; }

        /// <summary>
        /// Gets the option.
        /// </summary>
        public TOption? Option { get; }

        #endregion
    }
}
