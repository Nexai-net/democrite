// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.UnitTests.VGrains
{
    using AutoFixture;

    using Democrite.Framework.Core.Abstractions.Surrogates;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;
    using Democrite.Framework.Node.Blackboard.VGrains;
    using Democrite.UnitTests.ToolKit.Extensions;

    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    using NFluent;

    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="DefaultBlackboardControllerGrain"/>
    /// </summary>
    public sealed class DefaultBlackboardControllerGrainUTest
    {
        [Fact]
        public async Task Storage_Conflict_Prepared_Slot()
        {
            var fixture = ObjectTestHelper.PrepareFixture();

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var logicalType = "Result";
            var now = DateTime.UtcNow;
            var pastNow = DateTime.UtcNow.AddDays(-1);

            var newRecord = new DataRecordContainer<int>(logicalType,
                                                         Guid.NewGuid(),
                                                         logicalType + "new",
                                                         42,
                                                         RecordStatusEnum.Ready,
                                                         now,
                                                         fixture.Create<string>(),
                                                         now,
                                                         fixture.Create<string>(),
                                                         null);

            var preparedSolt = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                            logicalType,
                                                            logicalType + "Conflict",
                                                            null,
                                                            RecordContainerTypeEnum.Direct,

                                                            // Set preparation to test
                                                            RecordStatusEnum.Preparation,
                                                            pastNow,
                                                            fixture.Create<string>(),
                                                            pastNow,
                                                            fixture.Create<string>(),
                                                            null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(1,
                                                                   new [] { preparedSolt },
                                                                   newRecord,
                                                                   fixture.Create<BlackboardProcessingResolutionLimitTypeEnum>(),
                                                                   fixture.Create<BlackboardProcessingResolutionRemoveTypeEnum>());

            var resolutions = await grain.ResolvePushIssueAsync(issue, newRecord, new GrainCancellationTokenSource().Token);

            Check.That(resolutions).IsNotNull().And.CountIs(1);

            var action = resolutions!.First();

            Check.That(action).IsInstanceOf<BlackboardCommandStorageAddRecord<int>>();

            var cmd = (BlackboardCommandStorageAddRecord<int>)action;

            Check.That(cmd.Override).IsTrue();
            Check.That(cmd.InsertIfNew).IsTrue();
            Check.That(cmd.Record).IsNotNull();

            // Use most of the new record data
            var record = cmd.Record;
            Check.That(record.Data).IsEqualTo(newRecord.Data);
            Check.That(record.Status).IsEqualTo(newRecord.Status);
            Check.That(record.ContainsType).IsEqualTo(newRecord.ContainsType);
            Check.That(record.CustomMetadata).IsEqualTo(newRecord.CustomMetadata);
            Check.That(record.DisplayName).IsEqualTo(newRecord.DisplayName);
            Check.That(record.UTCLastUpdateTime).IsEqualTo(newRecord.UTCLastUpdateTime);
            Check.That(record.LastUpdaterIdentity).IsEqualTo(newRecord.LastUpdaterIdentity);

            // But keep the prepared slot important information
            Check.That(record.Uid).IsEqualTo(preparedSolt.Uid).And.IsNotEqualTo(newRecord.Uid);
            Check.That(record.UTCCreationTime).IsEqualTo(preparedSolt.UTCCreationTime).And.IsNotEqualTo(newRecord.UTCCreationTime);
            Check.That(record.CreatorIdentity).IsEqualTo(preparedSolt.CreatorIdentity).And.IsNotEqualTo(newRecord.CreatorIdentity);
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.Reject"/> strategy
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_Reject(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture();

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var logicalType = "Result";
            var now = DateTime.UtcNow;

            var newRecord = new DataRecordContainer<int>(logicalType,
                                                         Guid.NewGuid(),
                                                         logicalType + "new",
                                                         42,
                                                         RecordStatusEnum.Ready,
                                                         now,
                                                         null,
                                                         now,
                                                         null,
                                                         null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(1,
                                                                   new[]
                                                                   {
                                                                       new BlackboardRecordMetadata(Guid.NewGuid(),
                                                                                                    logicalType,
                                                                                                    logicalType + "Conflict",
                                                                                                    null,
                                                                                                    RecordContainerTypeEnum.Direct,
                                                                                                    RecordStatusEnum.Ready,
                                                                                                    now,
                                                                                                    null,
                                                                                                    now,
                                                                                                    null,
                                                                                                    null)
                                                                   },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.Reject,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, new GrainCancellationTokenSource().Token);

            // On Reject not resolution is available
            Check.That(resolutionSteps).IsNull();
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.ClearAll"/> strategy
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_ClearAll_One(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = fixture.Create<BlackboardRecordMetadata>();

            var issue = new MaxRecordBlackboardProcessingRuleIssue(1,
                                                                   new[] { conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.ClearAll,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(2);

            if (removeStrategy == BlackboardProcessingResolutionRemoveTypeEnum.Remove)
            {
                var remove = resolutionSteps!.First() as BlackboardCommandStorageRemoveRecord;

                Check.WithCustomMessage("First action must be removing the previous record").That(remove).IsNotNull();
                Check.That(remove!.Uid).IsEqualTo(conflictOne.Uid);
            }
            else
            {
                var deco = resolutionSteps!.First() as BlackboardCommandStorageDecommissionRecord;

                Check.WithCustomMessage("First action must be a decommission the previous record").That(deco).IsNotNull();
                Check.That(deco!.Uid).IsEqualTo(conflictOne.Uid);
            }
            var add = resolutionSteps!.Last() as BlackboardCommandStorageAddRecord<double>;
            Check.WithCustomMessage("Last action must be add new record").That(add).IsNotNull();
            Check.That(add!.Record).IsSameReferenceAs(newRecord);

            // Set value to true to force insertion
            Check.That(add!.Override).IsTrue();
            Check.That(add!.InsertIfNew).IsTrue();
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.ClearAll"/> strategy
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_ClearAll_Multiple(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = fixture.Create<BlackboardRecordMetadata>();
            var conflictTwo = fixture.Create<BlackboardRecordMetadata>();
            var conflictThree = fixture.Create<BlackboardRecordMetadata>();

            var issue = new MaxRecordBlackboardProcessingRuleIssue(3,
                                                                   new[] { conflictOne, conflictTwo, conflictThree },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.ClearAll,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(4);

            if (removeStrategy == BlackboardProcessingResolutionRemoveTypeEnum.Remove)
            {
                var removes = resolutionSteps!.SkipLast(1).OfType<BlackboardCommandStorageRemoveRecord>().ToArray();

                Check.WithCustomMessage("First actions must be removing the previous records").That(removes).IsNotNull().And.CountIs(3);
                Check.That(removes[0]!.Uid).IsEqualTo(conflictOne.Uid);
                Check.That(removes[1]!.Uid).IsEqualTo(conflictTwo.Uid);
                Check.That(removes[2]!.Uid).IsEqualTo(conflictThree.Uid);
            }
            else
            {
                var decos = resolutionSteps!.SkipLast(1).OfType<BlackboardCommandStorageDecommissionRecord>().ToArray();

                Check.WithCustomMessage("First actions must be decommissioned the previous records").That(decos).IsNotNull().And.CountIs(3);
                Check.That(decos[0]!.Uid).IsEqualTo(conflictOne.Uid);
                Check.That(decos[1]!.Uid).IsEqualTo(conflictTwo.Uid);
                Check.That(decos[2]!.Uid).IsEqualTo(conflictThree.Uid);
            }

            var add = resolutionSteps!.Last() as BlackboardCommandStorageAddRecord<double>;
            Check.WithCustomMessage("Last action must be add new record").That(add).IsNotNull();
            Check.That(add!.Record).IsSameReferenceAs(newRecord);

            // Set value to true to force insertion
            Check.That(add!.Override).IsTrue();
            Check.That(add!.InsertIfNew).IsTrue();
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepNewest"/> strategy
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepNewest_One(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force past creation
                                                           newRecord.UTCCreationTime.AddDays(-1),
                                                           null,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCLastUpdateTime.AddMinutes(5),
                                                           null, 
                                                           null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(1,
                                                                   new[] { conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepNewest,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(2);

            if (removeStrategy == BlackboardProcessingResolutionRemoveTypeEnum.Remove)
            {
                var remove = resolutionSteps!.First() as BlackboardCommandStorageRemoveRecord;

                Check.WithCustomMessage("First action must be removing the previous record").That(remove).IsNotNull();
                Check.That(remove!.Uid).IsEqualTo(conflictOne.Uid);
            }
            else
            {
                var deco = resolutionSteps!.First() as BlackboardCommandStorageDecommissionRecord;

                Check.WithCustomMessage("First action must be a decommission the previous record").That(deco).IsNotNull();
                Check.That(deco!.Uid).IsEqualTo(conflictOne.Uid);
            }

            var add = resolutionSteps!.Last() as BlackboardCommandStorageAddRecord<double>;
            Check.WithCustomMessage("Last action must be add new record").That(add).IsNotNull();
            Check.That(add!.Record).IsSameReferenceAs(newRecord);

            // Set value to true to force insertion
            Check.That(add!.Override).IsTrue();
            Check.That(add!.InsertIfNew).IsTrue();
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepNewest"/> strategy when pushed is alder
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepNewest_One_PushOlder(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force newer creation
                                                           newRecord.UTCCreationTime.AddDays(1),
                                                           null,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCLastUpdateTime.AddMinutes(-5),
                                                           null, 
                                                           null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(1,
                                                                   new[] { conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepNewest,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(1);
            Check.That(resolutionSteps!.First()).IsInstanceOf<BlackboardCommandRejectAction>();
            Check.That(((BlackboardCommandRejectAction)resolutionSteps!.First()).SourceIssue).IsSameReferenceAs(issue);
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepNewest"/> strategy
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepNewest_Multiple(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force past creation
                                                           newRecord.UTCCreationTime.AddDays(-1),
                                                           null,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCLastUpdateTime.AddMinutes(5),
                                                           null, null);

            var conflictTwo = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force After creation to be include in the keep ones
                                                           newRecord.UTCCreationTime.AddDays(1),
                                                           null,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCLastUpdateTime.AddMinutes(5),
                                                           null, null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(2,
                                                                   new[] { conflictTwo, conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepNewest,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(2);

            if (removeStrategy == BlackboardProcessingResolutionRemoveTypeEnum.Remove)
            {
                var remove = resolutionSteps!.First() as BlackboardCommandStorageRemoveRecord;

                Check.WithCustomMessage("First action must be removing the previous record").That(remove).IsNotNull();
                Check.That(remove!.Uid).IsEqualTo(conflictOne.Uid);
            }
            else
            {
                var deco = resolutionSteps!.First() as BlackboardCommandStorageDecommissionRecord;

                Check.WithCustomMessage("First action must be a decommission the previous record").That(deco).IsNotNull();
                Check.That(deco!.Uid).IsEqualTo(conflictOne.Uid);
            }

            var add = resolutionSteps!.Last() as BlackboardCommandStorageAddRecord<double>;
            Check.WithCustomMessage("Last action must be add new record").That(add).IsNotNull();
            Check.That(add!.Record).IsSameReferenceAs(newRecord);

            // Set value to true to force insertion
            Check.That(add!.Override).IsTrue();
            Check.That(add!.InsertIfNew).IsTrue();
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepNewest"/> strategy when pushed is alder
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepNewest_Multiple_PushOlder(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force newer creation
                                                           newRecord.UTCCreationTime.AddDays(1),
                                                           null,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCLastUpdateTime.AddMinutes(-5),
                                                           null, null);

            var conflictTwo = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force newer creation
                                                           newRecord.UTCCreationTime.AddDays(0.5),
                                                           null,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCLastUpdateTime.AddMinutes(-5),
                                                           null, null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(2,
                                                                   new[] { conflictTwo, conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepNewest,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(1);
            Check.That(resolutionSteps!.First()).IsInstanceOf<BlackboardCommandRejectAction>();
            Check.That(((BlackboardCommandRejectAction)resolutionSteps!.First()).SourceIssue).IsSameReferenceAs(issue);
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepNewestUpdated"/> strategy
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepNewest_Update_One(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCCreationTime.AddMinutes(5),

                                                           null,

                                                           // Force past creation
                                                           newRecord.UTCLastUpdateTime.AddDays(-1),
                                                           null,
                                                           null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(1,
                                                                   new[] { conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepNewestUpdated,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(2);

            if (removeStrategy == BlackboardProcessingResolutionRemoveTypeEnum.Remove)
            {
                var remove = resolutionSteps!.First() as BlackboardCommandStorageRemoveRecord;

                Check.WithCustomMessage("First action must be removing the previous record").That(remove).IsNotNull();
                Check.That(remove!.Uid).IsEqualTo(conflictOne.Uid);
            }
            else
            {
                var deco = resolutionSteps!.First() as BlackboardCommandStorageDecommissionRecord;

                Check.WithCustomMessage("First action must be a decommission the previous record").That(deco).IsNotNull();
                Check.That(deco!.Uid).IsEqualTo(conflictOne.Uid);
            }

            var add = resolutionSteps!.Last() as BlackboardCommandStorageAddRecord<double>;
            Check.WithCustomMessage("Last action must be add new record").That(add).IsNotNull();
            Check.That(add!.Record).IsSameReferenceAs(newRecord);

            // Set value to true to force insertion
            Check.That(add!.Override).IsTrue();
            Check.That(add!.InsertIfNew).IsTrue();
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepNewestUpdated"/> strategy when pushed is alder
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepNewest_Update_One_PushOlder(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCCreationTime.AddMinutes(-5),

                                                           null,

                                                           // Force newer creation
                                                           newRecord.UTCLastUpdateTime.AddDays(1),

                                                           null,
                                                           null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(1,
                                                                   new[] { conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepNewestUpdated,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(1);
            Check.That(resolutionSteps!.First()).IsInstanceOf<BlackboardCommandRejectAction>();
            Check.That(((BlackboardCommandRejectAction)resolutionSteps!.First()).SourceIssue).IsSameReferenceAs(issue);
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepNewestUpdated"/> strategy
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepNewest_Update_Multiple(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCCreationTime.AddMinutes(5),

                                                           null,

                                                           // Force past creation
                                                           newRecord.UTCLastUpdateTime.AddDays(-1),

                                                           null,
                                                           null);

            var conflictTwo = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force laster update to ensure that update date is used
                                                           newRecord.UTCCreationTime.AddMinutes(5),

                                                           null,

                                                           // Force After update to be include in the keep ones
                                                           newRecord.UTCLastUpdateTime.AddDays(1),

                                                           null,
                                                           null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(2,
                                                                   new[] { conflictTwo, conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepNewestUpdated,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(2);

            if (removeStrategy == BlackboardProcessingResolutionRemoveTypeEnum.Remove)
            {
                var remove = resolutionSteps!.First() as BlackboardCommandStorageRemoveRecord;

                Check.WithCustomMessage("First action must be removing the previous record").That(remove).IsNotNull();
                Check.That(remove!.Uid).IsEqualTo(conflictOne.Uid);
            }
            else
            {
                var deco = resolutionSteps!.First() as BlackboardCommandStorageDecommissionRecord;

                Check.WithCustomMessage("First action must be a decommission the previous record").That(deco).IsNotNull();
                Check.That(deco!.Uid).IsEqualTo(conflictOne.Uid);
            }

            var add = resolutionSteps!.Last() as BlackboardCommandStorageAddRecord<double>;
            Check.WithCustomMessage("Last action must be add new record").That(add).IsNotNull();
            Check.That(add!.Record).IsSameReferenceAs(newRecord);

            // Set value to true to force insertion
            Check.That(add!.Override).IsTrue();
            Check.That(add!.InsertIfNew).IsTrue();
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepNewestUpdated"/> strategy when pushed is alder
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepNewest_Update_Multiple_PushOlder(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force laster update to ensure that update date is used
                                                           newRecord.UTCCreationTime.AddMinutes(-5),

                                                           null,

                                                           // Force newer Update
                                                           newRecord.UTCLastUpdateTime.AddDays(1),
                                                           null,
                                                           null);

            var conflictTwo = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCCreationTime.AddMinutes(-5),

                                                           null,

                                                           // Force newer Update
                                                           newRecord.UTCLastUpdateTime.AddDays(0.5),
                                                           null,
                                                           null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(2,
                                                                   new[] { conflictTwo, conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepNewestUpdated,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(1);
            Check.That(resolutionSteps!.First()).IsInstanceOf<BlackboardCommandRejectAction>();
            Check.That(((BlackboardCommandRejectAction)resolutionSteps!.First()).SourceIssue).IsSameReferenceAs(issue);
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepOldest"/> strategy
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepOldest_One(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force past creation
                                                           newRecord.UTCCreationTime.AddDays(1),
                                                           null,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCLastUpdateTime.AddMinutes(-5),
                                                           null,
                                                           null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(1,
                                                                   new[] { conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepOldest,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(2);

            if (removeStrategy == BlackboardProcessingResolutionRemoveTypeEnum.Remove)
            {
                var remove = resolutionSteps!.First() as BlackboardCommandStorageRemoveRecord;

                Check.WithCustomMessage("First action must be removing the previous record").That(remove).IsNotNull();
                Check.That(remove!.Uid).IsEqualTo(conflictOne.Uid);
            }
            else
            {
                var deco = resolutionSteps!.First() as BlackboardCommandStorageDecommissionRecord;

                Check.WithCustomMessage("First action must be a decommission the previous record").That(deco).IsNotNull();
                Check.That(deco!.Uid).IsEqualTo(conflictOne.Uid);
            }

            var add = resolutionSteps!.Last() as BlackboardCommandStorageAddRecord<double>;
            Check.WithCustomMessage("Last action must be add new record").That(add).IsNotNull();
            Check.That(add!.Record).IsSameReferenceAs(newRecord);

            // Set value to true to force insertion
            Check.That(add!.Override).IsTrue();
            Check.That(add!.InsertIfNew).IsTrue();
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepOldest"/> strategy when pushed is alder
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepOldest_One_PushOlder(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force newer creation
                                                           newRecord.UTCCreationTime.AddDays(-1),
                                                           null,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCLastUpdateTime.AddMinutes(5),
                                                           null,
                                                           null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(1,
                                                                   new[] { conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepOldest,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(1);
            Check.That(resolutionSteps!.First()).IsInstanceOf<BlackboardCommandRejectAction>();
            Check.That(((BlackboardCommandRejectAction)resolutionSteps!.First()).SourceIssue).IsSameReferenceAs(issue);
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepOldest"/> strategy
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepOldest_Multiple(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force past creation
                                                           newRecord.UTCCreationTime.AddDays(1),
                                                           null,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCLastUpdateTime.AddMinutes(-5),
                                                           null,
                                                           null);

            var conflictTwo = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force After creation to be include in the keep ones
                                                           newRecord.UTCCreationTime.AddDays(-1),
                                                           null,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCLastUpdateTime.AddMinutes(5),
                                                           null,
                                                           null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(2,
                                                                   new[] { conflictTwo, conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepOldest,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(2);

            if (removeStrategy == BlackboardProcessingResolutionRemoveTypeEnum.Remove)
            {
                var remove = resolutionSteps!.First() as BlackboardCommandStorageRemoveRecord;

                Check.WithCustomMessage("First action must be removing the previous record").That(remove).IsNotNull();
                Check.That(remove!.Uid).IsEqualTo(conflictOne.Uid);
            }
            else
            {
                var deco = resolutionSteps!.First() as BlackboardCommandStorageDecommissionRecord;

                Check.WithCustomMessage("First action must be a decommission the previous record").That(deco).IsNotNull();
                Check.That(deco!.Uid).IsEqualTo(conflictOne.Uid);
            }

            var add = resolutionSteps!.Last() as BlackboardCommandStorageAddRecord<double>;
            Check.WithCustomMessage("Last action must be add new record").That(add).IsNotNull();
            Check.That(add!.Record).IsSameReferenceAs(newRecord);

            // Set value to true to force insertion
            Check.That(add!.Override).IsTrue();
            Check.That(add!.InsertIfNew).IsTrue();
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepOldest"/> strategy when pushed is alder
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepOldest_Multiple_PushOlder(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force newer creation
                                                           newRecord.UTCCreationTime.AddDays(-1),
                                                           null,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCLastUpdateTime.AddMinutes(5),
                                                           null,
                                                           null);

            var conflictTwo = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force newer creation
                                                           newRecord.UTCCreationTime.AddDays(-0.5),
                                                           null,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCLastUpdateTime.AddMinutes(5),
                                                           null,
                                                           null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(2,
                                                                   new[] { conflictTwo, conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepOldest,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(1);
            Check.That(resolutionSteps!.First()).IsInstanceOf<BlackboardCommandRejectAction>();
            Check.That(((BlackboardCommandRejectAction)resolutionSteps!.First()).SourceIssue).IsSameReferenceAs(issue);
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepOldestUpdated"/> strategy
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepOldest_Update_One(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCCreationTime.AddMinutes(-5),

                                                           null,

                                                           // Force past creation
                                                           newRecord.UTCLastUpdateTime.AddDays(1),
                                                           null,
                                                           null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(1,
                                                                   new[] { conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepOldestUpdated,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(2);

            if (removeStrategy == BlackboardProcessingResolutionRemoveTypeEnum.Remove)
            {
                var remove = resolutionSteps!.First() as BlackboardCommandStorageRemoveRecord;

                Check.WithCustomMessage("First action must be removing the previous record").That(remove).IsNotNull();
                Check.That(remove!.Uid).IsEqualTo(conflictOne.Uid);
            }
            else
            {
                var deco = resolutionSteps!.First() as BlackboardCommandStorageDecommissionRecord;

                Check.WithCustomMessage("First action must be a decommission the previous record").That(deco).IsNotNull();
                Check.That(deco!.Uid).IsEqualTo(conflictOne.Uid);
            }

            var add = resolutionSteps!.Last() as BlackboardCommandStorageAddRecord<double>;
            Check.WithCustomMessage("Last action must be add new record").That(add).IsNotNull();
            Check.That(add!.Record).IsSameReferenceAs(newRecord);

            // Set value to true to force insertion
            Check.That(add!.Override).IsTrue();
            Check.That(add!.InsertIfNew).IsTrue();
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepOldestUpdated"/> strategy when pushed is alder
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepOldest_Update_One_PushOlder(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCCreationTime.AddMinutes(5),

                                                           null,

                                                           // Force newer creation
                                                           newRecord.UTCLastUpdateTime.AddDays(-1),

                                                           null,
                                                           null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(1,
                                                                   new[] { conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepOldestUpdated,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(1);
            Check.That(resolutionSteps!.First()).IsInstanceOf<BlackboardCommandRejectAction>();
            Check.That(((BlackboardCommandRejectAction)resolutionSteps!.First()).SourceIssue).IsSameReferenceAs(issue);
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepOldestUpdated"/> strategy
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepOldest_Update_Multiple(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCCreationTime.AddMinutes(-5),

                                                           null,

                                                           // Force past creation
                                                           newRecord.UTCLastUpdateTime.AddDays(1),

                                                           null,
                                                           null);

            var conflictTwo = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force laster update to ensure that update date is used
                                                           newRecord.UTCCreationTime.AddMinutes(-5),

                                                           null,

                                                           // Force After update to be include in the keep ones
                                                           newRecord.UTCLastUpdateTime.AddDays(-1),

                                                           null,
                                                           null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(2,
                                                                   new[] { conflictTwo, conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepOldestUpdated,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(2);

            if (removeStrategy == BlackboardProcessingResolutionRemoveTypeEnum.Remove)
            {
                var remove = resolutionSteps!.First() as BlackboardCommandStorageRemoveRecord;

                Check.WithCustomMessage("First action must be removing the previous record").That(remove).IsNotNull();
                Check.That(remove!.Uid).IsEqualTo(conflictOne.Uid);
            }
            else
            {
                var deco = resolutionSteps!.First() as BlackboardCommandStorageDecommissionRecord;

                Check.WithCustomMessage("First action must be a decommission the previous record").That(deco).IsNotNull();
                Check.That(deco!.Uid).IsEqualTo(conflictOne.Uid);
            }

            var add = resolutionSteps!.Last() as BlackboardCommandStorageAddRecord<double>;
            Check.WithCustomMessage("Last action must be add new record").That(add).IsNotNull();
            Check.That(add!.Record).IsSameReferenceAs(newRecord);

            // Set value to true to force insertion
            Check.That(add!.Override).IsTrue();
            Check.That(add!.InsertIfNew).IsTrue();
        }

        /// <summary>
        /// Test resolution storage issue type limit with <see cref="BlackboardProcessingResolutionLimitTypeEnum.KeepOldestUpdated"/> strategy when pushed is alder
        /// </summary>
        [Theory]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Remove)]
        [InlineData(BlackboardProcessingResolutionRemoveTypeEnum.Decommission)]
        public async Task Storage_Resolution_Limit_Issue_KeepOldest_Update_Multiple_PushOlder(BlackboardProcessingResolutionRemoveTypeEnum removeStrategy)
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<RecordContainerTypeEnum>(() => RecordContainerTypeEnum.Direct);
            fixture.Register<RecordStatusEnum>(() => RecordStatusEnum.Ready);
            fixture.Register<ConcretTypeSurrogate?>(() => null);

            var grain = await fixture.CreateAndInitVGrain<DefaultBlackboardControllerGrain>();

            var cancellToken = new GrainCancellationTokenSource().Token;

            var newRecord = fixture.Create<DataRecordContainer<double>>();
            var conflictOne = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force laster update to ensure that update date is used
                                                           newRecord.UTCCreationTime.AddMinutes(5),

                                                           null,

                                                           // Force newer Update
                                                           newRecord.UTCLastUpdateTime.AddDays(-1),
                                                           null,
                                                           null);

            var conflictTwo = new BlackboardRecordMetadata(Guid.NewGuid(),
                                                           newRecord.LogicalType,
                                                           newRecord.LogicalType + "Conflict",
                                                           null,
                                                           RecordContainerTypeEnum.Direct,
                                                           RecordStatusEnum.Ready,

                                                           // Force laster update to ensure that create date is used
                                                           newRecord.UTCCreationTime.AddMinutes(5),

                                                           null,

                                                           // Force newer Update
                                                           newRecord.UTCLastUpdateTime.AddDays(-0.5),
                                                           null,
                                                           null);

            var issue = new MaxRecordBlackboardProcessingRuleIssue(2,
                                                                   new[] { conflictTwo, conflictOne },
                                                                   newRecord,
                                                                   BlackboardProcessingResolutionLimitTypeEnum.KeepOldestUpdated,
                                                                   removeStrategy);

            var resolutionSteps = await grain.ResolvePushIssueAsync(issue, newRecord, cancellToken);

            Check.That(resolutionSteps).IsNotNull().And.CountIs(1);
            Check.That(resolutionSteps!.First()).IsInstanceOf<BlackboardCommandRejectAction>();
            Check.That(((BlackboardCommandRejectAction)resolutionSteps!.First()).SourceIssue).IsSameReferenceAs(issue);
        }
    }
}
