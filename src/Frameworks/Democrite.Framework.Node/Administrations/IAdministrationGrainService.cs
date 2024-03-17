// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Administrations
{
    using Democrite.Framework.Node.Abstractions.Administrations;

    using Orleans.Services;

    /// <summary>
    /// Silo service used to align local service with administration configuration
    /// </summary>
    /// <seealso cref="IGrainService" />
    internal interface IAdministrationGrainService : IGrainService, IAdminEventReceiver
    {
    }
}
