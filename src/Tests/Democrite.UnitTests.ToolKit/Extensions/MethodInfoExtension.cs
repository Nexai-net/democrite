// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Extensions
{
    using Moq;

    using System.Linq;
    using System.Reflection;

    internal static class MethodInfoExtension
    {
        /// <summary>
        /// Gets the default generic implementation method.
        /// </summary>
        /// <remarks>
        ///     Resolve Generic defintion by constrains or by <see cref="It.IsAnyType"/>
        /// </remarks>
        public static MethodInfo GetDefaultGenericImplementationMethod(this MethodInfo method)
        {
            if (method.IsGenericMethod)
            {
                var defaultGenericTypes = method.GetGenericArguments()
                                                .Select(t => t.GetGenericParameterConstraints().FirstOrDefault(t => t.IsInterface) ?? typeof(It.IsAnyType))
                                                .ToArray();

                method = method.MakeGenericMethod(defaultGenericTypes);
            }

            return method;
        }
    }
}
