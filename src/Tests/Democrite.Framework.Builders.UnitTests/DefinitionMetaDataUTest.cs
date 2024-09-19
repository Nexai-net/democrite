// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.UnitTests
{
    using AutoFixture;

    using Democrite.Framework.Builders.Artifacts;
    using Democrite.Framework.Builders.Signals;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    using NFluent;

    using NSubstitute;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="IDefinitionMetaDataBuilder"/>, able to add metadata a all definition level
    /// </summary>
    public class DefinitionMetaDataUTest
    {
        #region Fields

        private readonly Fixture _fixture;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionMetaDataUTest"/> class.
        /// </summary>
        public DefinitionMetaDataUTest()
        {
            this._fixture = ObjectTestHelper.PrepareFixture();
        }

        #endregion

        #region Nesteds

        public interface ITestVGrain : IVGrain
        {
            Task TestMethodAsync(IExecutionContext ctx);
            Task<string> TestMethodAsync(string? str, IExecutionContext ctx);
        }

        #endregion

        #region Methods

        [Fact]
        public void Sequence()
        {
            MetaDataBuildTester(tester => Builders.Sequence.Build(this._fixture.Create<string>().ToLower(),
                                                                  this._fixture.Create<string>(),
                                                                  this._fixture.Create<Guid>(),
                                                                  null,
                                                                  metadataBuilder: m => MimicMetaDataDefinition(m, tester))
                                                            .NoInput()
                                                            .Build());
        }

        [Fact]
        public void Sequence_Stage_Call()
        {
            MetaDataBuildTester(tester => Builders.Sequence.Build(this._fixture.Create<string>().ToLower(),
                                                                  this._fixture.Create<string>(),
                                                                  this._fixture.Create<Guid>())
                                                           .NoInput()
                                                           .Use<ITestVGrain>(metaDataBuilder: m => MimicMetaDataDefinition(m, tester)).Call((g, ctx) => g.TestMethodAsync(ctx)).Return
                                                           .Build(), 
                                d => d.Stages.First().MetaData);
        }

        [Fact]
        public void Sequence_Stage_Foreach()
        {
            MetaDataBuildTester(tester => Builders.Sequence.Build(this._fixture.Create<string>().ToLower(), this._fixture.Create<string>(), this._fixture.Create<Guid>())
                                                           .RequiredInput<IEnumerable<string>>()
                                                           .Foreach(IType.From<string>(), s =>
                                                           {
                                                               return s.Use<ITestVGrain>().Call((g, str, ctx) => g.TestMethodAsync(str, ctx)).Return;
                                                           }, metaDataBuilderAction: m => MimicMetaDataDefinition(m, tester))
                                                           .Build(),
                                d => d.Stages.First().MetaData);
        }

        [Fact]
        public void Sequence_Stage_Foreach_InnerDef()
        {
            MetaDataBuildTester(tester => Builders.Sequence.Build(this._fixture.Create<string>().ToLower(), this._fixture.Create<string>(), this._fixture.Create<Guid>())
                                                           .RequiredInput<IEnumerable<string>>()
                                                           .Foreach(IType.From<string>(), s =>
                                                           {
                                                               return s.Use<ITestVGrain>(metaDataBuilder: m => MimicMetaDataDefinition(m, tester)).Call((g, str, ctx) => g.TestMethodAsync(str, ctx)).Return;
                                                           })
                                                           .Build(),
                                d => ((SequenceStageForeachDefinition)d.Stages.First()).InnerFlow.Stages.First().MetaData);
        }

        [Fact]
        public void Sequence_Stage_PushToContext()
        {
            MetaDataBuildTester(tester => Builders.Sequence.Build(this._fixture.Create<string>().ToLower(), this._fixture.Create<string>(), this._fixture.Create<Guid>())
                                                           .RequiredInput<int>()
                                                           .PushToContext(i => i, metaDataBuilder: m => MimicMetaDataDefinition(m, tester))
                                                           .Build(),
                                d => d.Stages.First().MetaData);
        }

        [Fact]
        public void Sequence_Stage_FireSignal()
        {
            MetaDataBuildTester(tester => Builders.Sequence.Build(this._fixture.Create<string>().ToLower(), this._fixture.Create<string>(), this._fixture.Create<Guid>())
                                                           .RequiredInput<int>()
                                                           .FireSignal(Guid.NewGuid(), metaDataBuilder: m => MimicMetaDataDefinition(m, tester)).RelayMessage()
                                                           .Build(),
                                d => d.Stages.First().MetaData);
        }

        [Fact]
        public void Sequence_Stage_Select()
        {
            MetaDataBuildTester(tester => Builders.Sequence.Build(this._fixture.Create<string>().ToLower(), this._fixture.Create<string>(), this._fixture.Create<Guid>())
                                                           .RequiredInput<int>()
                                                           .Select(i => i, metaDataBuilder: m => MimicMetaDataDefinition(m, tester))
                                                           .Build(),
                                d => d.Stages.First().MetaData);
        }

        [Fact]
        public void Sequence_Stage_NestedSequenceCall()
        {
            MetaDataBuildTester(tester => Builders.Sequence.Build(this._fixture.Create<string>().ToLower(), this._fixture.Create<string>(), this._fixture.Create<Guid>())
                                                           .RequiredInput<int>()
                                                           .CallSequence(Guid.NewGuid(), metaDataBuilder: m => MimicMetaDataDefinition(m, tester)).ReturnNoData
                                                           .Build(),
                                d => d.Stages.First().MetaData);
        }

        [Fact]
        public void Signal()
        {
            MetaDataBuildTester(tester => Builders.Signal.Create(new SignalId(this._fixture.Create<Guid>(), this._fixture.Create<string>().ToLower()),
                                                                 m => MimicMetaDataDefinition(m, tester)));

            MetaDataBuildTester(tester => Builders.Signal.Create(this._fixture.Create<string>().ToLower(),
                                                                 this._fixture.Create<string>(),
                                                                 this._fixture.Create<Guid>(),
                                                                 m => MimicMetaDataDefinition(m, tester)));
        }

        [Fact]
        public void Door_Logic()
        {
            MetaDataBuildTester(tester => Builders.Door.Create(this._fixture.Create<string>().ToLower(), 
                                                               this._fixture.Create<string>(), 
                                                               this._fixture.Create<Guid>(), 
                                                               m => MimicMetaDataDefinition(m, tester))
                                                       .Listen(this._fixture.Create<SignalId>())
                                                       .Relay()
                                                       .Build());
        }

        [Fact]
        public void Door_FilterRelay()
        {
            MetaDataBuildTester(tester => Builders.Door.Create(this._fixture.Create<string>().ToLower(), this._fixture.Create<string>(), this._fixture.Create<Guid>(), m => MimicMetaDataDefinition(m, tester))
                                                       .Listen(this._fixture.Create<SignalId>())
                                                       .UseRelayFilter(s => s.From != null)
                                                       .Build());
        }

        [Fact]
        public void Trigger_Cron()
        {
            MetaDataBuildTester(tester => Builders.Trigger.Cron("* * * * *",
                                                                this._fixture.Create<string>().ToLower(),
                                                                this._fixture.Create<string>(),
                                                                this._fixture.Create<Guid>(),
                                                                m => MimicMetaDataDefinition(m, tester))
                                                          .AddTargetSequence(Guid.NewGuid())   
                                                          .Build());
        }

        [Fact]
        public void Trigger_Signal()
        {
            MetaDataBuildTester(tester => Builders.Trigger.Signal(this._fixture.Create<SignalId>(),
                                                                  this._fixture.Create<string>().ToLower(),
                                                                  this._fixture.Create<string>(),
                                                                  this._fixture.Create<Guid>(),
                                                                  m => MimicMetaDataDefinition(m, tester))
                                                          .AddTargetSequence(Guid.NewGuid())
                                                          .Build());
        }

        [Fact]
        public void Trigger_Door()
        {
            MetaDataBuildTester(tester => Builders.Trigger.Door(this._fixture.Create<DoorId>(),
                                                                this._fixture.Create<string>().ToLower(),
                                                                this._fixture.Create<string>(),
                                                                this._fixture.Create<Guid>(),
                                                                m => MimicMetaDataDefinition(m, tester))
                                                          .AddTargetSequence(Guid.NewGuid())
                                                          .Build());
        }

        [Fact]
        public void Trigger_Stream()
        {
            MetaDataBuildTester(tester => Builders.Trigger.Stream(this._fixture.Create<Guid>(),
                                                                  this._fixture.Create<string>().ToLower(),
                                                                  this._fixture.Create<string>(),
                                                                  this._fixture.Create<Guid>(),
                                                                  m => MimicMetaDataDefinition(m, tester))
                                                          .MaxConcurrentProcess(1)
                                                          .AddTargetSequence(Guid.NewGuid())
                                                          .Build());
        }

        [Fact]
        public void StreamQueue()
        {
            MetaDataBuildTester(tester => Builders.StreamQueue.CreateFromDefaultStream(this._fixture.Create<string>().ToLower(), this._fixture.Create<Guid>(), this._fixture.Create<Guid>(), m => MimicMetaDataDefinition(m, tester)));
            MetaDataBuildTester(tester => Builders.StreamQueue.CreateFromDefaultStream(this._fixture.Create<string>().ToLower(), this._fixture.Create<string>(), this._fixture.Create<string>(), this._fixture.Create<Guid>(), m => MimicMetaDataDefinition(m, tester)));
            MetaDataBuildTester(tester => Builders.StreamQueue.CreateFromDefaultStream(this._fixture.Create<string>().ToLower(), this._fixture.Create<string>(), this._fixture.Create<Guid>(), this._fixture.Create<Guid>(), m => MimicMetaDataDefinition(m, tester)));
            MetaDataBuildTester(tester => Builders.StreamQueue.CreateFromDefaultStream(this._fixture.Create<string>().ToLower(), this._fixture.Create<string>(), this._fixture.Create<Guid>(), m => MimicMetaDataDefinition(m, tester)));

            MetaDataBuildTester(tester => Builders.StreamQueue.Create(this._fixture.Create<string>().ToLower(), this._fixture.Create<string>(), this._fixture.Create<string>(), this._fixture.Create<string>(), this._fixture.Create<Guid>(), m => MimicMetaDataDefinition(m, tester)));
            MetaDataBuildTester(tester => Builders.StreamQueue.Create(this._fixture.Create<string>().ToLower(), this._fixture.Create<string>(), this._fixture.Create<string>(), this._fixture.Create<Guid>(), this._fixture.Create<Guid>(), m => MimicMetaDataDefinition(m, tester)));
        }

        [Fact]
        public void Artifact()
        {
            var hashService = Substitute.For<IHashService>();

            var root = new Uri("c:/poney", UriKind.Absolute);

            var fileSystemService = Substitute.For<IFileSystemHandler>();
            fileSystemService.MakeUriAbsolute(Arg.Any<Uri>()).Returns(root);

            hashService.GetHash(Arg.Any<IReadOnlyCollection<Uri>>(), fileSystemService, Arg.Any<CancellationToken>()).Returns("Hash42");

            MetaDataBuildTester(tester => Builders.Artifact.VGrain(this._fixture.Create<string>().ToLower(), this._fixture.Create<string>(), this._fixture.Create<Guid>())
                                                           .MetaData(m => MimicMetaDataDefinition(m, tester))
                                                           .Python()
                                                           .Directory("./")
                                                           .ExecuteFile("a.py").CompileAsync(hashService, fileSystemService));
        }

        #region Tools

        private void MimicMetaDataDefinition(IDefinitionMetaDataBuilder m, DefinitionMetaData origin)
        {
            m.CategoryPath(origin.CategoryPath!)
             .Description(origin.Description!)
             .Namespace(origin.NamespaceIdentifier)
             .AddTags(origin.Tags.ToArray());
        }

        private void MimicMetaDataDefinition(IDefinitionMetaDataWithDisplayNameBuilder m, DefinitionMetaData origin)
        {
            m.CategoryPath(origin.CategoryPath!)
             .Description(origin.Description!)
             .DisplayName(origin.Description!)
             .Namespace(origin.NamespaceIdentifier)
             .AddTags(origin.Tags.ToArray());
        }

        private void MetaDataBuildTester<TDefinition>(Func<DefinitionMetaData, ValueTask<TDefinition>> builder, Func<TDefinition, DefinitionMetaData?>? metadataTestedAccess = null)
            where TDefinition : class, IDefinition
        {
            MetaDataBuildTester<TDefinition>(t => builder(t).GetAwaiter().GetResult(), metadataTestedAccess);
        }

        private void MetaDataBuildTester<TDefinition>(Func<DefinitionMetaData, TDefinition> builder, Func<TDefinition, DefinitionMetaData?>? metadataTestedAccess = null)
            where TDefinition : class, IDefinition
        {
            var tester_base = this._fixture.Create<DefinitionMetaData>();

            var tester = new DefinitionMetaData(tester_base.Description,
                                                tester_base.CategoryPath,
                                                tester_base.Tags,
                                                tester_base.UTCUpdateTime,
                                                tester_base.NamespaceIdentifier?.ToLowerWithSeparator('.').Replace("_", ".").Replace("-", "."));

            var tags = tester.Tags.ToArray();
            var distinctTags = tags.Distinct().ToArray();

            var definition = builder(tester);
            Check.That(definition).IsNotNull();

            DefinitionMetaData? metaData = null;
            if (metadataTestedAccess is null)
                metaData = definition?.MetaData;
            else
                metaData = metadataTestedAccess(definition);

            Check.That(metaData).IsNotNull();
            Check.That(metaData!.CategoryPath).IsNotNull().And.IsEqualTo(tester.CategoryPath);
            Check.That(metaData!.Description).IsNotNull().And.IsEqualTo(tester.Description);
            Check.That(metaData!.NamespaceIdentifier).IsEqualTo(tester.NamespaceIdentifier);
            Check.That(metaData!.Tags).IsNotNull().And.CountIs(distinctTags.Length).And.Contains(distinctTags);
        }

        #endregion

        #endregion
    }
}
