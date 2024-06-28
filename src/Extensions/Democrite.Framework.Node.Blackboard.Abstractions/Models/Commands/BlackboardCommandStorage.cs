// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Base class of any storage action
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public abstract record class BlackboardCommandStorage(BlackboardCommandStorageActionTypeEnum StorageAction) : BlackboardCommand(BlackboardCommandTypeEnum.Storage);

    /// <summary>
    /// Command in charge to add element
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="InsertIfNew">Allow creation if doesn't exist.</param>
    /// <param name="@override">Allow update if exist (Same DeferredId).</param>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class BlackboardCommandStorageAddRecord<TData>(DataRecordContainer<TData?> Record, bool InsertIfNew, bool @Override) : BlackboardCommandStorage(BlackboardCommandStorageActionTypeEnum.Add)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.StorageAction}] (InsertIfNew: {this.InsertIfNew}, @Override : {this.@Override}) {this.Record.ToDebugDisplayName()}";
        }
    }

    /// <summary>
    /// Command in charge to prepare a slot element element
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class BlackboardCommandStoragePrepareRecord(Guid Uid, string LogicType, string DisplayName) : BlackboardCommandStorage(BlackboardCommandStorageActionTypeEnum.Prepare)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.StorageAction}] (Uid: {this.Uid}, LogicType : {this.LogicType}) {this.DisplayName}";
        }
    }

    /// <summary>
    /// Command in charge to change status an element by his uid
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class BlackboardCommandStorageChangeStatusRecord(Guid Uid, RecordStatusEnum NewRecordStatus) : BlackboardCommandStorage(BlackboardCommandStorageActionTypeEnum.ChangeStatus)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.StorageAction}] - change status record - {this.Uid} new {this.NewRecordStatus}";
        }
    }

    /// <summary>
    /// Command in charge to change a record metadata
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class BlackboardCommandStorageChangeMetaData(Guid Uid, RecordMetadata NewMetaData) : BlackboardCommandStorage(BlackboardCommandStorageActionTypeEnum.ChangeMetaData)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.StorageAction}] - change record metadata - {this.Uid} new {this.NewMetaData}";
        }
    }

    /// <summary>
    /// Command in charge to remove an element by his uid
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class BlackboardCommandStorageRemoveRecord(Guid Uid) : BlackboardCommandStorage(BlackboardCommandStorageActionTypeEnum.Remove)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.StorageAction}] - remove record - {this.Uid}";
        }
    }

    /// <summary>
    /// Command in charge to Decommissione an element by his uid
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class BlackboardCommandStorageDecommissionRecord(Guid Uid) : BlackboardCommandStorage(BlackboardCommandStorageActionTypeEnum.Decommission)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.StorageAction}] - Decommission record - {this.Uid}";
        }
    }
}
