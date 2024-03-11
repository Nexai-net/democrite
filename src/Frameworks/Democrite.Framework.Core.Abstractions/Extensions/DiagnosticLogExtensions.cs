// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Extensions
{
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Elvex.Toolbox.Extensions;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class DiagnosticLogExtensions
    {
        #region Methods

        /// <summary>
        /// Converts to debug string display
        /// </summary>
        public static string ToDebugDisplayName(this IEnumerable<IDiagnosticLog> logs)
        {
            var parentIndx = logs.GroupBy(l => l.CallerId ?? Guid.Empty)
                                 .ToDictionary(k => k.Key, k => k.OrderBy(o => o.Orientation)
                                                                 .ThenBy(o => o.CreateOn)
                                                                 .ToReadOnly());

            return ToDebugDisplayName(parentIndx, Guid.Empty);
        }

        /// <summary>
        /// Converts to debug string display
        /// </summary>
        private static string ToDebugDisplayName(IReadOnlyDictionary<Guid, IReadOnlyCollection<IDiagnosticLog>> parentIndx, Guid parentId)
        {
            var builder = new StringBuilder();

            if (parentIndx.TryGetValue(parentId, out var logs))
            {
                foreach (var grp in logs.GroupBy(l => l.CurrentExecutionId))
                {
                    foreach (var log in grp)
                    {
                        builder.Append("* ");
                        builder.AppendLine(log.ToDebugDisplayName());
                    }

                    var childStr = ToDebugDisplayName(parentIndx, grp.Key).Replace("\n", "\n    ");
                    if (!string.IsNullOrEmpty(childStr))
                    {
                        builder.Append(' ', 4);
                        builder.AppendLine(childStr);
                    }
                }
            }

            return builder.ToString();
        }

        #endregion
    }
}
