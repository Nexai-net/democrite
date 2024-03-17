// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Helpers
{
    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Runtime;

    using System;

    public sealed class TestGrainLifecycle : LifecycleSubject, IGrainLifecycle
    {
        public TestGrainLifecycle(ILogger logger)
            : base(logger)
        {
        }

        public void AddMigrationParticipant(IGrainMigrationParticipant participant)
        {
            throw new NotImplementedException();
        }

        public void RemoveMigrationParticipant(IGrainMigrationParticipant participant)
        {
            throw new NotImplementedException();
        }
    }
}
