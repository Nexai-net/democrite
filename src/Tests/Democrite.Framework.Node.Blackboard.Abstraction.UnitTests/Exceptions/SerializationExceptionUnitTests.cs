// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstraction.UnitTests.Exceptions
{
    using AutoFixture;

    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.UnitTests.ToolKit.Tests;
    using Democrite.UnitTests.ToolKit.Xunits;

    using System;

    using Xunit;

    /// <summary>
    /// All exception in assembly Democrite.Framework.Node.Blackboard.Abstractions
    /// </summary>
    /// <seealso cref="ExceptionSerializationTester" />
    public sealed class SerializationExceptionUnitTests : ExceptionSerializationTester
    {
        #region Methods

        [Theory]
        [ChildTypeData<IDemocriteException, Democrite.Framework.Node.Blackboard.Abstractions.Exceptions.BlackboardCommandExecutionException>]
        public void Node_Exception_Serialization_Through_Surrogate(Type type)
        {
            base.Exception_Serialization_Through_Surrogate(type);
        }

        #endregion

        #region Tools

        /// <inheritdoc />
        protected override void OnSourceCreationSetup<TSource, TSurrogate, TConverter>(Fixture fixture)
        {
            fixture.Register<DataRecordContainer>(() =>
            {
                return new DataRecordContainer<double>(fixture.Create<string>(),
                                                       fixture.Create<Guid>(),
                                                       fixture.Create<string>(),
                                                       fixture.Create<double>(),
                                                       RecordStatusEnum.Ready,
                                                       fixture.Create<DateTime>(),
                                                       fixture.Create<string>(),
                                                       fixture.Create<DateTime>(),
                                                       fixture.Create<string>(),
                                                       null);
            });

            base.OnSourceCreationSetup<TSource, TSurrogate, TConverter>(fixture);
        }

        #endregion
    }
}
