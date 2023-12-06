// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Helpers
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Helper used to perform runtime dynamic call
    /// </summary>
    public static class DynamicCallHelper
    {
        /// <summary>
        /// Helper able to get a value in a object tree using the calling path.
        /// </summary>
        /// <remarks>
        ///     Use reflection without caching
        /// </remarks>
        public static object? GetValueFrom(object inst, string callChain, bool throwIfNotFounded = true)
        {
            ReadOnlySpan<char> callChainsSpan = callChain;
            return GetValueFrom(inst, callChainsSpan, throwIfNotFounded);
        }

        /// <summary>
        /// Helper able to get a value in a object tree using the calling path.
        /// </summary>
        /// <remarks>
        ///     Use reflection without caching
        /// </remarks>
        public static object? GetValueFrom(object inst, ReadOnlySpan<char> callChain, bool throwIfNotFounded = true)
        {
            if (inst is null)
            {
                if (throwIfNotFounded)
                    throw new ArgumentNullException(nameof(inst));

                return null;
            }

            var trait = inst.GetType();

            var prop = callChain;
            ReadOnlySpan<char> tail = default;

            var dotIndexOf = callChain.IndexOf('.');

            if (dotIndexOf > -1)
            {
                prop = callChain.Slice(0, dotIndexOf);
                tail = callChain.Slice(dotIndexOf + 1);
            }

            PropertyInfo? info = null;

            foreach (var property in trait.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (prop.Equals(property.Name, StringComparison.OrdinalIgnoreCase))
                {
                    info = property;
                    break;
                }
            }

            if (info == null && throwIfNotFounded)
                throw new ArgumentException("Missing or null call chain  part '" + prop.ToString() + "'");

            var resolvedValue = info?.GetValue(inst, null);

            if (tail.Length > 0 && resolvedValue is not null)
                return GetValueFrom(resolvedValue, tail, throwIfNotFounded);

            return resolvedValue;
        }
    }
}