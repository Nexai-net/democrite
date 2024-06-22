// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Docker.Abstractions
{
    using global::Docker.DotNet;

    /// <summary>
    /// Abstraction above docker.dotnet client api
    /// </summary>
    public interface IDockerClientProxy : IDockerClient, IDisposable
    {
    }
}
