// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Toolbox.Abstractions.Supports;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Service at node level that need to be initialize at start
    /// </summary>
    /// <seealso cref="ISupportInitialization" />
    public interface INodeInitService : ISupportInitialization<IServiceProvider>
    {
    }
}
