// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstraction.UnitTests.Models
{
    using AutoFixture;
    using AutoFixture.Kernel;

    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Node.Abstractions.Models;

    using Elvex.Toolbox.Abstractions.Conditions;
    using Elvex.Toolbox.Models;

    using System;
    using System.Linq.Expressions;

    using Xunit;

    using BaseTesterAlias = Democrite.UnitTests.ToolKit.Tests.SurrogateBaseTest<Abstractions.Models.SequenceExecutorState, Abstractions.Models.SequenceExecutorStateSurrogate, Abstractions.Models.SequenceExecutorStateConverter>;

    /// <summary>
    /// Test for <see cref="SequenceExecutorExecThreadState"/> serialization through <see cref="SequenceExecutorExecThreadStateSurrogate"/>
    /// </summary>
    public sealed class SequenceExecutorExecSurrogateUTest
    {
        #region Fields

        private readonly BaseTesterAlias _tester;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorExecSurrogateUTest"/> class.
        /// </summary>
        public SequenceExecutorExecSurrogateUTest()
        {
            this._tester = new BaseTesterAlias(sourceCreation: SequenceTestCreation, surrogateCreation: SurrogateCreation);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Ensures the surrogate is serialization and deserializable.
        /// </summary>
        [Fact]
        public void Ensure_Surrogate_Serialization()
        {
            this._tester.Ensure_Surrogate_Serialization();

        }

        /// <summary>
        /// Ensures the <see cref="SequenceExecutorExecThreadState"/> is serializable using <see cref="SequenceExecutorExecThreadStateSurrogate"/> and <see cref="SequenceExecutorExecThreadStateConverter"/>.
        /// </summary>
        [Fact]
        public void Ensure_Source_Is_Serializable_Using_Surrogate_And_Converter()
        {
            this._tester.Ensure_Source_Is_Serializable_Using_Surrogate_And_Converter();
        }

        #region Tools

        /// <summary>
        /// Sequences the test creation.
        /// </summary>
        private SequenceExecutorState SequenceTestCreation(Fixture fixture)
        {
            fixture.Register<SequenceDefinition>(() =>
            {
                return new SequenceDefinition(Guid.NewGuid(),
                                              RefIdHelper.Generate(Core.Abstractions.Enums.RefTypeEnum.Sequence, "sequence-test-creation", "unit.tests"),
                                              fixture.Create<string>(),
                                              SequenceOptionDefinition.Default,
                                              new SequenceStageDefinition[]
                                              {
                                                  new SequenceStageFilterDefinition(Guid.NewGuid(), (CollectionType)typeof(List<string>).GetAbstractType() ,"", ((Expression<Func<string, bool>>)(s => s != null)).Serialize(), null),
                                                  new SequenceStageFilterDefinition(Guid.NewGuid(), (CollectionType)typeof(List<string>).GetAbstractType() ,"", ((Expression<Func<string, bool>>)(s => s != null)).Serialize(), null),
                                                  new SequenceStageFilterDefinition(Guid.NewGuid(), (CollectionType)typeof(List<string>).GetAbstractType() ,"", ((Expression<Func<string, bool>>)(s => s != null)).Serialize(), null),
                                              },
                                              null);
            });

            return new SequenceExecutorState(Guid.NewGuid(),
                                             Guid.NewGuid().ToString(),
                                             Guid.NewGuid(),
                                             Guid.NewGuid(),
                                             fixture.Create<SequenceExecutorExecThreadState>(),
                                             fixture.Create<DateTime>(),
                                             null);
        }

        private SequenceExecutorStateSurrogate SurrogateCreation(Fixture fixture)
        {
            fixture.Customizations.Add(new TypeRelay(typeof(VGrainRedirectionDefinition), typeof(VGrainInterfaceRedirectionDefinition)));

            fixture.Register<ConditionBaseDefinition>(() => new ConditionValueDefinition(typeof(int).GetAbstractType(), 42));
            fixture.Register<AbstractType>(() => new ConcretType(fixture.Create<string>(), null, fixture.Create<string>(), false, null!));

            return fixture.Create<SequenceExecutorStateSurrogate>();
        }

        #endregion

        #endregion
    }
}
