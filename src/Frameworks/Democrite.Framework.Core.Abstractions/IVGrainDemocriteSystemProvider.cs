// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Services
{
    using Democrite.Framework.Core.Abstractions;

    /// <summary>
    /// Provider that used the <see cref="IGrainOrleanFactory"/> to ensure no redirection
    /// </summary>
    /// <seealso cref="IVGrainProvider" />
    public interface IVGrainDemocriteSystemProvider : IVGrainProvider
    {
    }
}
