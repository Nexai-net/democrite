// Copyright (c) Nexai.
// This file is licenses to you under the MIT license.
// Produce by nexai, elvexoft & community (cf. docs/Teams.md)

// Keep : Democrite.Framework.Node.Blackboard.Abstractions.Models
namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;

    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Abstractions.Supports;

    using System.Linq.Expressions;
    using System.Reflection;

    public static class BlackboardRecordExtensions
    {
        #region Fields
        
        private static readonly MethodInfo s_createGenericMethod;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="BlackboardRecordExtensions"/> class.
        /// </summary>
        static BlackboardRecordExtensions()
        {
            Expression<Func<DataRecordContainer<int>, BlackboardCommandStorageAddRecord<int>>> createGenericMethod = d => CreateAddCommandGeneric<int>(d, false, false);

            s_createGenericMethod = ((MethodCallExpression)createGenericMethod.Body).Method.GetGenericMethodDefinition();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create an update from an existing <paramref name="origin"/>
        /// </summary>
        public static DataRecordContainer<TData> Update<TData>(this DataRecordContainer<TData> origin,
                                                               Func<TData?, TData> update,
                                                               ITimeManager timeManager,
                                                               RecordStatusEnum? newRecordStatus = null,
                                                               RecordMetadata? customMetadata = null,
                                                               IIdentityCard? identity = null)
        {
            return new DataRecordContainer<TData>(origin.LogicalType,
                                                  origin.Uid,
                                                  origin.DisplayName,
                                                  update(origin.Data),
                                                  newRecordStatus ?? RecordStatusEnum.Ready,
                                                  origin.UTCCreationTime,
                                                  origin.CreatorIdentity,
                                                  timeManager.UtcNow,
                                                  identity?.ToString(), // TODO : Get identifier SAM
                                                  customMetadata); 
        }

        /// <summary>
        /// Create an update from an existing <paramref name="origin"/>
        /// </summary>
        public static DataRecordContainer<TData> CreateNewDataRecord<TData>(this TData origin,
                                                                            string logicalType,
                                                                            string? displayName,
                                                                            ITimeManager timeManager,
                                                                            RecordStatusEnum? newRecordStatus = null,
                                                                            IIdentityCard? identity = null,
                                                                            RecordMetadata? recordMetadata = null)
        {
            return new DataRecordContainer<TData>(logicalType,
                                                  Guid.NewGuid(),
                                                  
                                                  !string.IsNullOrEmpty(displayName) 
                                                        ? displayName
                                                        : (origin is ISupportDebugDisplayName debugDisplayName)
                                                            ? debugDisplayName.ToDebugDisplayName()
                                                            : origin?.ToString() ?? typeof(TData).Name,

                                                  origin,
                                                  newRecordStatus ?? RecordStatusEnum.Ready,
                                                  timeManager.UtcNow,
                                                  identity?.ToString(), // TODO : Get identifier SAM,
                                                  timeManager.UtcNow,
                                                  identity?.ToString(),
                                                  recordMetadata); // TODO : Get identifier SAM
        }

        /// <summary>
        /// Creates the add command from <see cref="DataRecordContainer"/>
        /// </summary>
        public static BlackboardCommand CreateAddCommand(this DataRecordContainer dataRecord, bool insertIfNew = true, bool @override = true)
        {
            ArgumentNullException.ThrowIfNull(dataRecord);

            if (dataRecord.RecordContainerType != RecordContainerTypeEnum.Direct)
                throw new InvalidCastException("Data Record must by type Direct to exposed the data to add");

            return (BlackboardCommand)s_createGenericMethod.MakeGenericMethod(dataRecord.ContainsType!.ToType()).Invoke(null, new object?[] { dataRecord, insertIfNew, @override })!;
        }

        #region Tools
        internal static BlackboardCommandStorageAddRecord<TData> CreateAddCommandGeneric<TData>(this DataRecordContainer<TData?> dataRecord, bool InsertIfNew = true, bool @Override = true)
        {
            ArgumentNullException.ThrowIfNull(dataRecord);

            if (dataRecord.RecordContainerType != RecordContainerTypeEnum.Direct)
                throw new InvalidCastException("Data Record must by type Direct to exposed the data to add");

            return new BlackboardCommandStorageAddRecord<TData>(dataRecord, InsertIfNew, @Override);
        }

        #endregion
     
        #endregion
    }
}
