// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstraction.UnitTests.Exceptions
{
    using AutoFixture;

    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.UnitTests.ToolKit.Tests;
    using Democrite.UnitTests.ToolKit.Xunits;

    using System;

    using Xunit;

    /// <summary>
    /// All exception in assembly Democrite.Framework.Core.Abstraction
    /// </summary>
    /// <seealso cref="ExceptionSerializationTester" />
    public sealed class SerializationExceptionUnitTests : ExceptionSerializationTester
    {
        [Theory]
        [ChildTypeData<IDemocriteException, Democrite.Framework.Core.Abstractions.IDefinition>]
        public void Node_Exception_Serialization_Through_Surrogate(Type type)
        {
            base.Exception_Serialization_Through_Surrogate(type);
        }

        protected override void OnSourceCreationSetup<TSource, TSurrogate, TConverter>(Fixture fixture)
        {
            fixture.Register<DemocriteBaseException>(() => new DemocriteException("inner error"));

            base.OnSourceCreationSetup<TSource, TSurrogate, TConverter>(fixture);
        }
    }
}
