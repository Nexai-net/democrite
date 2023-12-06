// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class ExceptionExtensions
    {
        /// <summary>
        /// Gets full exception string with inner
        /// </summary>
        public static string GetFullString(this Exception exception)
        {
            var builder = new StringBuilder();

            var exceptionType = exception.GetType();

            var pad = string.Empty.PadLeft(4);

            builder.Append(exceptionType.Name);
            builder.Append(" : ");
            builder.Append(exception.Message?.Replace("\n", $"\n{pad}"));

            var inners = new HashSet<Exception>();

            if (exception.InnerException != null)
                inners.Add(exception.InnerException);

            if (exception is AggregateException aggr && aggr.InnerExceptions != null && aggr.InnerExceptions.Any())
            {
                foreach (var innerAggrException in aggr.InnerExceptions)
                    inners.Add(innerAggrException);
            }

            if (inners.Any())
            {
                foreach (var inner in inners)
                {
                    var innerString = GetFullString(inner);
                    builder.Append(pad);
                    builder.Append(innerString?.Replace("\n", $"\n{pad}"));
                }
            }

            return builder.ToString();
        }
    }
}
