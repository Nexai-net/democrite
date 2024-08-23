// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Models
{
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Storages;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;

    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Disposables;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal sealed class BlackboardLogicalTypeHandler : SafeDisposable, IEquatable<string>, IEquatable<BlackboardLogicalTypeHandler>
    {
        #region Fields

        private readonly Regex _logicalTypeFilter;
        private readonly string _logicalType;

        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ReaderWriterLockSlim _quickRepoLockerAccess;
        private readonly SemaphoreSlim _asyncGetLocker;

        private BlackboardStorageDefinition _storage = null!;

        private IRepository<DataRecordContainer, Guid>? _repository;
        private bool? _repositoryNeedInitialization;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardLogicalTypeHandler"/> class.
        /// </summary>
        public BlackboardLogicalTypeHandler(string logicalType,
                                            IRepositoryFactory repositoryFactory)
        {
            this._repositoryFactory = repositoryFactory;

            // this locker is used to allow a quick access after the initialization
            this._quickRepoLockerAccess = new ReaderWriterLockSlim();

            // this locker ensure the repository is get and init only once
            // We choose a semaphore before the init process in async
            this._asyncGetLocker = new SemaphoreSlim(1);

            this._logicalType = logicalType;
            this._logicalTypeFilter = new Regex(logicalType, RegexOptions.Compiled);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the order.
        /// </summary>
        public short Order { get; private set; }

        /// <summary>
        /// Gets the rule solver.
        /// </summary>
        public IBlackboardDataLogicalTypeRuleSolver? RuleSolver { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [remain on sealed].
        /// </summary>
        public bool RemainOnSealed { get; private set; }

        #endregion

        #region Nested

        public sealed class BlackboardLogicalTypeHandlerComparer : IComparer<BlackboardLogicalTypeHandler>
        {
            #region Ctor

            /// <summary>
            /// Initializes the <see cref="BlackboardLogicalTypeHandlerComparer"/> class.
            /// </summary>
            static BlackboardLogicalTypeHandlerComparer()
            {
                Default = new BlackboardLogicalTypeHandlerComparer();
            }

            /// <summary>
            /// Prevents a default instance of the <see cref="BlackboardLogicalTypeHandlerComparer"/> class from being created.
            /// </summary>
            private BlackboardLogicalTypeHandlerComparer()
            {
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets the default.
            /// </summary>
            public static BlackboardLogicalTypeHandlerComparer Default { get; }

            #endregion

            #region Methods

            /// <inheritdoc />
            int IComparer<BlackboardLogicalTypeHandler>.Compare(BlackboardLogicalTypeHandler? x, BlackboardLogicalTypeHandler? y)
            {
                return (x?.Order ?? -2) - (y?.Order ?? -2);
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the repository.
        /// </summary>
        public async ValueTask<IRepository<DataRecordContainer, Guid>> GetRepositoryAsync(CancellationToken token)
        {
            this._quickRepoLockerAccess.EnterReadLock();
            try
            {
                var repo = this._repository;
                var needInit = this._repositoryNeedInitialization;

                if (repo is not null && needInit is not null && (needInit == false || ((ISupportInitialization<string>)repo).IsInitialized))
                    return repo;
            }
            finally
            {
                this._quickRepoLockerAccess.ExitReadLock();
            }

            return await RepositoryPreparationAsync(token);
        }

        /// <summary>
        /// Matches the specified logical type.
        /// </summary>
        internal bool Match(string? logicalType)
        {
            logicalType ??= string.Empty;
            return this._logicalTypeFilter.IsMatch(logicalType);
        }

        /// <summary>
        /// Updates 
        /// </summary>
        internal void Update(IBlackboardDataLogicalTypeRuleValidatorProvider blackboardDataLogicalTypeRuleValidatorProvider,
                             BlackboardOrderLogicalTypeRule? order,
                             BlackboardStorageDefinition storage,
                             BlackboardRemainOnSealedLogicalTypeRule? remain,
                             IReadOnlyCollection<BlackboardLogicalTypeBaseRule>? rules)
        {
            this.Order = order?.Order ?? -1;

            var oldStorage = this._storage;
            this._storage = storage;

            this.RemainOnSealed = remain is not null;

            if (rules is not null && rules.Any())
            {
                this.RuleSolver = blackboardDataLogicalTypeRuleValidatorProvider.Create(this._logicalTypeFilter, rules);
            }
            else
            {
                this.RuleSolver = null;
            }

            if (this._storage?.Equals(oldStorage) ?? oldStorage is null)
                return;

            // Storage change
            this._asyncGetLocker.Wait();
            try
            {
                // Reset repository to force a re-init with new storage informations
                this._repository = null;
                this._repositoryNeedInitialization = null;
            }
            finally
            {
                this._asyncGetLocker.Release();
            }
        }

        /// <inheritdoc />
        public bool Equals(string? other)
        {
            if (string.IsNullOrEmpty(other))
                return false;

            return string.Equals(other, this._logicalType);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is BlackboardLogicalTypeHandler otherHandler)
                return Equals(otherHandler);

            if (obj is string otherHandlerStr)
                return Equals(otherHandlerStr);

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this._logicalTypeFilter.GetHashCode();
        }

        /// <inheritdoc />
        public bool Equals(BlackboardLogicalTypeHandler? other)
        {
            return Equals(other?._logicalType);
        }

        /// <inheritdoc />
        protected override void DisposeEnd()
        {
            this._asyncGetLocker.Dispose();
            this._quickRepoLockerAccess.Dispose();
            base.DisposeEnd();
        }

        #region Tools

        /// <summary>
        /// Get and initializa the repository if needed
        /// </summary>
        private async ValueTask<IRepository<DataRecordContainer, Guid>> RepositoryPreparationAsync(CancellationToken token)
        {
            await this._asyncGetLocker.WaitAsync(token);
            try
            {
                if (this._repository is null)
                {
                    var request = new RepositoryGetOptions(this._storage.StorageKey, false, this._storage.StorageConfiguration);
                    var repository = this._repositoryFactory.Get<IRepository<DataRecordContainer, Guid>, DataRecordContainer, Guid>(request, cancellationToken: token);

                    if (repository is ISupportInitialization<string> initStrRepo && !initStrRepo.IsInitialized)
                        await initStrRepo.InitializationAsync(this._storage.StorageKey, token);
                    else if (repository is ISupportInitialization initRepo && !initRepo.IsInitialized)
                        await initRepo.InitializationAsync(token);

                    this._quickRepoLockerAccess.EnterWriteLock();
                    try
                    {
                        this._repository = (IRepository<DataRecordContainer, Guid>)repository;
                        this._repositoryNeedInitialization = repository is ISupportInitialization<string>;
                    }
                    finally
                    {
                        this._quickRepoLockerAccess.ExitWriteLock();
                    }
                }

                if (this._repositoryNeedInitialization! == true && this._repository is ISupportInitialization<string> init && !init.IsInitialized)
                {
                    await init.InitializationAsync(this._storage.StorageKey, token);
                }
            }
            finally
            {
                this._asyncGetLocker.Release();
            }

            return this._repository;
        }

        #endregion

        #endregion
    }
}
