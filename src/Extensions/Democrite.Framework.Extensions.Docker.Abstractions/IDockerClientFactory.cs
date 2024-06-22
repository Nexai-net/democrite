// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Docker.Abstractions
{
    /// <summary>
    /// Factory used to configured and provider <see cref="IDockerClientProxy"/>
    /// </summary>
    public interface IDockerClientFactory
    {
        /// <summary>
        /// Gets the local docker client
        /// </summary>
        IDockerClientProxy GetLocal(string? configurationName = null, string? minimalVersionRequired = null);
        global::Docker.DotNet.Models.AuthConfig GetRepositoryAuthentication(string? repository, string? configurationName);
    }
}
