// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstraction.UnitTests
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.UnitTests.ToolKit.Extensions;

    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    using Microsoft.Extensions.Logging;

    using NFluent;

    using NSubstitute;

    using Orleans.Runtime;
    using Orleans.Services;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="DedicatedGrainId"/>
    /// </summary>
    public sealed class DedicatedGrainIdUTest
    {
        #region Nested

        internal interface IReceiver { }

        internal interface ITestGrain : IVGrain, IReceiver { }

        internal interface ITestGrainTest : IReceiver { }

        internal interface ITestGrainService : IGrainService { }

        internal interface ITestGrainServiceWithReceiver : IGrainService, IReceiver { }

        internal sealed class TestGrain : Grain, ITestGrain
        {
        }

        internal sealed class TestGrainWithExtra : Grain, ITestGrainTest, ITestGrain
        {
        }

        internal sealed class TestGrainService : GrainService, ITestGrainService
        {
            public TestGrainService(GrainId grainId, Silo silo, ILoggerFactory loggerFactory) 
                : base(grainId, silo, loggerFactory)
            {
            }
        }

        internal sealed class TestGrainServiceWithReceiver : GrainService, ITestGrainServiceWithReceiver
        {
            public TestGrainServiceWithReceiver(GrainId grainId, Silo silo, ILoggerFactory loggerFactory) 
                : base(grainId, silo, loggerFactory)
            {
            }
        }

        #endregion

        #region Methods

        [Fact]
        public async Task Create_From_ClassicGrain()
        {
            var id = Guid.NewGuid();
            var forceGrainId = GrainId.Create(GrainType.Create(nameof(TestGrain)), GrainIdKeyExtensions.CreateGuidKey(id));

            var fixture = ObjectTestHelper.PrepareFixture();
            var grain = await fixture.CreateAndInitVGrain<TestGrain>(forcedGrainId: forceGrainId);

            var dedicated = grain.GetDedicatedGrainId<IReceiver>();

            Check.That(dedicated.GrainInterface).IsNotNull().And.IsEqualTo(typeof(ITestGrain).GetAbstractType());
            Check.That(dedicated.IsGrainService).IsFalse();
            Check.That(dedicated.Target.Key).IsEqualTo(forceGrainId.Key);
        }

        #endregion
    }
}
