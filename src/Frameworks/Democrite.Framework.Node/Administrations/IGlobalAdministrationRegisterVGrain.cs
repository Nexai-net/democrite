// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Administrations
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;

    [VGrainIdSingleton]
    [DemocriteSystemVGrain]
    internal interface IGlobalAdministrationRegisterVGrain : IVGrain
    {
    }
}
