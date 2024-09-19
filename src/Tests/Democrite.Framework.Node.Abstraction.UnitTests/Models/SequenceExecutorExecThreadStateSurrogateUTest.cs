namespace Democrite.Framework.Node.Abstraction.UnitTests.Models
{
    using AutoFixture;

    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Node.Abstractions.Models;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Models;

    using System;
    using System.Linq.Expressions;

    using Xunit;

    using BaseTesterAlias = Democrite.UnitTests.ToolKit.Tests.SurrogateBaseTest<Abstractions.Models.SequenceExecutorExecThreadState, Abstractions.Models.SequenceExecutorExecThreadStateSurrogate, Abstractions.Models.SequenceExecutorExecThreadStateConverter>;

    /// <summary>
    /// Test for <see cref="SequenceExecutorExecThreadState"/> serialization through <see cref="SequenceExecutorExecThreadStateSurrogate"/>
    /// </summary>
    public sealed class SequenceExecutorExecThreadStateSurrogateUTest
    {
        #region Fields
        
        private readonly BaseTesterAlias _tester;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorExecThreadStateSurrogateUTest"/> class.
        /// </summary>
        public SequenceExecutorExecThreadStateSurrogateUTest()
        {
            this._tester = new BaseTesterAlias(sourceCreation: SequenceTestCreation);
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
        private SequenceExecutorExecThreadState SequenceTestCreation(Fixture fixture)
        {
            fixture.Register<SequenceDefinition>(() =>
            {
                return new SequenceDefinition(Guid.NewGuid(),
                                              RefIdHelper.Generate(RefTypeEnum.Sequence, "test"),
                                              fixture.Create<string>(),
                                              SequenceOptionDefinition.Default,
                                              new SequenceStageDefinition[]
                                              {
                                                  new SequenceStageFilterDefinition(Guid.NewGuid(), (CollectionType)typeof(List<string>).GetAbstractType(), "", ((Expression<Func<string, bool>>)(s => s != null)).Serialize(), null),
                                                  new SequenceStageFilterDefinition(Guid.NewGuid(), (CollectionType)typeof(List<string>).GetAbstractType(), "", ((Expression<Func<string, bool>>)(s => s != null)).Serialize(), null),
                                                  new SequenceStageFilterDefinition(Guid.NewGuid(), (CollectionType)typeof(List<string>).GetAbstractType(), "", ((Expression<Func<string, bool>>)(s => s != null)).Serialize(), null),
                                              },
                                              null);
            });

            return fixture.Create<SequenceExecutorExecThreadState>();
        }

        #endregion

        #endregion
    }
}
