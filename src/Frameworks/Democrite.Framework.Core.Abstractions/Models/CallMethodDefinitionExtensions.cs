// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Models
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    public static class CallMethodDefinitionExtensions
    {
        #region Methods

        /// <summary>
        /// Converts to definition.
        /// </summary>
        public static CallMethodDefinition ToCallDefinition(this MethodInfo methodInfo)
        {
            ArgumentNullException.ThrowIfNull(methodInfo);

            var genericTypes = methodInfo.GetGenericArguments();
            var mthd = methodInfo;

            if (methodInfo.IsGenericMethod && !methodInfo.IsGenericMethodDefinition)
                mthd = methodInfo.GetGenericMethodDefinition();

            return new CallMethodDefinition(mthd.Name,
                                            mthd.ReturnType,
                                            mthd.GetParameters().Select(p => p.ParameterType),
                                            mthd.DeclaringType,
                                            genericTypes);
        }

        #endregion
    }
}
