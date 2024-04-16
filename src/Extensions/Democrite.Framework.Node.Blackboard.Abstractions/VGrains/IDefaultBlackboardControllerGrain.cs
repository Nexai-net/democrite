// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.VGrains
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains.Controllers;

    /// <summary>
    /// Dabic default controller mainly in charge to resolve data conflict
    /// </summary>
    /// <seealso cref="IBlackboardBaseControllerGrain" />
    public interface IDefaultBlackboardControllerGrain : IBlackboardStorageControllerGrain, IBlackboardBaseControllerGrain<DefaultControllerOptions>
    {
    }
}
