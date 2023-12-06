// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Helpers
{
    using System;

    /// <summary>
    /// Helper about  formula logical or/and mathematical
    /// </summary>
    public static class FormulaHelper
    {
        #region Methods

        /// <summary>
        /// Generates the name of the variable.
        /// Convert <paramref name="indx"/> into base 26
        /// </summary>
        public static string GenerateVariableName(uint indx, ushort variableNameSize = 0)
        {
            variableNameSize = Math.Max(variableNameSize, (ushort)((indx / 26) + 1));

            Span<char> sb = stackalloc char[variableNameSize];
            sb.Fill('A');

            for (var i = variableNameSize - 1; i > -1; i--)
            {
                sb[i] = (char)('A' + (indx % 26));
                indx /= 26;

                if (indx <= 0)
                    break;
            }

            if (indx > 0)
                throw new InvalidOperationException("Could not generate a variable name long enought to support number " + indx);

            return sb.ToString();
        }

        #endregion
    }
}
