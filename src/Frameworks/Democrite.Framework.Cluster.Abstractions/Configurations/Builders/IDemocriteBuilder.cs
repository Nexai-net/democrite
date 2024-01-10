// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    /// <summary>
    /// Root of all builder used to configure a cluster part
    /// </summary>
    public interface IDemocriteBuilder
    {
    }

    /// <summary>
    /// Builder used to configure a cluster part
    /// </summary>
    public interface IDemocriteBuilder<TWizard, TWizardConfig> : IDemocriteBuilder
        where TWizard : IDemocriteWizard<TWizard, TWizardConfig>
        where TWizardConfig : IDemocriteCoreConfigurationWizard<TWizardConfig>
    {
        /// <summary>
        /// Configurate the cluster node 
        /// </summary>
        IDemocriteWizardStart<TWizard, TWizardConfig> WizardConfig();

        /// <summary>
        /// Setups the cluster option.
        /// </summary>
        IDemocriteBuilder<TWizard, TWizardConfig> SetupClusterOption(Action<IClusterOptionBuilder> value);
    }
}
