// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.VGrains.Transformers
{
    using Democrite.Framework.Core.Abstractions;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;

    using System;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    [StatelessWorker]
    [Guid("613277F0-DC2B-4211-8E10-6CAA03D7B99A")]
    public sealed class TestExtractEmailTransformer : ITestExtractEmailTransformer
    {
        #region Fields

        private static readonly Regex s_emailRegex = new Regex(@"([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly ILogger<TestExtractEmailTransformer> _logger;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TestExtractEmailTransformer"/> class.
        /// </summary>
        public TestExtractEmailTransformer(ILogger<TestExtractEmailTransformer> logger)
        {
            this._logger = logger;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<string[]> ExtractEmailsAsync(string input, IExecutionContext context)
        {
            ArgumentNullException.ThrowIfNull(input);

            var results = s_emailRegex.Matches(input)
                                      .Where(m => m.Success)
                                      .Select(m => m.Value)
                                      .ToArray();

            return Task.FromResult(results);
        }

        #endregion
    }
}
