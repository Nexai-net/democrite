// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    /// <summary>
    /// Builder used to setup cluster information
    /// </summary>
    /// <typeparam name="TOption">The type of the option.</typeparam>
    public interface IDemocriteClusterExternalBuilder<TOption> : IDemocriteExternalServiceBuilder<TOption, IDemocriteClusterExternalBuilder<TOption>>
        where TOption : class, new()
    {
    }
}
