// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstraction.UnitTests.Signals
{
    using AutoFixture;

    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    using NFluent;

    using Orleans.Runtime;

    using System.Linq;

    /// <summary>
    /// Test for <see cref="SignalMessageComparer"/>
    /// </summary>
    public sealed class SignalMessageComparerUTest
    {
        /// <summary>
        /// Ensures the <see cref="SignalMessage"/> default comparer <see cref="SignalMessageComparer.Default"/>.
        /// </summary>
        [Fact]
        public void Ensure_SignalMessageComparer_Default_Comparer()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var sourceMsg = fixture.CreateMany<SignalMessage>(10).ToArray();

            for (int i = 0; i < sourceMsg.Length; i++)
            {
                var msg = sourceMsg[i];
                for (int j = 0; j < sourceMsg.Length; j++)
                {
                    var otherMsg = sourceMsg[j];

                    var checker = Check.That(msg);

                    if (i != j)
                        checker = checker.Not;

                    checker.IsEqualTo(otherMsg, SignalMessageComparer.Default);
                }
            }
        }

        /// <summary>
        /// Ensures the <see cref="SignalMessage"/> with comparer <see cref="SignalMessageComparer.OnlyDefinition"/> only compare definition part.
        /// </summary>
        [Fact]
        public void Ensure_SignalMessageComparer_OnlyDefinition_Comparer()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            var sourceMsg = fixture.CreateMany<SignalMessage>(10).ToArray();

            var sharedDefinitionId = Guid.NewGuid();
            var sharedDefinitionName = fixture.Create<string>();

            fixture.Register<SignalSource>(() =>
            {
                return new SignalSource(Guid.NewGuid(),
                                        sharedDefinitionId,
                                        sharedDefinitionName,
                                        fixture.Create<bool>(),
                                        fixture.Create<DateTime>(),
                                        fixture.Create<GrainId>(),
                                        fixture.Create<VGrainMetaData>(),
                                        null);
            });

            var otherFromSameDefinition = fixture.CreateMany<SignalMessage>(10).ToArray();

            for (int i = 0; i < sourceMsg.Length; i++)
            {
                var msg = sourceMsg[i];
                for (int j = 0; j < sourceMsg.Length; j++)
                {
                    var otherMsg = sourceMsg[j];

                    var checker = Check.That(msg);

                    if (i != j && 
                        msg.From.SourceDefinitionId != otherMsg.From.SourceDefinitionId && 
                        msg.From.SourceDefinitionName != otherMsg.From.SourceDefinitionName)
                    {
                        checker = checker.Not;
                    }

                    checker.IsEqualTo(otherMsg, SignalMessageComparer.Default);
                }
            }
        }
    }
}
