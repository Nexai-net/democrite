// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Models
{
    using System.Threading.Tasks;

    /// <summary>
    /// Result returned by stage execution
    /// </summary>
    public readonly record struct StageStepResult(Type? expectedResultType, Task result);
}
