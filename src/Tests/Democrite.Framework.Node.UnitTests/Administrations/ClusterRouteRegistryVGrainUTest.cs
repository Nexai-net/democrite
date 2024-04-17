// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.UnitTests.Administrations
{
    using AutoFixture;

    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Node.Abstractions.Administrations;
    using Democrite.Framework.Node.Abstractions.Enums;
    using Democrite.Framework.Node.Abstractions.Models.Administrations;
    using Democrite.Framework.Node.Administrations;
    using Democrite.UnitTests.ToolKit.Extensions;

    using Elvex.Toolbox.Helpers;
    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    using Microsoft.Extensions.Logging;

    using NFluent;

    using NSubstitute;

    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="IClusterRouteRegistryVGrain"/>
    /// </summary>
    public sealed class ClusterRouteRegistryVGrainUTest
    {
        #region Fields

        private readonly Fixture _fixture;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterRouteRegistryVGrainUTest"/> class.
        /// </summary>
        public ClusterRouteRegistryVGrainUTest()
        {
            this._fixture = ObjectTestHelper.PrepareFixture();
        }

        #endregion

        #region Nested

        public interface IListenerAdminReceiver : IAdminEventReceiver, IVGrain
        {
        }

        public interface IListenerAdminOtherReceiver : IAdminEventReceiver, IVGrain
        {
        }

        private sealed class ListenerAdminReceiver : VGrainBase<IListenerAdminReceiver>, IListenerAdminReceiver
        {
            private readonly List<AdminEventArg> _eventReceived;

            public ListenerAdminReceiver(ILogger<IListenerAdminReceiver> logger)
                : base(logger)
            {
                this._eventReceived = new List<AdminEventArg>();
            }

            public IReadOnlyCollection<AdminEventArg> EventReceived
            {
                get { return this._eventReceived; }
            }

            public Task ReceiveAsync<TEvent>(TEvent adminEvent, GrainCancellationToken cancellationToken) where TEvent : AdminEventArg
            {
                this._eventReceived.Add(adminEvent);
                return Task.CompletedTask;
            }
        }

        #endregion

        #region Methods

        [Fact]
        public async Task Subscribe_Unsubscribe()
        {
            var factoryMock = Substitute.For<IGrainFactory>();

            var listener = await this._fixture.CreateAndInitVGrain<ListenerAdminReceiver>();

            var adminGrain = await InitializeAdminRegistryGrainAsync<IClusterRouteRegistryVGrain>(factoryMock);
            Check.That(adminGrain).IsNotNull();

            using (var timeout = CancellationHelper.DisposableTimeout())
            using (var cancel = timeout.Content.ToGrainCancellationTokenSource())
            {
                var uid = await adminGrain.SubscribeRouteChangeAsync(listener.GetDedicatedGrainId<IAdminEventReceiver>(), cancel.Token);

                Check.That(uid).Not.IsDefaultValue();

                await adminGrain.UnsubscribeAsync(uid);
            }
        }

        [Fact]
        public async Task Add_Redirection_With_Notification()
        {
            var redirectionTest = Guid.NewGuid().ToString();
            var factoryMock = Substitute.For<IGrainFactory>();

            var listener = await this._fixture.CreateAndInitVGrain<ListenerAdminReceiver>();
            factoryMock.GetGrain(listener.GetGrainId()).Returns(listener);

            var adminGrain = await InitializeAdminRegistryGrainAsync<IClusterRouteRegistryVGrain>(factoryMock);
            Check.That(adminGrain).IsNotNull();

            using (var timeout = CancellationHelper.DisposableTimeout())
            using (var cancel = timeout.Content.ToGrainCancellationTokenSource())
            {
                var uid = await adminGrain.SubscribeRouteChangeAsync(listener.GetDedicatedGrainId<IAdminEventReceiver>(), cancel.Token);
                Check.That(uid).Not.IsDefaultValue();

                var resultAppend = await adminGrain.RequestAppendRedirectionAsync(VGrainClassNameRedirectionDefinition.Create<IListenerAdminReceiver>(redirectionTest), null!);
                Check.That(resultAppend).IsTrue();

                Check.That(listener.EventReceived).IsNotNull().And.CountIs(1);

                var adminEvent = listener.EventReceived.First();
                Check.That(adminEvent).IsNotNull().And.WhichMember(a => a.Type).Verifies(e => e.IsEqualTo(AdminEventTypeEnum.RouteChange));

                await adminGrain.UnsubscribeAsync(uid);
            }
        }

        [Fact]
        public async Task Add_Redirection_With_Notification_And_Get()
        {
            var redirectionTest = Guid.NewGuid().ToString();
            var factoryMock = Substitute.For<IGrainFactory>();

            var listener = await this._fixture.CreateAndInitVGrain<ListenerAdminReceiver>();
            factoryMock.GetGrain(listener.GetGrainId()).Returns(listener);

            var adminGrain = await InitializeAdminRegistryGrainAsync<IClusterRouteRegistryVGrain>(factoryMock);
            Check.That(adminGrain).IsNotNull();

            using (var timeout = CancellationHelper.DisposableTimeout())
            using (var cancel = timeout.Content.ToGrainCancellationTokenSource())
            {
                var uid = await adminGrain.SubscribeRouteChangeAsync(listener.GetDedicatedGrainId<IAdminEventReceiver>(), cancel.Token);
                Check.That(uid).Not.IsDefaultValue();

                // Add first
                var firstRedirectionDef = VGrainClassNameRedirectionDefinition.Create<IListenerAdminReceiver>(redirectionTest);
                var resultAppend = await adminGrain.RequestAppendRedirectionAsync(firstRedirectionDef, null!);
                Check.That(resultAppend).IsTrue();

                Check.That(listener.EventReceived).IsNotNull().And.CountIs(1);

                var adminEvent = listener.EventReceived.First();
                Check.That(adminEvent).IsNotNull().And.WhichMember(a => a.Type).Verifies(e => e.IsEqualTo(AdminEventTypeEnum.RouteChange));

                // Add Other
                var secondRedirectionDef = VGrainClassNameRedirectionDefinition.Create<IListenerAdminOtherReceiver>(redirectionTest);
                var resultOtherAppend = await adminGrain.RequestAppendRedirectionAsync(secondRedirectionDef, null!);
                Check.That(resultOtherAppend).IsTrue();

                Check.That(listener.EventReceived).IsNotNull().And.CountIs(2);

                var adminOtherEvent = listener.EventReceived.Except(adminEvent.AsEnumerable()).First();
                Check.That(adminOtherEvent).IsNotNull().And.WhichMember(a => a.Type).Verifies(e => e.IsEqualTo(AdminEventTypeEnum.RouteChange));

                var redirections = await adminGrain.GetGlobalRedirection(string.Empty, cancel.Token);
                Check.That(redirections.Value.Info).IsNotNull().And.CountIs(2);
                Check.That(redirections.Value.Info.Select(i => i.Source).Except(new[] { firstRedirectionDef, secondRedirectionDef }.Select(i => i.Source)).ToArray()).CountIs(0);

                await adminGrain.UnsubscribeAsync(uid);
            }
        }

        [Fact]
        public async Task Add_Redirection_With_Notification_And_Get_Delete()
        {
            var redirectionTest = Guid.NewGuid().ToString();
            var factoryMock = Substitute.For<IGrainFactory>();

            var listener = await this._fixture.CreateAndInitVGrain<ListenerAdminReceiver>();
            factoryMock.GetGrain(listener.GetGrainId()).Returns(listener);

            var adminGrain = await InitializeAdminRegistryGrainAsync<IClusterRouteRegistryVGrain>(factoryMock);
            Check.That(adminGrain).IsNotNull();

            using (var timeout = CancellationHelper.DisposableTimeout())
            using (var cancel = timeout.Content.ToGrainCancellationTokenSource())
            {
                var uid = await adminGrain.SubscribeRouteChangeAsync(listener.GetDedicatedGrainId<IAdminEventReceiver>(), cancel.Token);
                Check.That(uid).Not.IsDefaultValue();

                // Add first
                var firstRedirectionDef = VGrainClassNameRedirectionDefinition.Create<IListenerAdminReceiver>(redirectionTest);
                var resultAppend = await adminGrain.RequestAppendRedirectionAsync(firstRedirectionDef, null!);
                Check.That(resultAppend).IsTrue();

                Check.That(listener.EventReceived).IsNotNull().And.CountIs(1);

                var adminEvent = listener.EventReceived.First();
                Check.That(adminEvent).IsNotNull().And.WhichMember(a => a.Type).Verifies(e => e.IsEqualTo(AdminEventTypeEnum.RouteChange));

                // Add Other
                var secondRedirectionDef = VGrainClassNameRedirectionDefinition.Create<IListenerAdminOtherReceiver>(redirectionTest);
                var resultOtherAppend = await adminGrain.RequestAppendRedirectionAsync(secondRedirectionDef, null!);
                Check.That(resultOtherAppend).IsTrue();

                Check.That(listener.EventReceived).IsNotNull().And.CountIs(2);

                var adminOtherEvent = listener.EventReceived.Except(adminEvent.AsEnumerable()).First();
                Check.That(adminOtherEvent).IsNotNull().And.WhichMember(a => a.Type).Verifies(e => e.IsEqualTo(AdminEventTypeEnum.RouteChange));

                var redirections = await adminGrain.GetGlobalRedirection(string.Empty, cancel.Token);
                Check.That(redirections.Value.Info).IsNotNull().And.CountIs(2);
                Check.That(redirections.Value.Info.Select(i => i.Source).Except(new[] { firstRedirectionDef, secondRedirectionDef }.Select(i => i.Source)).ToArray()).CountIs(0);

                // Clear Second
                var remove = await adminGrain.RequestPopRedirectionAsync(secondRedirectionDef.Uid, null!);
                Check.That(remove).IsTrue();

                var redirectionsAfterRemove = await adminGrain.GetGlobalRedirection(string.Empty, cancel.Token);
                Check.That(redirectionsAfterRemove.Value.Info).IsNotNull().And.CountIs(1);

                await adminGrain.UnsubscribeAsync(uid);
            }
        }

        [Fact]
        public async Task Add_Redirection_Conflict()
        {
            var redirectionTest = Guid.NewGuid().ToString();
            var factoryMock = Substitute.For<IGrainFactory>();

            var listener = await this._fixture.CreateAndInitVGrain<ListenerAdminReceiver>();
            factoryMock.GetGrain(listener.GetGrainId()).Returns(listener);

            var adminGrain = await InitializeAdminRegistryGrainAsync<IClusterRouteRegistryVGrain>(factoryMock);
            Check.That(adminGrain).IsNotNull();

            using (var timeout = CancellationHelper.DisposableTimeout())
            using (var cancel = timeout.Content.ToGrainCancellationTokenSource())
            {
                var uid = await adminGrain.SubscribeRouteChangeAsync(listener.GetDedicatedGrainId<IAdminEventReceiver>(), cancel.Token);
                Check.That(uid).Not.IsDefaultValue();

                // Append
                var resultAppend = await adminGrain.RequestAppendRedirectionAsync(VGrainClassNameRedirectionDefinition.Create<IListenerAdminReceiver>(redirectionTest), null!);
                Check.That(resultAppend).IsTrue();

                // Conflict
                var resultConflictAppend = await adminGrain.RequestAppendRedirectionAsync(VGrainClassNameRedirectionDefinition.Create<IListenerAdminReceiver>(redirectionTest), null!);
                Check.That(resultConflictAppend).IsFalse();

                Check.That(listener.EventReceived).IsNotNull().And.CountIs(1);

                var adminEvent = listener.EventReceived.First();
                Check.That(adminEvent).IsNotNull().And.WhichMember(a => a.Type).Verifies(e => e.IsEqualTo(AdminEventTypeEnum.RouteChange));

                await adminGrain.UnsubscribeAsync(uid);
            }
        }

        #endregion

        #region Tools

        private async Task<TGrain> InitializeAdminRegistryGrainAsync<TGrain>(IGrainFactory mockGrainFactory)
        {
            var grain = await this._fixture.CreateAndInitVGrain<AdministrationRegistryVGrain>(factory: mockGrainFactory);
            return (TGrain)(object)grain;
        }

        #endregion
    }
}
