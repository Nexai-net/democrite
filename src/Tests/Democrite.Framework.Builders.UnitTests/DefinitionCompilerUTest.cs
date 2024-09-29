// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.UnitTests
{
    using AutoFixture;
    using AutoFixture.Kernel;

    using Democrite.Framework.Builders.Models;
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.References;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Streams;
    using Democrite.Framework.Core.Abstractions.Triggers;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Expressions;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Models;
    using Elvex.Toolbox.Models.Converters;

    using Microsoft.VisualStudio.TestPlatform.Utilities;

    using NFluent;

    using NSubstitute;

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    using static Democrite.Framework.Builders.UnitTests.DefinitionCompilerUTest;
    using static Democrite.Framework.Core.Abstractions.DemocriteSystemDefinitions;

    /// <summary>
    /// Test for <see cref="DefinitionCompiler"/>
    /// </summary>
    public sealed class DefinitionCompilerUTest
    {
        #region Fields

        private static readonly MethodInfo s_yaml_To_Sequence_Select_Direct_Impl_gen;
        private readonly ObjectConverter _converter;

        #endregion

        #region Nested

        internal record struct Token();

        internal class TextVisitor
        {
            public TextVisitor(string msg)
            {
                this.Msg = msg;
            }

            public string? Lang { get; }

            public string Msg { get; }

            public IReadOnlyCollection<Token>? Tokens { get; }
        }

        internal record struct TokenizeSourceInfo(string Lang);
        internal record class PatternMatchingModelInfo(Guid ModelId, string Lang);

        internal interface ITextTool : IVGrain
        {
            Task<TextVisitor> Simplify(TextVisitor textVisitor, IExecutionContext executionContext);
        }

        internal interface ITextLangDetector : IVGrain
        {
            Task<T> Lang<T>(T textVisitor, IExecutionContext executionContext);
        }

        internal interface ITextTokenizer : IVGrain
        {
            Task<TextVisitor> Tokenize(TextVisitor textVisitor, IExecutionContext<string> executionContext);
        }

        internal interface ITextPatternMatching : IVGrain
        {
            Task<TextVisitor> Tag<TCfg>(TextVisitor textVisitor, IExecutionContext<TCfg> executionContext)
                where TCfg : PatternMatchingModelInfo;
        }

        #endregion

        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        static DefinitionCompilerUTest()
        {
            Expression<Func<DefinitionCompilerUTest, Task>> eprx = e => e.Yaml_To_Sequence_Select_Direct_Impl<string>();
            s_yaml_To_Sequence_Select_Direct_Impl_gen = ((MethodCallExpression)eprx.Body).Method.GetGenericMethodDefinition();
        }

        /// <summary>
        /// 
        /// </summary>
        public DefinitionCompilerUTest()
        {
            this._converter = new ObjectConverter(new IDedicatedObjectConverter[]
            {
                new ScalarDedicatedConverter(),
                new GuidDedicatedConverter()
            });
        }

        #endregion

        #region Methods

        #region Signal

        /// <summary>
        /// Yamls to signal simple.
        /// </summary>
        [Fact]
        public async Task Yaml_To_Signal_Simple()
        {
            var yaml = """
                signals:
                    - signal : sn-1
                """;

            var solver = Substitute.For<IDemocriteReferenceSolverService>();
            var parser = new DefinitionCompiler(solver, this._converter);
            var sgnl = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(sgnl).IsNotNull();
            Check.That(sgnl.Success).IsTrue();
            Check.That(sgnl.Logs).IsNullOrEmpty();
            Check.That(sgnl.CompileOption).IsNull();
            Check.That(sgnl.Definitions).IsNotNull().And.CountIs(1);

            var def = sgnl.Definitions.First();
            Check.That(def).IsNotNull().And.IsInstanceOf<SignalDefinition>();

            var signalDef = (SignalDefinition)def;

            Check.That(signalDef.DisplayName).IsEqualTo("sn-1");
            Check.That(signalDef.Name).IsEqualTo("sn-1");
            Check.That(signalDef.MetaData).IsNull();
            Check.That(signalDef.Uid).Not.IsDefaultValue();
            Check.That(RefIdHelper.IsRefId(signalDef.RefId)).IsTrue();
        }

        /// <summary>
        /// Yamls to signal simple.
        /// </summary>
        [Fact]
        public async Task Yaml_To_Signal_Full()
        {
            var fixture = new Fixture();

            var test_id = new Guid("A7543238-15A0-4C1A-AEF3-ACD48511A16A");
            var description = fixture.Create<string>() + " && " + fixture.Create<string>();
            var namespaceTest = "bag.sgnl";
            var category_path = "root/sub/lief";

            var yaml = """
                signals:
                    - signal : sn-1
                    - signal : sn-2
                      uid: <<uid>>
                      meta-data:
                        description: "<<description>>"
                        namespace: <<namespace>>
                        category: <<category_path>>
                        tags:
                            - tg1
                            - tg2
                            - tg3
                """.Replace("<<description>>", description)
                   .Replace("<<namespace>>", namespaceTest)
                   .Replace("<<category_path>>", category_path)
                   .Replace("<<uid>>", test_id.ToString());

            var solver = Substitute.For<IDemocriteReferenceSolverService>();
            var parser = new DefinitionCompiler(solver, this._converter);

            var sgnl = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(sgnl).IsNotNull();
            Check.That(sgnl.Success).IsTrue();
            Check.That(sgnl.Logs).IsNullOrEmpty();
            Check.That(sgnl.CompileOption).IsNull();
            Check.That(sgnl.Definitions).IsNotNull().And.CountIs(2);

            var def = sgnl.Definitions.First();
            Check.That(def).IsNotNull().And.IsInstanceOf<SignalDefinition>();

            var signalDef = (SignalDefinition)def;

            Check.That(signalDef.DisplayName).IsEqualTo("sn-1");
            Check.That(signalDef.Name).IsEqualTo("sn-1");
            Check.That(signalDef.MetaData).IsNull();
            Check.That(signalDef.Uid).Not.IsDefaultValue();
            Check.That(RefIdHelper.IsRefId(signalDef.RefId)).IsTrue();

            var cmpl = sgnl.Definitions.Last();
            Check.That(cmpl).IsNotNull().And.IsInstanceOf<SignalDefinition>();

            var signalOtherDef = (SignalDefinition)cmpl;

            Check.That(signalOtherDef.DisplayName).IsEqualTo("sn-2");
            Check.That(signalOtherDef.Name).IsEqualTo("sn-2");

            Check.That(signalOtherDef.MetaData).IsNotNull();
            Check.That(signalOtherDef.MetaData!.NamespaceIdentifier).IsNotNull().And.IsEqualTo(namespaceTest);
            Check.That(signalOtherDef.MetaData.CategoryPath).IsNotNull().And.IsEqualTo(category_path);
            Check.That(signalOtherDef.MetaData.Description).IsNotNull().And.IsEqualTo(description);
            Check.That(signalOtherDef.MetaData.Tags).IsNotNull().And.ContainsExactly(["tg1", "tg2", "tg3"]);

            Check.That(signalOtherDef.Uid).Not.IsDefaultValue().And.IsEqualTo(test_id);
            Check.That(RefIdHelper.IsRefId(signalOtherDef.RefId)).IsTrue();
            RefIdHelper.Explode(signalOtherDef.RefId, out var type, out var np, out var sni);

            Check.That(type).IsEqualTo(RefTypeEnum.Signal);
            Check.That(np).IsEqualTo(namespaceTest);
            Check.That(sni).IsEqualTo("sn-2");

        }

        #endregion

        #region Stream

        /// <summary>
        /// Yamls to triggers simple.
        /// </summary>
        [Fact]
        public async Task Yaml_To_Stream_Simple()
        {
            var fixture = new Fixture();

            var queueName = fixture.Create<string>();

            var yaml = """
                streams:
                    - stream : strm-1
                      queue-namespace : txt
                      queue-name : <<queue-name>>
                """.Replace("<<queue-name>>", queueName);

            var solver = Substitute.For<IDemocriteReferenceSolverService>();
            var parser = new DefinitionCompiler(solver, this._converter);
            var stream = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(stream).IsNotNull();
            Check.That(stream.Success).IsTrue();
            Check.That(stream.Logs).IsNullOrEmpty();
            Check.That(stream.CompileOption).IsNull();
            Check.That(stream.Definitions).IsNotNull().And.CountIs(1);

            var def = stream.Definitions.First();
            Check.That(def).IsNotNull().And.IsInstanceOf<StreamQueueDefinition>();

            var streamQueueDef = (StreamQueueDefinition)def;

            Check.That(streamQueueDef.DisplayName).IsEqualTo("DemocriteClusterStream/txt+" + queueName);
            Check.That(streamQueueDef.StreamNamespace).IsEqualTo("txt");
            Check.That(streamQueueDef.StreamKey).IsEqualTo(queueName);
            Check.That(streamQueueDef.MetaData).IsNull();
            Check.That(streamQueueDef.Uid).Not.IsDefaultValue();
            Check.That(RefIdHelper.IsRefId(streamQueueDef.RefId)).IsTrue();
        }

        /// <summary>
        /// Yamls to triggers simple.
        /// </summary>
        [Fact]
        public async Task Yaml_To_Stream_Full()
        {
            var fixture = new Fixture();

            var queueName = fixture.Create<string>();
            var queueName2 = fixture.Create<string>();

            var test_id = new Guid("A7543238-15A0-4C1A-AEF3-ACD48511A16A");
            var description = fixture.Create<string>() + " && " + fixture.Create<string>();
            var namespaceTest = "bag.stream";
            var category_path = "root/sub/lief";

            var yaml = """
                streams:
                    - stream : strm-1
                      queue-namespace : txt
                      queue-name : <<queue-name>>

                    - stream : strm-2
                      queue-namespace : txt
                      queue-name : <<queue-name-2>>
                      uid: <<uid>>
                      meta-data:
                        description: "<<description>>"
                        namespace: <<namespace>>
                        category: <<category_path>>
                        tags:
                            - tg1
                            - tg2
                            - tg3
                """.Replace("<<queue-name>>", queueName)
                   .Replace("<<queue-name-2>>", queueName2)
                   .Replace("<<description>>", description)
                   .Replace("<<namespace>>", namespaceTest)
                   .Replace("<<category_path>>", category_path)
                   .Replace("<<uid>>", test_id.ToString());

            var solver = Substitute.For<IDemocriteReferenceSolverService>();
            var parser = new DefinitionCompiler(solver, this._converter);
            var stream = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(stream).IsNotNull();
            Check.That(stream.Success).IsTrue();
            Check.That(stream.Logs).IsNullOrEmpty();
            Check.That(stream.CompileOption).IsNull();
            Check.That(stream.Definitions).IsNotNull().And.CountIs(2);

            var def = stream.Definitions.First();
            Check.That(def).IsNotNull().And.IsInstanceOf<StreamQueueDefinition>();

            var streamQueueDef = (StreamQueueDefinition)def;

            Check.That(streamQueueDef.DisplayName).IsEqualTo("DemocriteClusterStream/txt+" + queueName);
            Check.That(streamQueueDef.StreamNamespace).IsEqualTo("txt");
            Check.That(streamQueueDef.StreamKey).IsEqualTo(queueName);
            Check.That(streamQueueDef.MetaData).IsNull();
            Check.That(streamQueueDef.Uid).Not.IsDefaultValue();
            Check.That(RefIdHelper.IsRefId(streamQueueDef.RefId)).IsTrue();

            var cmpl = stream.Definitions.Last();
            Check.That(cmpl).IsNotNull().And.IsInstanceOf<StreamQueueDefinition>();

            var otherStreamDef = (StreamQueueDefinition)cmpl;

            Check.That(otherStreamDef.DisplayName).IsEqualTo("DemocriteClusterStream/txt+" + queueName2);
            Check.That(otherStreamDef.StreamNamespace).IsEqualTo("txt");
            Check.That(otherStreamDef.StreamKey).IsEqualTo(queueName2);

            Check.That(otherStreamDef.MetaData).IsNotNull();
            Check.That(otherStreamDef.MetaData!.NamespaceIdentifier).IsNotNull().And.IsEqualTo(namespaceTest);
            Check.That(otherStreamDef.MetaData.CategoryPath).IsNotNull().And.IsEqualTo(category_path);
            Check.That(otherStreamDef.MetaData.Description).IsNotNull().And.IsEqualTo(description);
            Check.That(otherStreamDef.MetaData.Tags).IsNotNull().And.ContainsExactly(["tg1", "tg2", "tg3"]);

            Check.That(otherStreamDef.Uid).Not.IsDefaultValue().And.IsEqualTo(test_id);
            Check.That(RefIdHelper.IsRefId(otherStreamDef.RefId)).IsTrue();
            RefIdHelper.Explode(otherStreamDef.RefId, out var type, out var np, out var sni);

            Check.That(type).IsEqualTo(RefTypeEnum.StreamQueue);
            Check.That(np).IsEqualTo(namespaceTest);
            Check.That(sni).IsEqualTo("strm-2");
        }

        #endregion

        #region Sequence

        #region Select

        /// <summary>
        /// Convert triggers with only select stage
        /// </summary>
        [Fact]
        public async Task Yaml_To_Sequence_Select_Build()
        {
            var yaml = """
                sequences:
                  - sequence: tokenize-text
                    required-input: system@string
                    stages:
                      - select: bag.text@text-visitor
                        mode: build
                        from:
                            - input
                """;

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var sequence = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(sequence).IsNotNull();
            Check.That(sequence.Success).IsTrue();
            Check.That(sequence.Logs).IsNullOrEmpty();
            Check.That(sequence.CompileOption).IsNull();
            Check.That(sequence.Definitions).IsNotNull().And.CountIs(1);

            var def = sequence.Definitions.First() as SequenceDefinition;

            Check.That(def).IsNotNull();
            Check.That(def!.Stages).IsNotNull().And.CountIs(1);
            Check.That(def.Input).IsNotNull().And.IsEqualTo(typeof(string).GetAbstractType());
            Check.That(def.Output).IsNotNull().And.IsEqualTo(typeof(TextVisitor).GetAbstractType());

            var initSelect = CheckStage<SequenceStageSelectDefinition, string, TextVisitor>(def.Stages.First());

            Check.That(initSelect.SelectAccess).IsNotNull();
            Check.That(initSelect.SelectAccess.ChainCall).IsNull();
            Check.That(initSelect.SelectAccess.DirectObject).IsNull();
            Check.That(initSelect.SelectAccess.MemberInit).IsNotNull();

            var memberInit = initSelect.SelectAccess.MemberInit;
            Check.That(memberInit!.Inputs).IsNotNull().And.CountIs(1);
            var input = memberInit.Inputs.First();
            Check.That(input).IsNotNull().And.IsEqualTo(typeof(string).GetAbstractType());

            Check.That(memberInit.Bindings).IsNotNull().And.CountIs(1);
            var binding = memberInit.Bindings.First();
            Check.That(binding.IsCtorParameter).IsTrue();
            Check.That(memberInit.Ctor).IsNotNull();

            Check.That(initSelect.SelectAccess.TargetType).IsNotNull().And.IsEqualTo(typeof(TextVisitor).GetAbstractType());
        }

        /// <summary>
        /// Convert triggers with only select stage
        /// </summary>
        [Fact]
        public async Task Yaml_To_Sequence_Select_ChainPath()
        {
            var yaml = """
                sequences:
                  - sequence: tokenize-text
                    required-input: bag.text@text-visitor
                    stages:
                      - select: 
                        from:
                            - input.Tokens
                """;

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var sequence = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(sequence).IsNotNull();
            Check.That(sequence.Success).IsTrue();
            Check.That(sequence.Logs).IsNullOrEmpty();
            Check.That(sequence.CompileOption).IsNull();
            Check.That(sequence.Definitions).IsNotNull().And.CountIs(1);

            var def = sequence.Definitions.First() as SequenceDefinition;

            Check.That(def).IsNotNull();
            Check.That(def!.Stages).IsNotNull().And.CountIs(1);
            Check.That(def.Input).IsNotNull().And.IsEqualTo(typeof(TextVisitor).GetAbstractType());
            Check.That(def.Output).IsNotNull().And.IsEqualTo(typeof(IReadOnlyCollection<Token>).GetAbstractType());

            var initSelect = CheckStage<SequenceStageSelectDefinition, TextVisitor, IReadOnlyCollection<Token>>(def.Stages.First());

            Check.That(initSelect.SelectAccess).IsNotNull();
            Check.That(initSelect.SelectAccess.ChainCall).IsNotNull().And.IsEqualTo("input.Tokens");
            Check.That(initSelect.SelectAccess.DirectObject).IsNull();
            Check.That(initSelect.SelectAccess.MemberInit).IsNull();
            Check.That(initSelect.SelectAccess.TargetType).IsNotNull().And.IsEqualTo(typeof(IReadOnlyCollection<Token>).GetAbstractType());
        }

        /// <summary>
        /// Convert triggers with only select stage
        /// </summary>
        [Fact]
        public async Task Yaml_To_Sequence_Select_Build_From_Direct()
        {
            var fixture = new Fixture();

            var inputMsg = "hello world ahahahah " + fixture.Create<string>();

            var yaml = """
                sequences:
                  - sequence: tokenize-text
                    stages:
                      - select: bag.text@text-visitor
                        from: 
                            - <<FIX_STRING_VALUE>>
                """.Replace("<<FIX_STRING_VALUE>>", inputMsg);

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var sequence = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(sequence).IsNotNull();
            Check.That(sequence.Success).IsTrue();
            Check.That(sequence.Logs).IsNullOrEmpty();
            Check.That(sequence.CompileOption).IsNull();
            Check.That(sequence.Definitions).IsNotNull().And.CountIs(1);

            var def = sequence.Definitions.First() as SequenceDefinition;

            Check.That(def).IsNotNull();
            Check.That(def!.Stages).IsNotNull().And.CountIs(1);
            Check.That(def.Input).IsNull();
            Check.That(def.Output).IsNotNull().And.IsEqualTo(typeof(TextVisitor).GetAbstractType());

            var initSelect = CheckStage<SequenceStageSelectDefinition, NoneType, TextVisitor>(def.Stages.First());

            Check.That(initSelect.SelectAccess).IsNotNull();
            Check.That(initSelect.SelectAccess.ChainCall).IsNull();
            Check.That(initSelect.SelectAccess.DirectObject).IsNull();
            Check.That(initSelect.SelectAccess.MemberInit).IsNotNull();
            Check.That(initSelect.SelectAccess.TargetType).IsNotNull().And.IsEqualTo(typeof(TextVisitor).GetAbstractType());
        }

        /// <summary>
        /// Convert triggers with only select stage
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(double))]
        public async Task Yaml_To_Sequence_Select_Direct(Type paramType)
        {
            var gen = s_yaml_To_Sequence_Select_Direct_Impl_gen;

            var tsk = (Task)gen.MakeGenericMethod(paramType).Invoke(this, new object[0])!;
            await tsk;
        }

        private async Task Yaml_To_Sequence_Select_Direct_Impl<TParamType>()
        {
            var fixture = new Fixture();

            var paramType = typeof(TParamType);

            var input = fixture.Create(paramType, new SpecimenContext(fixture));

            if (input is string)
                input = "EXtend test to prevent guid to be used " + input;

            var yaml = """
                sequences:
                  - sequence: tokenize-text
                    stages:
                      - select: 
                        from: 
                            - <<FIX_STRING_VALUE>>
                """.Replace("<<FIX_STRING_VALUE>>", input.ToString());

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var sequence = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(sequence).IsNotNull();
            Check.That(sequence.Success).IsTrue();
            Check.That(sequence.Logs).IsNullOrEmpty();
            Check.That(sequence.CompileOption).IsNull();
            Check.That(sequence.Definitions).IsNotNull().And.CountIs(1);

            var def = sequence.Definitions.First() as SequenceDefinition;

            Check.That(def).IsNotNull();
            Check.That(def!.Stages).IsNotNull().And.CountIs(1);
            Check.That(def.Input).IsNull();
            Check.That(def.Output).IsNotNull().And.IsEqualTo(paramType.GetAbstractType());

            var initSelect = CheckStage<SequenceStageSelectDefinition, NoneType, TParamType>(def.Stages.First());

            Check.That(initSelect.SelectAccess).IsNotNull();
            Check.That(initSelect.SelectAccess.ChainCall).IsNull();
            Check.That(initSelect.SelectAccess.DirectObject).IsNotNull().And.IsEqualTo(TypedArgument.From(input, paramType));
            Check.That(initSelect.SelectAccess.MemberInit).IsNull();
            Check.That(initSelect.SelectAccess.TargetType).IsNotNull().And.IsEqualTo(typeof(TParamType).GetAbstractType());
        }

        #endregion

        #region Use

        /// <summary>
        /// Test creating simple Use stage without any context or generic
        /// </summary>
        [Fact]
        public async Task Yaml_To_Sequence_Use_Simple()
        {
            var yaml = """
                sequences:
                  - sequence: tokenize-text
                    required-input: bag.text@text-visitor
                    stages:
                      - use: bag.text@toolbox
                        call: simplify
                """;

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var sequence = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(sequence).IsNotNull();
            Check.That(sequence.Success).IsTrue();
            Check.That(sequence.Logs).IsNullOrEmpty();
            Check.That(sequence.CompileOption).IsNull();
            Check.That(sequence.Definitions).IsNotNull().And.CountIs(1);

            var def = sequence.Definitions.First() as SequenceDefinition;

            Check.That(def).IsNotNull();
            Check.That(def!.Stages).IsNotNull().And.CountIs(1);
            Check.That(def.Input).IsNotNull().And.IsEqualTo(typeof(TextVisitor).GetAbstractType());
            Check.That(def.Output).IsNotNull().And.IsEqualTo(typeof(TextVisitor).GetAbstractType());

            var call = CheckStage<SequenceStageCallDefinition, TextVisitor, TextVisitor>(def.Stages.First());

            // Simplify
            CheckCallStage<ITextTool>(call, t => t.Simplify(null!, null!));
        }

        /// <summary>
        /// Test creating simple Use stage with a generic
        /// </summary>
        [Fact]
        public async Task Yaml_To_Sequence_Use_Simple_Generic_Method()
        {
            var yaml = """
                sequences:
                  - sequence: tokenize-text
                    required-input: bag.text@text-visitor
                    stages:
                      - use: bag.text@language-detector
                        call: lang
                """;

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var sequence = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(sequence).IsNotNull();
            Check.That(sequence.Success).IsTrue();
            Check.That(sequence.Logs).IsNullOrEmpty();
            Check.That(sequence.CompileOption).IsNull();
            Check.That(sequence.Definitions).IsNotNull().And.CountIs(1);

            var def = sequence.Definitions.First() as SequenceDefinition;

            Check.That(def).IsNotNull();
            Check.That(def!.Stages).IsNotNull().And.CountIs(1);
            Check.That(def.Input).IsNotNull().And.IsEqualTo(typeof(TextVisitor).GetAbstractType());
            Check.That(def.Output).IsNotNull().And.IsEqualTo(typeof(TextVisitor).GetAbstractType());

            var call = CheckStage<SequenceStageCallDefinition, TextVisitor, TextVisitor>(def.Stages.First());

            // Lang Detect
            CheckCallStage<ITextLangDetector>(call, t => t.Lang<TextVisitor>(null!, null!));
        }

        /// <summary>
        /// Test creating simple Use stage with a simple string context
        /// </summary>
        [Fact]
        public async Task Yaml_To_Sequence_Use_Simple_Context_String()
        {
            var yaml = """
                sequences:
                  - sequence: tokenize-text
                    required-input: bag.text@text-visitor
                    stages:
                      - use: bag.text@tokenizer
                        call: tokenize
                        config:
                          from: 
                            - input.lang
                """;

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var sequence = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(sequence).IsNotNull();
            Check.That(sequence.Success).IsTrue();
            Check.That(sequence.Logs).IsNullOrEmpty();
            Check.That(sequence.CompileOption).IsNull();
            Check.That(sequence.Definitions).IsNotNull().And.CountIs(1);

            var def = sequence.Definitions.First() as SequenceDefinition;

            Check.That(def).IsNotNull();
            Check.That(def!.Stages).IsNotNull().And.CountIs(1);
            Check.That(def.Input).IsNotNull().And.IsEqualTo(typeof(TextVisitor).GetAbstractType());
            Check.That(def.Output).IsNotNull().And.IsEqualTo(typeof(TextVisitor).GetAbstractType());

            var call = CheckStage<SequenceStageCallDefinition, TextVisitor, TextVisitor>(def.Stages.First());

            // Tokenize
            CheckCallStage<ITextTokenizer>(call, t => t.Tokenize(null!, null!), (cfg, from) =>
            {
                Check.That(cfg).IsNotNull();
                Check.That(cfg!.ChainCall).IsNotNull().And.IsEqualTo("input.Lang");
                Check.That(cfg.DirectObject).IsNull();
                Check.That(cfg.MemberInit).IsNull();
                Check.That(cfg.TargetType).IsNotNull().And.IsEqualTo(typeof(string).GetAbstractType());

                Check.That(from).IsNull();
            });
        }

        /// <summary>
        /// Test creating simple Use stage with complex context
        /// </summary>
        [Fact]
        public async Task Yaml_To_Sequence_Use_Simple_Context_Complex_And_Generic()
        {
            var yaml = """
                sequences:
                  - sequence: tokenize-text
                    required-input: bag.text@text-visitor
                    stages:
                      - use: bag.text@pattern-matching
                        call: tag
                        config:
                          type: bag.text@pattern-matching-model
                          from:
                            - 1b2e1a88-ae82-45bf-bd79-1f99c3b52401
                            - input.lang
                """;

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var sequence = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(sequence).IsNotNull();
            Check.That(sequence.Success).IsTrue();
            Check.That(sequence.Logs).IsNullOrEmpty();
            Check.That(sequence.CompileOption).IsNull();
            Check.That(sequence.Definitions).IsNotNull().And.CountIs(1);

            var def = sequence.Definitions.First() as SequenceDefinition;

            Check.That(def).IsNotNull();
            Check.That(def!.Stages).IsNotNull().And.CountIs(1);
            Check.That(def.Input).IsNotNull().And.IsEqualTo(typeof(TextVisitor).GetAbstractType());
            Check.That(def.Output).IsNotNull().And.IsEqualTo(typeof(TextVisitor).GetAbstractType());

            var call = CheckStage<SequenceStageCallDefinition, TextVisitor, TextVisitor>(def.Stages.First());

            // Pattern-Matching
            CheckCallStage<ITextPatternMatching>(call, t => t.Tag<PatternMatchingModelInfo>(null!, null!), (cfg, from) =>
            {
                Check.That(cfg).IsNotNull();
                Check.That(cfg!.ChainCall).IsNull();
                Check.That(cfg.DirectObject).IsNull();
                Check.That(cfg.MemberInit).IsNotNull();
                Check.That(cfg.TargetType).IsNotNull().And.IsEqualTo(typeof(PatternMatchingModelInfo).GetAbstractType());
                Check.That(from).IsNull();
            });
        }

        #endregion

        /// <summary>
        /// Yamls to triggers simple.
        /// </summary>
        [Fact]
        public async Task Yaml_To_Sequence_Full()
        {
            var fixture = new Fixture();

            var uid = fixture.Create<Guid>();

            var yaml = """
                sequences:
                  - sequence: tokenize-text
                    uid: <<uid>>
                    meta-data:
                      description: transform a text into NLP tokens
                      tags:
                        - NLP
                        - text
                        - token
                
                    required-input: system@string
                    stages:
                      - select: bag.text@text-visitor
                        mode: build
                        from: 
                            - input
                
                      - use: bag.text@toolbox
                        call: simplify
                
                      - use: bag.text@language-detector
                        call: lang
                
                      - use: bag.text@tokenizer
                        call: tokenize
                        config:
                          from: 
                            - input.lang
                
                      - use: bag.text@pattern-matching
                        call: tag
                        config:
                          type: bag.text@pattern-matching-model
                          from:
                            - 1b2e1a88-ae82-45bf-bd79-1f99c3b52401
                            - input.lang
                
                      - select:
                        from: 
                          - input.Tokens
                """.Replace("<<uid>>", uid.ToString());

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var sequence = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(sequence).IsNotNull();
            Check.That(sequence.Success).IsTrue();
            Check.That(sequence.Logs).IsNullOrEmpty();
            Check.That(sequence.CompileOption).IsNull();
            Check.That(sequence.Definitions).IsNotNull().And.CountIs(1);

            var def = sequence.Definitions.First();
            Check.That(def).IsNotNull().And.IsInstanceOf<SequenceDefinition>();

            var sequenceDef = (SequenceDefinition)def;

            Check.That(sequenceDef.Input).IsNotNull().And.IsInstanceOf<ConcretType>().And.IsEqualTo(typeof(string).GetAbstractType());
            Check.That(sequenceDef.Output).IsNotNull().And.IsInstanceOf<CollectionType>().And.IsEqualTo(typeof(IReadOnlyCollection<Token>).GetAbstractType());
            Check.That(sequenceDef.Stages).IsNotNull().And.CountIs(6).And.ContainsNoNull();

            var stages = sequenceDef.Stages.ToArray();

            var initSelect = CheckStage<SequenceStageSelectDefinition, string, TextVisitor>(stages[0]);
            var simplifyCall = CheckStage<SequenceStageCallDefinition, TextVisitor, TextVisitor>(stages[1]);
            var langDetector = CheckStage<SequenceStageCallDefinition, TextVisitor, TextVisitor>(stages[2]);
            var tokenize = CheckStage<SequenceStageCallDefinition, TextVisitor, TextVisitor>(stages[3]);
            var patternMatching = CheckStage<SequenceStageCallDefinition, TextVisitor, TextVisitor>(stages[4]);
            var selectResult = CheckStage<SequenceStageSelectDefinition, TextVisitor, IReadOnlyCollection<Token>>(stages[5]);

            // init
            Check.That(initSelect.SelectAccess).IsNotNull();
            Check.That(initSelect.SelectAccess.ChainCall).IsNull();
            Check.That(initSelect.SelectAccess.DirectObject).IsNull();
            Check.That(initSelect.SelectAccess.MemberInit).IsNotNull();
            Check.That(initSelect.SelectAccess.TargetType).IsNotNull().And.IsEqualTo(typeof(TextVisitor).GetAbstractType());

            // Simplify
            CheckCallStage<ITextTool>(simplifyCall, t => t.Simplify(null!, null!));

            // Language
            CheckCallStage<ITextLangDetector>(langDetector, t => t.Lang<TextVisitor>(null!, null!));

            // Tokenize
            CheckCallStage<ITextTokenizer>(tokenize, t => t.Tokenize(null!, null!), (cfg, from) =>
            {
                Check.That(cfg).IsNotNull();
                Check.That(cfg!.ChainCall).IsNotNull().And.IsEqualTo("input.Lang");
                Check.That(cfg.DirectObject).IsNull();
                Check.That(cfg.MemberInit).IsNull();
                Check.That(cfg.TargetType).IsNotNull().And.IsEqualTo(typeof(string).GetAbstractType());

                Check.That(from).IsNull();
            });

            // Pattern-Matching
            CheckCallStage<ITextPatternMatching>(patternMatching, t => t.Tag<PatternMatchingModelInfo>(null!, null!), (cfg, from) =>
            {
                Check.That(cfg).IsNotNull();
                Check.That(cfg!.ChainCall).IsNull();
                Check.That(cfg.DirectObject).IsNull();
                Check.That(cfg.MemberInit).IsNotNull();
                Check.That(cfg.TargetType).IsNotNull().And.IsEqualTo(typeof(PatternMatchingModelInfo).GetAbstractType());

                Check.That(from).IsNull();
            });

            // Result
            Check.That(selectResult.SelectAccess).IsNotNull();
            Check.That(selectResult.SelectAccess.ChainCall).IsNotNull().And.IsEqualTo("input.Tokens");
            Check.That(selectResult.SelectAccess.DirectObject).IsNull();
            Check.That(selectResult.SelectAccess.MemberInit).IsNull();
            Check.That(selectResult.SelectAccess.TargetType).IsNotNull().And.IsEqualTo(typeof(IReadOnlyCollection<Token>).GetAbstractType());

        }

        #endregion

        #region Triggers

        /// <summary>
        /// Yaml create Cron trigger with static guid target
        /// </summary>
        [Fact]
        public async Task Yaml_To_Trigger_Cron_Target_Sequence_Guid()
        {
            var cron = "*/2 * 10 */fri *";
            var seqUid = Guid.NewGuid();

            var yaml = """
                    triggers:
                        - cron : every-fri-10-h-2sec
                          period: '<<CRON>>'
                          targets:
                            sequences:
                                - <<SEQ_CALL>>
                    """.Replace("<<CRON>>", cron)
                       .Replace("<<SEQ_CALL>>", seqUid.ToString());

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var triggers = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(triggers).IsNotNull();
            Check.That(triggers.Success).IsTrue();
            Check.That(triggers.Logs).IsNullOrEmpty();
            Check.That(triggers.CompileOption).IsNull();
            Check.That(triggers.Definitions).IsNotNull().And.CountIs(1);

            var trigger = triggers.Definitions.First() as CronTriggerDefinition;

            Check.That(trigger).IsNotNull();
            Check.That(trigger!.CronExpression).IsNotNull().And.IsEqualTo(cron);
            Check.That(trigger!.Targets).IsNotNull().And.CountIs(1);

            var seq = trigger.Targets.First();
            Check.That(seq).IsNotNull();
            Check.That(seq.Type).IsEqualTo(TargetTypeEnum.Sequence);
            Check.That(seq.Uid).IsEqualTo(seqUid);
        }

        /// <summary>
        /// Yaml create Cron trigger with static guid target
        /// </summary>
        [Fact]
        public async Task Yaml_To_Trigger_Cron_Target_Sequence_Guid_Static_Output()
        {
            var cron = "*/2 * 10 */fri *";
            var seqUid = Guid.NewGuid();

            var modes = Enum.GetValues<PullModeEnum>()
                            .Where(m => m != PullModeEnum.None)
                            .ToArray();

            var mode = modes.ElementAt(Random.Shared.Next(0, modes.Length));

            var val1 = Guid.NewGuid();
            var val2 = Guid.NewGuid();

            var yaml = """
                    triggers:
                        - cron : every-fri-10-h-2sec
                          period: '<<CRON>>'
                          output:
                            static:
                                type: system@guid
                                mode: <<MODE>>
                                values:
                                    - <<VAL1>>
                                    - <<VAL2>>
                                    
                          targets:
                            sequences:
                                - <<SEQ_CALL>>
                    """.Replace("<<CRON>>", cron)
                       .Replace("<<SEQ_CALL>>", seqUid.ToString())
                       .Replace("<<VAL1>>", val1.ToString())
                       .Replace("<<VAL2>>", val2.ToString())
                       .Replace("<<MODE>>", mode.ToString());

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var triggers = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(triggers).IsNotNull();
            Check.That(triggers.Success).IsTrue();
            Check.That(triggers.Logs).IsNullOrEmpty();
            Check.That(triggers.CompileOption).IsNull();
            Check.That(triggers.Definitions).IsNotNull().And.CountIs(1);

            var trigger = triggers.Definitions.First() as CronTriggerDefinition;

            Check.That(trigger).IsNotNull();
            Check.That(trigger!.CronExpression).IsNotNull().And.IsEqualTo(cron);
            Check.That(trigger!.Targets).IsNotNull().And.CountIs(1);

            Check.That(trigger.TriggerGlobalOutputDefinition).IsNotNull().And.IsInstanceOf<DataSourceStaticCollectionDefinition<Guid>>();

            var output = trigger.TriggerGlobalOutputDefinition as DataSourceStaticCollectionDefinition<Guid>;

            Check.That(output).IsNotNull();
            Check.That(output!.PullMode).IsEqualTo(mode);
            Check.That(output.Collection).IsNotNull()
                                         .And
                                         .CountIs(2)
                                         .And
                                         .ContainsExactly(val1, val2);

            var seq = trigger.Targets.First();
            Check.That(seq).IsNotNull();
            Check.That(seq.Type).IsEqualTo(TargetTypeEnum.Sequence);
            Check.That(seq.Uid).IsEqualTo(seqUid);
        }

        /// <summary>
        /// Yaml create Cron trigger with static guid target
        /// </summary>
        [Fact]
        public async Task Yaml_To_Trigger_Cron_Target_Sequence_With_Target_Ref()
        {
            var cron = "*/2 * 10 */fri *";
            var seqUid = Guid.NewGuid();
            var externalSeqUid = Guid.NewGuid();

            var yaml = """
                    signals:
                        - signal: signal-define-in-ctx

                    triggers:
                        - cron : every-fri-10-h-2sec
                          period: '<<CRON>>'
                          targets:
                            signals:
                                - ref/signal-define-in-ctx
                            sequences:
                                - <<SEQ_CALL>>
                            streams:
                                - ref/bag.toolbox@external-text-stream

                    """.Replace("<<CRON>>", cron)
                       .Replace("<<SEQ_CALL>>", seqUid.ToString());

            var solver = GenerateTestSolver();

            solver.GetReferenceDefinitionsAsync(Arg.Any<Uri>()).Returns(info =>
            {
                var refId = info[0] as Uri;

                Check.That(refId).IsNotNull();

                var definitions = new List<IDefinition>();

                RefIdHelper.Explode(refId!, out var type, out var ns, out var sni);

                if (type == RefTypeEnum.StreamQueue && ns == "bag.toolbox" && sni == "external-text-stream")
                {
                    var strDef = Substitute.For<IDefinition>();
                    strDef.Uid.Returns(externalSeqUid);

                    definitions.Add(strDef);
                }

                if (definitions.Any())
                    return definitions;

                throw new NotSupportedException("Definition Ref not founded " + refId);
            });

            var parser = new DefinitionCompiler(solver, this._converter);
            var triggers = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(triggers).IsNotNull();
            Check.That(triggers.Success).IsTrue();
            Check.That(triggers.Logs).IsNullOrEmpty();
            Check.That(triggers.CompileOption).IsNull();
            Check.That(triggers.Definitions).IsNotNull().And.CountIs(2);

            var signal = triggers.Definitions.OfType<SignalDefinition>().FirstOrDefault();
            var trigger = triggers.Definitions.OfType<CronTriggerDefinition>().FirstOrDefault();

            Check.That(signal).IsNotNull();
            Check.That(signal!.Uid).Not.IsDefaultValue();

            Check.That(trigger).IsNotNull();
            Check.That(trigger!.CronExpression).IsNotNull().And.IsEqualTo(cron);
            Check.That(trigger!.Targets).IsNotNull().And.CountIs(3);

            var targets = trigger.Targets.ToDictionary(k => k.Type);
         
            var sgnl = targets.TryGetValueInline(TargetTypeEnum.Signal, out _);
            var strm = targets.TryGetValueInline(TargetTypeEnum.Stream, out _);
            var seq = targets.TryGetValueInline(TargetTypeEnum.Sequence, out _);

            Check.That(sgnl).IsNotNull();
            Check.That(sgnl!.Uid).IsEqualTo(signal.Uid);

            Check.That(strm).IsNotNull();
            Check.That(strm!.Uid).IsEqualTo(externalSeqUid);

            Check.That(seq).IsNotNull();
            Check.That(seq!.Uid).IsEqualTo(seqUid);
        }

        /// <summary>
        /// Yaml create Cron trigger with static guid target
        /// </summary>
        [Fact]
        public async Task Yaml_To_Trigger_Signal_Target_Sequence_Guid()
        {
            var signalId = Guid.NewGuid();
            var seqUid = Guid.NewGuid();

            var yaml = """
                    triggers:
                        - signal : from-guid-signal
                          from: <<SIGNAL>>
                          targets:
                            sequences:
                                - <<SEQ_CALL>>
                    """.Replace("<<SIGNAL>>", signalId.ToString())
                       .Replace("<<SEQ_CALL>>", seqUid.ToString());

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var triggers = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(triggers).IsNotNull();
            Check.That(triggers.Success).IsTrue();
            Check.That(triggers.Logs).IsNullOrEmpty();
            Check.That(triggers.CompileOption).IsNull();
            Check.That(triggers.Definitions).IsNotNull().And.CountIs(1);

            var trigger = triggers.Definitions.First() as SignalTriggerDefinition;

            Check.That(trigger).IsNotNull();
            Check.That(trigger!.ListenSignal).IsNotNull();
            Check.That(trigger!.ListenSignal!.Value.Uid).IsEqualTo(signalId);
            Check.That(trigger!.Targets).IsNotNull().And.CountIs(1);

            var seq = trigger.Targets.First();
            Check.That(seq).IsNotNull();
            Check.That(seq.Type).IsEqualTo(TargetTypeEnum.Sequence);
            Check.That(seq.Uid).IsEqualTo(seqUid);
        }

        /// <summary>
        /// Yaml create Cron trigger with static guid target
        /// </summary>
        [Fact]
        public async Task Yaml_To_Trigger_Signal_Target_Sequence_Guid_By_Ref()
        {
            var seqUid = Guid.NewGuid();

            var yaml = """
                    global:
                        namespace: bag.utest

                    signals:
                        - signal: target-signal
                        - signal: target-signal
                          meta-data:
                            namespace: other.ns

                    triggers:
                        - signal : from-guid-signal
                          from: ref/bag.utest@target-signal
                          targets:
                            sequences:
                                - <<SEQ_CALL>>
                    """.Replace("<<SEQ_CALL>>", seqUid.ToString());

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var triggers = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(triggers).IsNotNull();
            Check.That(triggers.Success).IsTrue();
            Check.That(triggers.Logs).IsNullOrEmpty();
            Check.That(triggers.CompileOption).IsNull();
            Check.That(triggers.Definitions).IsNotNull().And.CountIs(3);

            var signals = triggers.Definitions.OfType<SignalDefinition>().ToArray();

            var signal = signals.Single(s => s.MetaData is not null && s.MetaData.NamespaceIdentifier == "bag.utest");
            var otherSignal = signals.Single(s => s.MetaData is not null && s.MetaData.NamespaceIdentifier == "other.ns");

            var trigger = triggers.Definitions.OfType<SignalTriggerDefinition>().Single();

            Check.That(signal).IsNotNull();
            Check.That(signal.Uid).Not.IsDefaultValue();

            Check.That(otherSignal).IsNotNull();
            Check.That(otherSignal.Uid).Not.IsDefaultValue();

            Check.That(trigger).IsNotNull();
            Check.That(trigger!.ListenSignal).IsNotNull();
            Check.That(trigger!.ListenSignal!.Value.Uid).IsEqualTo(signal.Uid);
            Check.That(trigger!.Targets).IsNotNull().And.CountIs(1);

            var seq = trigger.Targets.First();
            Check.That(seq).IsNotNull();
            Check.That(seq.Type).IsEqualTo(TargetTypeEnum.Sequence);
            Check.That(seq.Uid).IsEqualTo(seqUid);
        }

        /// <summary>
        /// Yaml create Cron trigger with static guid target
        /// </summary>
        [Fact]
        public async Task Yaml_To_Trigger_Stream_Target_Sequence_Guid()
        {
            var streamId = Guid.NewGuid();
            var seqUid = Guid.NewGuid();

            var consumer = Random.Shared.Next(1, 42);

            var yaml = """
                    triggers:
                        - stream : from-guid-stream
                          from: <<STREAM>>
                          max-consumer-by-node: <<CONSUMER>>
                          targets:
                            sequences:
                                - <<SEQ_CALL>>
                    """.Replace("<<STREAM>>", streamId.ToString())
                       .Replace("<<SEQ_CALL>>", seqUid.ToString())
                       .Replace("<<CONSUMER>>", consumer.ToString());

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var triggers = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(triggers).IsNotNull();
            Check.That(triggers.Success).IsTrue();
            Check.That(triggers.Logs).IsNullOrEmpty();
            Check.That(triggers.CompileOption).IsNull();
            Check.That(triggers.Definitions).IsNotNull().And.CountIs(1);

            var trigger = triggers.Definitions.First() as StreamTriggerDefinition;

            Check.That(trigger).IsNotNull();
            Check.That(trigger!.FixedMaxConcurrentProcess).IsEqualTo(StreamTriggerDefinition.DEFAULT_FIX_CONCURRENT_PROCESS);
            Check.That(trigger.RelativeMaxConcurrentProcess).IsEqualTo(consumer);
            Check.That(trigger.StreamSourceDefinitionUid).IsEqualTo(streamId);
            Check.That(trigger.Targets).IsNotNull().And.CountIs(1);

            var seq = trigger.Targets.First();
            Check.That(seq).IsNotNull();
            Check.That(seq.Type).IsEqualTo(TargetTypeEnum.Sequence);
            Check.That(seq.Uid).IsEqualTo(seqUid);
        }

        /// <summary>
        /// Yaml create Cron trigger with static guid target
        /// </summary>
        [Fact]
        public async Task Yaml_To_Trigger_Stream_Target_Sequence_Guid_By_Ref()
        {
            var seqUid = Guid.NewGuid();

            var consumer = Random.Shared.Next(1, 42);

            var yaml = """
                    signals:
                        - signal: from-trigger

                    streams:
                        - stream : from-trigger
                          queue-namespace : txt
                          queue-name : name

                    triggers:
                        - stream : from-guid-stream
                          from: ref/from-trigger
                          max-consumer-by-node: <<CONSUMER>>
                          targets:
                            sequences:
                                - <<SEQ_CALL>>
                    """.Replace("<<SEQ_CALL>>", seqUid.ToString())
                       .Replace("<<CONSUMER>>", consumer.ToString());

            var solver = GenerateTestSolver();
            var parser = new DefinitionCompiler(solver, this._converter);
            var triggers = await parser.CompileAsync(yaml, Models.DefinitionParserSourceEnum.Yaml);

            Check.That(triggers).IsNotNull();
            Check.That(triggers.Success).IsTrue();
            Check.That(triggers.Logs).IsNullOrEmpty();
            Check.That(triggers.CompileOption).IsNull();
            Check.That(triggers.Definitions).IsNotNull().And.CountIs(3);

            var signal = triggers.Definitions.OfType<SignalDefinition>().Single();
            var stream = triggers.Definitions.OfType<StreamQueueDefinition>().Single();
            var trigger = triggers.Definitions.OfType<StreamTriggerDefinition>().Single();

            Check.That(signal).IsNotNull();
            Check.That(signal!.Uid).Not.IsDefaultValue();

            Check.That(stream).IsNotNull();
            Check.That(stream!.Uid).Not.IsDefaultValue();

            Check.That(trigger).IsNotNull();
            Check.That(trigger!.FixedMaxConcurrentProcess).IsEqualTo(StreamTriggerDefinition.DEFAULT_FIX_CONCURRENT_PROCESS);
            Check.That(trigger.RelativeMaxConcurrentProcess).IsEqualTo(consumer);
            Check.That(trigger.StreamSourceDefinitionUid).IsEqualTo(stream.Uid);
            Check.That(trigger.Targets).IsNotNull().And.CountIs(1);

            var seq = trigger.Targets.First();
            Check.That(seq).IsNotNull();
            Check.That(seq.Type).IsEqualTo(TargetTypeEnum.Sequence);
            Check.That(seq.Uid).IsEqualTo(seqUid);
        }

        #endregion

        #region Tools

        /// <summary>
        /// 
        /// </summary>
        private void CheckCallStage<TVGrain>(SequenceStageCallDefinition stage,
                                             Expression<Func<TVGrain, Task>> method,
                                             Action<AccessExpressionDefinition?, ConcretType?>? validateConfiguration = null)
        {
            if (validateConfiguration is null)
            {
                Check.That(stage.Configuration).IsNull();
                Check.That(stage.ConfigurationFromContextDataType).IsNull();
            }
            else
            {
                validateConfiguration(stage.Configuration, stage.ConfigurationFromContextDataType);
            }

            var expectedMethod = ((MethodCallExpression)method.Body).Method;

            Check.That(stage.CallMethodDefinition).IsNotNull().And.IsEqualTo(expectedMethod.GetAbstractMethod());
            Check.That(stage.VGrainType).IsNotNull().And.IsEqualTo(typeof(TVGrain).GetAbstractType());

            Check.That(stage.ParameterDefinitions).IsNotNull().And.CountIs(expectedMethod.GetParameters().Length);

            for (int i = 0; i < stage.ParameterDefinitions!.Count; i++)
            {
                var p = stage.ParameterDefinitions.ElementAt(i);
                var mthP = expectedMethod.GetParameters()[i];

                var paramProvideType = p.Access.TargetType.ToType();
                Check.That(paramProvideType).IsEqualTo(mthP.ParameterType);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private TStageType CheckStage<TStageType, TInput, TOutput>(SequenceStageDefinition stage, DefinitionMetaData? metaData = null)
            where TStageType : SequenceStageDefinition
        {
            Check.That(stage).IsNotNull().And.IsInstanceOf<TStageType>();

            if (NoneType.IsEqualTo<TInput>())
                Check.That(stage.Input).IsNull();
            else
                Check.That(stage.Input).IsNotNull().And.IsEqualTo(typeof(TInput).GetAbstractType());

            if (NoneType.IsEqualTo<TOutput>())
                Check.That(stage.Output).IsNull();
            else
                Check.That(stage.Output).IsNotNull().And.IsEqualTo(typeof(TOutput).GetAbstractType());

            Check.That(stage.Uid).Not.IsDefaultValue();
            Check.That(stage.MetaData).IsEqualTo(metaData);

            return (TStageType)stage;
        }

        /// <summary>
        /// Generate valid solver base on interface, class and type describe in this class
        /// </summary>
        private IDemocriteReferenceSolverService GenerateTestSolver()
        {
            var solver = Substitute.For<IDemocriteReferenceSolverService>();

            solver.GetReferenceTypeAsync(Arg.Any<Uri>())!.Returns(info =>
            {
                var refId = info[0] as Uri;

                Check.That(refId).IsNotNull();

                RefIdHelper.Explode(refId!, out var type, out var np, out var sni);

                if (type == RefTypeEnum.Type && sni == "text-visitor")
                    return ValueTask.FromResult(Tuple.Create(typeof(TextVisitor), refId!));

                if (type == RefTypeEnum.Type && np == "system" && sni == "string")
                    return ValueTask.FromResult(Tuple.Create(typeof(string), refId!));

                if (type == RefTypeEnum.Type && np == "system" && sni == "guid")
                    return ValueTask.FromResult(Tuple.Create(typeof(Guid), refId!));

                if (type == RefTypeEnum.Type && np == "bag.text" && sni == "pattern-matching-model")
                    return ValueTask.FromResult(Tuple.Create(typeof(PatternMatchingModelInfo), refId!));

                if (type == RefTypeEnum.VGrain && np == "bag.text" && sni == "toolbox")
                    return ValueTask.FromResult(Tuple.Create(typeof(ITextTool), refId!));

                if (type == RefTypeEnum.VGrain && np == "bag.text" && sni == "language-detector")
                    return ValueTask.FromResult(Tuple.Create(typeof(ITextLangDetector), refId!));

                if (type == RefTypeEnum.VGrain && np == "bag.text" && sni == "tokenizer")
                    return ValueTask.FromResult(Tuple.Create(typeof(ITextTokenizer), refId!));

                if (type == RefTypeEnum.VGrain && np == "bag.text" && sni == "pattern-matching")
                    return ValueTask.FromResult(Tuple.Create(typeof(ITextPatternMatching), refId!));

                throw new InvalidOperationException("Request not exepected " + refId);
            });

            solver.GetReferenceMethodAsync(Arg.Any<Uri>(), Arg.Any<Type>()).Returns(info =>
            {
                var refId = info[0] as Uri;

                Check.That(refId).IsNotNull();

                RefIdHelper.Explode(refId!, out var type, out var np, out var sni);

                Check.That(type).IsEqualTo(RefTypeEnum.Method);

                var method = RefIdHelper.GetMethodName(refId!);

                if (np == "bag.text" && sni == "toolbox" && method == "simplify")
                    return ValueTask.FromResult(typeof(ITextTool).GetMethod(nameof(ITextTool.Simplify)));

                if (np == "bag.text" && sni == "language-detector" && method == "lang")
                    return ValueTask.FromResult(typeof(ITextLangDetector).GetMethod(nameof(ITextLangDetector.Lang)));

                if (np == "bag.text" && sni == "tokenizer" && method == "tokenize")
                    return ValueTask.FromResult(typeof(ITextTokenizer).GetMethod(nameof(ITextTokenizer.Tokenize)));

                if (np == "bag.text" && sni == "pattern-matching" && method == "tag")
                    return ValueTask.FromResult(typeof(ITextPatternMatching).GetMethod(nameof(ITextPatternMatching.Tag)));

                throw new InvalidOperationException("Method Request not exepected " + refId);
            });

            return solver;
        }

        #endregion

        #endregion
    }
}
