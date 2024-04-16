// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Models
{
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

        private IRepository<DataRecordContainer, Guid> _repository = null!;
        private BlackboardStorageDefinition _storage = null!;
        private bool _repositoryNeedInitialization;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardLogicalTypeHandler"/> class.
        /// </summary>
        public BlackboardLogicalTypeHandler(string logicalType)
        {
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
        public ValueTask<IRepository<DataRecordContainer, Guid>> GetRepositoryAsync(CancellationToken token)
        {
            if (this._repositoryNeedInitialization && this._repository is ISupportInitialization<string> init && !init.IsInitialized)
            {
                var initTask = Task.Run(async () =>
                {
                    await init.InitializationAsync(this._storage.StorageKey, token);
                    return this._repository;
                });

                return new ValueTask<IRepository<DataRecordContainer, Guid>>(initTask);
            }
            return ValueTask.FromResult(this._repository);
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
        internal void Update(IRepositoryFactory repositoryFactory,
                             IBlackboardDataLogicalTypeRuleValidatorProvider blackboardDataLogicalTypeRuleValidatorProvider,
                             BlackboardOrderLogicalTypeRule? order,
                             BlackboardStorageDefinition storage,
                             BlackboardRemainOnSealedLogicalTypeRule? remain,
                             IReadOnlyCollection<BlackboardLogicalTypeBaseRule>? rules)
        {
            this.Order = order?.Order ?? -1;
            this._repository = repositoryFactory.Get<IRepository<DataRecordContainer, Guid>, DataRecordContainer>(storage.StorageKey, storage.StorageConfiguration);

            this._storage = storage;

            this._repositoryNeedInitialization = this._repository is ISupportInitialization<string>;

            this.RemainOnSealed = remain is not null;

            if (rules is not null && rules.Any())
            {
                this.RuleSolver = blackboardDataLogicalTypeRuleValidatorProvider.Create(this._logicalTypeFilter, rules);
            }
            else
            {
                this.RuleSolver = null;
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

        #endregion
    }
}
