// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Validators
{
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Solver used to always validate to ensure solver is not null
    /// </summary>
    /// <seealso cref="IBlackboardDataLogicalTypeRuleSolver" />
    public sealed class NullBlackboardDataLogicalTypeRuleSolver : IBlackboardDataLogicalTypeRuleSolver
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="NullBlackboardDataLogicalTypeRuleSolver"/> class.
        /// </summary>
        static NullBlackboardDataLogicalTypeRuleSolver()
        {
            Instance = new NullBlackboardDataLogicalTypeRuleSolver();   
        }

        private NullBlackboardDataLogicalTypeRuleSolver()
        {
            
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static NullBlackboardDataLogicalTypeRuleSolver Instance { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Allways Validate
        /// </summary>
        public ValueTask<BlackboardProcessingIssue?> ValidateAsync<TData>(DataRecordContainer<TData> data,
                                                                          IReadOnlyDictionary<Guid, BlackboardRecordMetadata> recordMetadata)
        {
            return ValueTask.FromResult<BlackboardProcessingIssue?>(null);
        }

        #endregion
    }
}
