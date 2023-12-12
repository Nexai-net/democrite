// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Orleans.Services;

    /// <summary>
    /// Grain service client for <see cref="IComponentIdentitCardProvider"/>
    /// </summary>
    internal interface IComponentIdentitCardProviderClient : IGrainServiceClient<IComponentIdentitCardProvider>, IComponentIdentitCardProvider
    {
    }
}
