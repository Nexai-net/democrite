// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Extensions
{
    using Democrite.Framework.Toolbox.Abstractions.Expressions;
    using Democrite.Framework.Toolbox.Helpers;

    using System.Linq.Expressions;

    public static class AccessExpressionDefinitionExtensions
    {
        /// <summary>
        /// Try resolve configuration based on <paramref name="input"/> and <paramref name="lambdaBaseConfiguration"/>
        /// </summary>
        public static object? Resolve(this AccessExpressionDefinition accessConfiguration, object? input)
        {
            if (!string.IsNullOrEmpty(accessConfiguration.ChainCall))
                return DynamicCallHelper.GetValueFrom(input, accessConfiguration.ChainCall, true);

            if (accessConfiguration.MemberInit is not null)
            {
                // Opti: May be cache the lambda function 
                var expr = accessConfiguration.MemberInit.ToMemberInitializationLambdaExpression();

                if (expr.Parameters.Any())
                    return expr.Compile().DynamicInvoke(input);

                return expr.Compile().DynamicInvoke();
            }

            return accessConfiguration.DirectObject?.GetValue();
        }
    }
}
