// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.UnitTests.Helpers
{
    using AutoFixture;

    using Democrite.Framework.Core.Abstractions.Enums;

    using Microsoft.Extensions.Logging.Abstractions;

    using NFluent;

    using System;

    /// <summary>
    /// Test for <see cref="RefIdHelper"/>
    /// </summary>
    public sealed class RefIdHelperUTest
    {
        #region Methods

        [Theory]
        [InlineData(RefTypeEnum.VGrain, "sni-vgrain", "vgrain.namespace", "ref://vgr@vgrain.namespace/sni-vgrain")]
        [InlineData(RefTypeEnum.VGrainImplementation, "sni-vgrain-impl", "vgrain.namespace", "ref://gri@vgrain.namespace/sni-vgrain-impl")]
        [InlineData(RefTypeEnum.Sequence, "sni-sequence", "sequence.namespace", "ref://seq@sequence.namespace/sni-sequence")]
        [InlineData(RefTypeEnum.Signal, "sni-signal", "signal.namespace", "ref://sgl@signal.namespace/sni-signal")]
        [InlineData(RefTypeEnum.Door, "sni-door", "door.namespace", "ref://dor@door.namespace/sni-door")]
        [InlineData(RefTypeEnum.Trigger, "sni-trigger", "trigger.namespace", "ref://tgr@trigger.namespace/sni-trigger")]
        [InlineData(RefTypeEnum.Artifact, "sni-artifact", "artifact.namespace", "ref://art@artifact.namespace/sni-artifact")]
        [InlineData(RefTypeEnum.StreamQueue, "sni-stream-queue", "stream.queue.namespace", "ref://stm@stream.queue.namespace/sni-stream-queue")]
        [InlineData(RefTypeEnum.BlackboardTemplate, "sni-blackboard-template", "blackboard.template.namespace", "ref://bbt@blackboard.template.namespace/sni-blackboard-template")]
        [InlineData(RefTypeEnum.BlackboardController, "sni-blackboard-controller", "blackboard.controller.namespace", "ref://bbc@blackboard.controller.namespace/sni-blackboard-controller")]
        [InlineData(RefTypeEnum.Type, "sni-type", "type.namespace", "ref://typ@type.namespace/sni-type")]
        [InlineData(RefTypeEnum.Method, "sni-method", "method.namespace", "ref://mth@method.namespace/sni-method")]
        [InlineData(RefTypeEnum.Other, "sni-other", "other.namespace", "ref://otr@other.namespace/sni-other")]
        public void RefId_Generate(RefTypeEnum type, string simpleNameIdentifier, string? @namespace, string expected)
        {
            var refId = RefIdHelper.Generate(type, simpleNameIdentifier, @namespace);

            Check.That(refId).IsNotNull().And.IsEqualTo(new Uri(expected));
        }

        [Fact]
        public void RefId_BadSNI()
        {
            // UPPER not allowed
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "MAJ")).Throws<InvalidDataException>();
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "maj")).DoesNotThrow();

            // Min 2 char
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "a")).Throws<InvalidDataException>();
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "aa")).DoesNotThrow();

            // Invalid symbols
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "abcdefghijklmnopqrstuvwxyz0123456789+-*/=!:|&\"\\/")).Throws<InvalidDataException>();
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "abcdefghijklmnopqrstuvwxyz0123456789-")).DoesNotThrow();
        }

        [Fact]
        public void RefId_Bad_Namespace()
        {
            // NULL or empty allowed
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "sni", null)).DoesNotThrow();
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "sni", string.Empty)).DoesNotThrow();
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "sni", "          ")).DoesNotThrow();

            // UPPER not allowed
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "sni", "MAJ")).Throws<InvalidDataException>();
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "sni", "maj")).DoesNotThrow();

            // Min 2 char
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "sni", "a")).Throws<InvalidDataException>();
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "sni", "aa")).DoesNotThrow();

            // Invalid symbols
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "sni", "abcdefghijklmnopqrstuvwxyz0123456789+-*/=!:|&\"\\/")).Throws<InvalidDataException>();
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "sni", "abcdefghijklmnopqrstuvwxyz0123456789.")).DoesNotThrow();
        }

        // RefTypeEnum.None
        [Fact]
        public void RefId_Bad_RefTyp()
        {
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.VGrain, "maj")).DoesNotThrow();
            Check.ThatCode(() => RefIdHelper.Generate(RefTypeEnum.None, "maj")).Throws<InvalidDataException>();
        }

        [Fact]
        public void RefId_Explode_And_Extract()
        {
            var fixture = new Fixture();

            var sni = fixture.Create<string>().ToLower();
            var namespaceIdentifier = fixture.Create<string>().ToLower().Replace("-", ".");

            var type = fixture.Create<RefTypeEnum>();
            while (type == RefTypeEnum.None)
                type = fixture.Create<RefTypeEnum>();

            var refId = RefIdHelper.Generate(type, sni, namespaceIdentifier);

            RefIdHelper.Explode(refId, out var testType, out var testNamespace, out var testSimpleNameIdentifier);

            Check.That(testType).IsEqualTo(type);
            Check.That(testSimpleNameIdentifier).IsEqualTo(sni);
            Check.That(testNamespace).IsEqualTo(namespaceIdentifier);

            var individualType = RefIdHelper.GetDefinitionType(refId);
            Check.That(individualType).IsEqualTo(type);

            var individualSNI = RefIdHelper.GetSimpleNameIdentification(refId);
            Check.That(individualSNI).IsEqualTo(sni);

            var individualNamespace = RefIdHelper.GetNamespaceIdentification(refId);
            Check.That(individualNamespace).IsEqualTo(namespaceIdentifier);
        }

        [Fact]
        public void RefId_Validation()
        {
            var fixture = new Fixture();

            var sni = fixture.Create<string>().ToLower();
            var namespaceIdentifier = fixture.Create<string>().ToLower().Replace("-", ".");

            var type = fixture.Create<RefTypeEnum>();
            while (type == RefTypeEnum.None)
                type = fixture.Create<RefTypeEnum>();

            var refId = RefIdHelper.Generate(type, sni, namespaceIdentifier);

            var webUri = new Uri("http://google.com");
            var fileUri = new Uri("file://c://toto.txt");

            Check.That(RefIdHelper.IsRefId(webUri)).IsFalse();
            Check.That(RefIdHelper.IsRefId(fileUri)).IsFalse();
            Check.That(RefIdHelper.IsRefId(refId)).IsTrue();

            Check.That(RefIdHelper.ValidateRefId(refId, NullLogger.Instance)).IsTrue();
            Check.That(RefIdHelper.ValidateRefId(webUri, NullLogger.Instance)).IsFalse();
            Check.That(RefIdHelper.ValidateRefId(fileUri, NullLogger.Instance)).IsFalse();
        }

        [Fact]
        public void RefId_WithMethod()
        {
            var fixture = new Fixture();

            var sni = fixture.Create<string>().ToLower();
            var namespaceIdentifier = fixture.Create<string>().ToLower().Replace("-", ".");

            var type = fixture.Create<RefTypeEnum>();
            while (type == RefTypeEnum.None)
                type = fixture.Create<RefTypeEnum>();

            var refId = RefIdHelper.Generate(type, sni, namespaceIdentifier);

            var mth = fixture.Create<string>().ToLower();
            var refIdWithMth = RefIdHelper.WithMethod(refId, mth);

            // By default the URiCOmparer doesn't check the fragment part
            Check.That(refIdWithMth).IsNotNull().And.IsEqualTo(refId);
            Check.That(refId.Fragment).IsNullOrEmpty();
            Check.That(refIdWithMth.Fragment).IsNotNull().And.IsNotEmpty().And.IsEqualTo('#' + mth);
        }

        #region Tools

        #endregion
        #endregion
    }
}
