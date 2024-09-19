// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.UnitTests
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Builders.UnitTests.TestData;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.UnitTests.ToolKit.Tests;

    using Moq;

    using NFluent;

    using System;
    using System.Diagnostics;
    using System.Net;

    /// <summary>
    /// 
    /// </summary>
    public class SequenceBuildUnitTest
    {
        #region Fields

        private const string TEST_GOOGLE_URI_STRING = "www.google.com";
        private const string TEST_FACEBOOK_URI_STRING = "www.facebook.com";

        #endregion

        #region Methods

        #region Nested

        public class TestOptionArg
        {
            public int testNumber { get; set; }
            public Guid testId { get; set; }
        }

        public interface ISequenceTestConverter : IVGrain
        {
            Task<Guid> Convert(string input);

            Task<Guid> Convert(IPAddress input);

            Task<int> Convert(Guid input);
        }

        #endregion

        /// <summary>
        /// Create a simple small sequence definition
        /// </summary>
        [Fact]
        public void Simple()
        {
            var definition = Sequence.Build("unit-test-simple")
                                     .NoInput()
                                     .Use<IBasicTestVGrain>().Call((a, ctx) => a.ExecuteStringAsync(ctx)).Return
                                     .Use<IBasicTestOtherVGrain>().Call((a, input, ctx) => a.OtherReturnIpAddressFromStringAsync(input, ctx)).Return
                                     .Build();

            Check.That(definition).IsNotNull();
            Check.That(definition.Uid).IsNotEqualTo(Guid.Empty);

            Check.That(definition.DisplayName).IsNotNull().And.IsEqualTo(definition.DisplayName);

            Check.That(definition.Input).IsNull();
            Check.That(definition.Output!.ToType()).IsNotNull().And.IsEqualTo(typeof(IPAddress));

            Check.That(definition.Stages).IsNotNull().And.CountIs(2);

            var first = definition.Stages.FirstOrDefault();

            Check.That(first).IsNotNull().And.IsInstanceOf<SequenceStageCallDefinition>();

            var firstCall = first as SequenceStageCallDefinition;
            Debug.Assert(firstCall is not null);

            Check.That(firstCall.CallMethodDefinition.MethodName).IsNotNull();
            Check.That(firstCall.CallMethodDefinition.MethodName).IsNotNull().And.IsEqualTo(nameof(IBasicTestVGrain.ExecuteStringAsync));

            Check.That(firstCall.Input).IsNull();
            Check.That(firstCall.Output!.ToType()).IsNotNull().And.IsEqualTo(typeof(string));

            var last = definition.Stages.LastOrDefault();

            Check.That(last).IsNotNull().And.IsNotEqualTo(first).And.IsInstanceOf<SequenceStageCallDefinition>();

            var lastCall = last as SequenceStageCallDefinition;
            Debug.Assert(lastCall is not null);

            Check.That(lastCall.CallMethodDefinition.MethodName).IsNotNull();
            Check.That(lastCall.CallMethodDefinition.MethodName).IsNotNull().And.IsEqualTo(nameof(IBasicTestOtherVGrain.OtherReturnIpAddressFromStringAsync));

            Check.That(lastCall.Input!.ToType()).IsNotNull().And.IsEqualTo(typeof(string));
            Check.That(lastCall.Output!.ToType()).IsNotNull().And.IsEqualTo(typeof(IPAddress));
        }

        /// <summary>
        /// Create a small sequence with a foreach condition definition
        /// </summary>
        [Fact]
        public void Foreach()
        {
            var definition = Sequence.Build("unit-test-foreach")
                                     .NoInput()
                                     .Use<IBasicTestVGrain>().Call((a, ctx) => a.GetUrisAsync(ctx)).Return
                                     .Foreach(IType.From<Uri>(), each =>
                                     {
                                         return each.Use<IBasicTestVGrain>().Call((a, uri, ctx) => a.GetHtmlFromUriAsync(uri!, ctx)).Return
                                                    .Use<IBasicTestOtherVGrain>().Call((a, html, ctx) => a.OtherReturnIpAddressFromStringAsync(html, ctx)).Return;
                                     })
                                     .Use<ITestStoreVGrain>().Call((a, input, ctx) => a.Storage(input, ctx)).Return
                                     .Build();

            Check.That(definition).IsNotNull();
            Check.That(definition.Input).IsNull();
            Check.That(definition.Output).IsNull();

            Check.That(definition.Stages).IsNotNull().And.CountIs(3);

            var getUris = definition.Stages.FirstOrDefault();
            var storage = definition.Stages.LastOrDefault();
            var foreachStage = definition.Stages.Skip(1).FirstOrDefault();

            // Simple check
            Check.That(getUris).IsNotNull().And.IsInstanceOf<SequenceStageCallDefinition>();

            var getUriStep = getUris as SequenceStageCallDefinition;
            Check.That(getUriStep).IsNotNull()
                                  .And
                                  .WhichMember(c => c!.CallMethodDefinition.MethodName).Verifies(s => s.Contains(nameof(IBasicTestVGrain.GetUrisAsync)));

            Check.That(storage).IsNotNull().And.IsInstanceOf<SequenceStageCallDefinition>();

            var storageStep = storage as SequenceStageCallDefinition;
            Check.That(storageStep).IsNotNull()
                                   .And
                                   .WhichMember(c => c!.CallMethodDefinition.MethodName).Verifies(s => s.Contains(nameof(ITestStoreVGrain.Storage)));

            Check.That(foreachStage).IsNotNull().And.IsInstanceOf<SequenceStageForeachDefinition>();

            var firstStep = foreachStage as SequenceStageForeachDefinition;
            Check.That(firstStep).IsNotNull()
                                 .And
                                 .WhichMember(m => m!.InnerFlow).Verifies(f => f.IsNotNull());

            var foreachFlow = firstStep?.InnerFlow;
            Check.That(foreachFlow).IsNotNull();
            Debug.Assert(foreachFlow is not null);

            Check.That(foreachFlow.Input!.ToType()).IsNotNull().And.IsEqualTo(typeof(Uri));
            Check.That(foreachFlow.Output!.ToType()).IsNotNull().And.IsEqualTo(typeof(IPAddress));

            Check.That(foreachFlow.Stages).IsNotNull().And.CountIs(2);
        }

        /// <summary>
        /// Test filter condition
        /// </summary>
        [Fact]
        public void Filter()
        {
            var inputVariable = ";";

            var definition = Sequence.Build("unit-test-filter")
                                     .NoInput()
                                     .Use<IBasicTestVGrain>().Call((a, ctx) => a.GetUrisAsync(ctx)).Return

                                     .Filter((Uri item) => !string.IsNullOrEmpty(item.OriginalString) &&
                                                            item.IsFile &&
                                                            item.IsAbsoluteUri &&
                                                            (item.OriginalString.Equals("CONST", StringComparison.OrdinalIgnoreCase) ||
                                                             item.OriginalString.Equals(inputVariable, StringComparison.OrdinalIgnoreCase)))

                                     .Build();
        }

        /// <summary>
        /// Call MUST use a method from declaring vgrain
        /// </summary>
        [Fact]
        public void InvalidCallRecord_FromAnother_VGrain()
        {
            var mockOtherVGrain = new Mock<IBasicTestOtherVGrain>().Object;

            var sequenceBuild = Sequence.Build("unit-test-invalid-call-control")
                                        .NoInput()
                                        .Use<IBasicTestVGrain>();

            Check.ThatCode(() => sequenceBuild.Call((a, ctx) => mockOtherVGrain.OtherExecuteStringAsync(ctx)))
                 .Throws<InvalidCastException>()
                 .WithProperty(p => p.Message,
                               "OtherExecuteStringAsync must be callable from Democrite.Framework.Builders.UnitTests.TestData.IBasicTestVGrain not from Democrite.Framework.Builders.UnitTests.TestData.IBasicTestOtherVGrain");
        }

        /// <summary>
        /// Call MUST use as parameter only <see cref="IExecutionContext"/> or input type
        /// </summary>
        [Fact]
        public void External_Arguments()
        {
            var mockOtherVGrain = new Mock<IBasicTestOtherVGrain>().Object;

            var sequenceBuild = Sequence.Build("unit-test-not-allow-arg")
                                        .NoInput()
                                        .Use<IBasicTestVGrain>();

            Check.ThatCode(() => sequenceBuild.Call((a, ctx) => a.ExecuteIpAddressAsync('x', ctx))).DoesNotThrow();
            //.Throws<InvalidCastException>()
            //.WithProperty(p => p.Message,
            //              "Parameter (System.Char) name 'c' must be of type Democrite.Framework.Core.Abstractions." + nameof(IExecutionContext));
        }

        /// <summary>
        /// Call MUST use as parameter only <see cref="IExecutionContext"/> or input type
        /// </summary>
        [Fact]
        public void External_Arguments_DifferentFromInput()
        {
            var mockOtherVGrain = new Mock<IBasicTestOtherVGrain>().Object;

            var sequenceBuild = Sequence.Build("unit-test-invalid-call-record-not-allow-arguments-different-from-input")
                                        .NoInput()
                                        .Use<IBasicTestVGrain>().Call((a, ctx) => a.ExecuteStringAsync(ctx)).Return;

            Check.ThatCode(() => sequenceBuild.Use<IBasicTestVGrain>().Call((a, input, ctx) => a.ExecuteIpAddressAsync('x', ctx))).DoesNotThrow();
            //.Throws<InvalidCastException>()
            //.WithProperty(p => p.Message,
            //              "Parameter (System.Char) name 'c' must be of type Democrite.Framework.Core.Abstractions." + nameof(IExecutionContext) + " or System.String");
        }

        /// <summary>
        /// Complexes the sequence serializer.
        /// </summary>
        [Fact]
        public void Complexe_Sequence_Serializer()
        {
            var definition = Sequence.Build("unit-test-complexe-sequence-serializer", metadataBuilder: m =>
                                     {
                                         m.CategoryPath("root/path/leaft")
                                          .Description("description")
                                          .AddTags("poney")
                                          .AddTags("rose");
                                     })
                                     .NoInput()
                                     .Use<IBasicTestVGrain>(m =>
                                     {
                                         m.AddTags("debug")
                                          .Description("Test serialization");

                                     }).Call((a, ctx) => a.GetUrisAsync(ctx)).Return
                                     .Foreach(IType.From<Uri>(), each =>
                                     {
                                         return each.Use<IBasicTestVGrain>().Call((a, uri, ctx) => a.GetHtmlFromUriAsync(uri!, ctx)).Return
                                                    .Use<IBasicTestOtherVGrain>().Call((a, html, ctx) => a.OtherReturnIpAddressFromStringAsync(html, ctx)).Return;
                                     }, m => m.Description("Even in foreach extension methods"))
                                     .Use<ITestStoreVGrain>().Call((a, input, ctx) => a.Storage(input, ctx)).Return
                                     .Build();

            SerializationTester.SerializeTester(definition);
        }

        /// <summary>
        /// Simple the sequence serializer.
        /// </summary>
        [Fact]
        public void Simple_Sequence_Serializer()
        {
            var definition = Sequence.Build("unit-test-simple-sequence-serializer")
                                     .NoInput()
                                     .Use<IBasicTestVGrain>().Call((a, ctx) => a.ExecuteStringAsync(ctx)).Return
                                     .Use<IBasicTestOtherVGrain>().Call((a, input, ctx) => a.OtherReturnIpAddressFromStringAsync(input, ctx)).Return
                                     .Build();

            SerializationTester.SerializeTester(definition);
        }

        /// <summary>
        /// Simple the sequence serializer.
        /// </summary>
        [Fact]
        public void Simple_Sequence_Serializer_With_Custom_Access()
        {

            var definition = Sequence.Build("unit-test-simple-sequence-serializer")
                                     .NoInput()
                                     .Use<IBasicTestVGrain>().Call((a, ctx) => a.ExecuteStringAsync(ctx)).Return
                                     // Check input.Length
                                     .Use<IBasicTestOtherVGrain>().Call((a, input, ctx) => a.OtherReturnIpAddressFromIntAsync(input.Length, ctx)).Return
                                     .Build();

            SerializationTester.SerializeTester(definition);
        }

        /// <summary>
        /// Simple the sequence serializer with input and output.
        /// </summary>
        [Fact]
        public void Simple_Sequence_Serializer_With_Input_And_Output()
        {
            var definition = Sequence.Build("unit-test-simple-sequence-serializer")
                                     .RequiredInput<Uri>()
                                     .Use<IBasicTestVGrain>().Call((a, uri, ctx) => a.GetHtmlFromUriAsync(uri!, ctx)).Return
                                     .Use<IBasicTestOtherVGrain>().Call((a, html, ctx) => a.OtherReturnIpAddressFromStringAsync(html, ctx)).Return
                                     .Build();

            SerializationTester.SerializeTester(definition);
        }

        #endregion
    }
}
