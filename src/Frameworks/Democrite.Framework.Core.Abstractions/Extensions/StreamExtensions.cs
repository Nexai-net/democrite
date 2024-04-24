// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Keep : System
namespace System
{
    using System.Threading.Tasks;

    public static class StreamExtensions
    {
        /// <summary>
        /// Diffuses the line asynchronous.
        /// </summary>
        public static Task StreamLinesAsync(this StreamReader reader, Action<string> action, CancellationToken token = default, Func<bool>? finished = null)
        {
            return Task.Run(() =>
            {
                do
                {
                    var line = reader.ReadLine();

                    if (!string.IsNullOrEmpty(line))
                        action(line);

                    if (reader.BaseStream.Length == 0 && (finished is null || finished()))
                        break;

                } while (reader.EndOfStream);
            }, token);
        }
    }
}
