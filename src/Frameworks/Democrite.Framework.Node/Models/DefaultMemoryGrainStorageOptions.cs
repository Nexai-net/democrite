// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Models
{
    using Orleans.Configuration;

    public record class DefaultMemoryGrainStorageOptions(uint NumStorageGrains = 10, uint InitStage = MemoryGrainStorageOptions.DEFAULT_INIT_STAGE);
}
