// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.UnitTests.Tools
{
    using AutoFixture;

    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Sequence;

    using Elvex.Toolbox.Models;
    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    using System;

    public class TestSequenceStageDefinition : SequenceStageDefinition
    {
        private static readonly Fixture s_globalAutoFixture;

        static TestSequenceStageDefinition()
        {
            s_globalAutoFixture = ObjectTestHelper.PrepareFixture();
        }

        public TestSequenceStageDefinition(Guid uid,
                                           string displayName,
                                           StageTypeEnum type,
                                           AbstractType? input,
                                           AbstractType? output,
                                           DefinitionMetaData? metaData,
                                           bool preventReturn = false)
            : base(uid, displayName, type, input, output, metaData, preventReturn)
        {
        }

        protected override bool OnStageEquals(SequenceStageDefinition other)
        {
            return true;
        }

        protected override int OnStageGetHashCode()
        {
            return 0;
        }

        public static TestSequenceStageDefinition Create(IFixture? fixture = null, Guid? uid = null)
        {
            fixture ??= s_globalAutoFixture;

            return new TestSequenceStageDefinition(uid ?? fixture.Create<Guid>(),
                                                   fixture.Create<string>(),
                                                   fixture.Create<StageTypeEnum>(),
                                                   fixture.Create<AbstractType>(),
                                                   fixture.Create<AbstractType>(),
                                                   fixture.Create<DefinitionMetaData>(),
                                                   fixture.Create<bool>());
        }
    }
}
