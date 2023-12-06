// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Extensions methods about reflection tools
    /// </summary>
    public static class ReflectionExtensions
    {
        #region Fields

        private static readonly Dictionary<MethodInfo, string> s_methodUniqueId;
        private static readonly HashSet<string> s_methodUniqueIdHashset;

        private static readonly ReaderWriterLockSlim s_methodUniqueIdLocker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ReflectionExtensions"/> class.
        /// </summary>
        static ReflectionExtensions()
        {
            s_methodUniqueIdLocker = new ReaderWriterLockSlim();
            s_methodUniqueId = new Dictionary<MethodInfo, string>();
            s_methodUniqueIdHashset = new HashSet<string>();
        }

        #endregion

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <remarks>
        ///     This unique ID is only guarenty unique by <see cref="AppDomain"/>
        ///     We use TypeFullName + MethodFUllName hash by MD5.
        ///     We use cache to guaranty it's unique if the Hash produce is the same a Guid is introduce to add hazard
        /// </remarks>
        public static string GetUniqueId(this MethodInfo methodInfo, bool raisedErrorIfDuplicate = false)
        {
            s_methodUniqueIdLocker.EnterReadLock();

            try
            {
                if (s_methodUniqueId.TryGetValue(methodInfo, out var uniqueKey))
                    return uniqueKey;
            }
            finally
            {
                s_methodUniqueIdLocker.ExitReadLock();
            }

            s_methodUniqueIdLocker.EnterWriteLock();

            try
            {
                if (s_methodUniqueId.TryGetValue(methodInfo, out var uniqueKey))
                    return uniqueKey;

                string hashKey = string.Empty;
                bool cacheContainsHash = false;
                Guid? randomKeyPart = null;

                do
                {
                    if (cacheContainsHash)
                        randomKeyPart = Guid.NewGuid();

                    hashKey = BuildMethodUniqueId(methodInfo, randomKeyPart);

                    cacheContainsHash = s_methodUniqueIdHashset.Contains(hashKey);

                    if (raisedErrorIfDuplicate && cacheContainsHash)
                        throw new InvalidOperationException("Could not genenerate unique id for " + methodInfo + " because another method have the same hash =>" + s_methodUniqueId.First(m => m.Value == hashKey));
                }
                while (string.IsNullOrEmpty(hashKey) || cacheContainsHash);

                s_methodUniqueId.Add(methodInfo, hashKey);
                s_methodUniqueIdHashset.Add(hashKey);

                return hashKey;
            }
            finally
            {
                s_methodUniqueIdLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Builds the method unique identifier.
        /// </summary>
        private static string BuildMethodUniqueId(MethodInfo methodInfo, Guid? randomFactor)
        {
            var keyBuilder = new StringBuilder();

            keyBuilder.Append(methodInfo.DeclaringType?.FullName);
            keyBuilder.Append("_");
            keyBuilder.Append(methodInfo.ReturnType.FullName);
            keyBuilder.Append(methodInfo.Name);

            if (methodInfo.IsGenericMethod)
            {
                keyBuilder.Append("<");

                foreach (var geneParam in methodInfo.GetGenericArguments())
                {
                    keyBuilder.Append(geneParam);
                    keyBuilder.Append(",");
                }

                keyBuilder.Append(">");
            }

            keyBuilder.Append("(");

            foreach (var param in methodInfo.GetParameters())
            {
                keyBuilder.Append(param.ParameterType.FullName);
                keyBuilder.Append(",");
            }

            keyBuilder.Append(")");

            if (randomFactor.HasValue)
            {
                keyBuilder.Append(':');
                keyBuilder.Append(randomFactor.Value);
            }

            var fullKey = keyBuilder.ToString();

            using (var hash = SHA512.Create())
            {
                var keyBytes = Encoding.UTF8.GetBytes(fullKey);
                var hashKeyBytes = hash.ComputeHash(keyBytes);

                return Convert.ToBase64String(hashKeyBytes);
            }
        }
    }
}
