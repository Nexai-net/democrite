// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Helpers
{
    using Democrite.Framework.Toolbox.Abstractions.Attributes;
    using Democrite.Framework.Toolbox.Abstractions.Enums;

    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    public static class LogicHelper
    {
        #region Fields

        private static readonly ImmutableDictionary<LogicEnum, char> s_logicAliases;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicHelper"/> class.
        /// </summary>
        static LogicHelper()
        {
            var type = typeof(LogicEnum);

            s_logicAliases = Enum.GetValues<LogicEnum>()
                                 .Where(l => l != LogicEnum.None)
                                 .ToImmutableDictionary(kv => kv,
                                                        kv =>
                                                        {
                                                            var member = type.GetMember(kv.ToString()).First();
                                                            return member.GetCustomAttribute<DescriptionWithAliasAttribute>()!
                                                                         .Alias;
                                                        });

            LogicOperators = s_logicAliases.Select(kv => kv.Value).Append('!').ToArray();
            LogicOperatorString = string.Join("", LogicOperators);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the logic operators.
        /// </summary>
        public static char[] LogicOperators { get; }

        /// <summary>
        /// Gets the logic operators.
        /// </summary>
        public static string LogicOperatorString { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the symbol from <see cref="LogicEnum"/>.
        /// </summary>
        public static char? GetSymbolFrom(LogicEnum logicEnum)
        {
            if (logicEnum == LogicEnum.None)
                return null;

            return s_logicAliases[logicEnum];
        }

        /// <summary>
        /// Gets <see cref="LogicEnum"/> value from character
        /// </summary>
        public static LogicEnum? GetLogicValueFrom(char logicValue)
        {
            var kv = s_logicAliases.FirstOrDefault(k => k.Value == logicValue);

            if (kv.Key == LogicEnum.None)
                return null;

            return kv.Key;
        }

        #endregion
    }
}
