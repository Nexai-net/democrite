// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace System
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class ExceptionExtensions
    {
        /// <summary>
        /// Gets full exception string with inner
        /// </summary>
        public static string GetFullString(this Exception exception, bool includeData = false)
        {
            if (exception is null)
                return string.Empty;

            var builder = new StringBuilder();

            var exceptionType = exception.GetType();

            var pad = string.Empty.PadLeft(4);

            builder.Append(exceptionType.Name);
            builder.Append(" : ");
            builder.Append(exception.Message?.Replace("\n", $"\n{pad}"));

            if (includeData && exception.Data is not null && exception.Data.Count > 0)
            {
                var data = exception.Data;

                builder.AppendLine();
                builder.Append("-- Data (");
                builder.Append(data.Count);
                builder.Append("): --");

                foreach (object key in data.Keys)
                {
                    var value = data[key];

                    builder.AppendLine();
                    builder.Append(' ', 2);
                    builder.Append(key);
                    builder.Append(" : ");
                    builder.Append(value?.ToString() ?? "null");
                }
            }

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
                    builder.AppendLine();
                    var innerString = GetFullString(inner, includeData);
                    builder.Append(pad);
                    builder.Append(innerString?.Replace("\n", $"\n{pad}"));
                }
            }

            return builder.ToString();
        }
    }
}
