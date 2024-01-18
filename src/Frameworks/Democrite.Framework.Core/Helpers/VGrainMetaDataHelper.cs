// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Helpers
{
    using Democrite.Framework.Core.Abstractions.Attributes;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper used to provider meta data about an vgrain
    /// </summary>
    internal static class VGrainMetaDataHelper
    {
        #region Fields

        private static readonly IReadOnlyCollection<Regex> s_democriteSystemVGrainIdentifer;
        private static readonly IReadOnlyCollection<Regex> s_systemVGrainIdentifer;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="VGrainMetaDataHelper"/> class.
        /// </summary>
        static VGrainMetaDataHelper()
        {
            // Regex used to identify system/framework vgrain using ArtifactType.AssemblyQualifiedName or ArtifactType.Assembly.FullName
            s_systemVGrainIdentifer = new[]
            {
                new Regex(",\\sOrleans.Core[a-zA-Z.]*,\\sVersion=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(",\\sOrleans.Persistence[a-zA-Z.]*,\\sVersion=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(",\\sOrleans.Runtime[a-zA-Z.]*,\\sVersion=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(",\\sOrleans.Reminders[a-zA-Z.]*,\\sVersion=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(",\\sOrleans.Streaming[a-zA-Z.]*,\\sVersion=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new Regex(",\\sOrleans.TestingHost[a-zA-Z.]*,\\sVersion=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            };

            s_democriteSystemVGrainIdentifer = new[]
{
                new Regex(",\\sDemocrite.Framework[a-zA-Z.]*,\\sVersion=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the service implementation <paramref name="impl"/> is a system/framework vgrain needed to the well working of democrite/orleans
        /// </summary>
        public static bool IsSystemVGrain(Type impl)
        {
            return IsOrleanSystemVGrain(impl) || IsDemocriteSystemVGrain(impl);
        }

        /// <summary>
        /// Determines whether the service implementation <paramref name="impl"/> is a system/framework vgrain needed to the well working of democrite
        /// </summary>
        public static bool IsOrleanSystemVGrain(Type impl)
        {
            return s_systemVGrainIdentifer.Any(r => !string.IsNullOrEmpty(impl.AssemblyQualifiedName) && r.IsMatch(impl.AssemblyQualifiedName)) ||
                   s_systemVGrainIdentifer.Any(r => !string.IsNullOrEmpty(impl.Assembly.FullName) && r.IsMatch(impl.Assembly.FullName));
        }

        /// <summary>
        /// Determines whether the service implementation <paramref name="impl"/> is a system/framework vgrain needed to the well working of democrite
        /// </summary>
        public static bool IsDemocriteSystemVGrain(Type impl)
        {
            return impl.GetCustomAttribute<DemocriteSystemVGrainAttribute>() != null ||
                   s_democriteSystemVGrainIdentifer.Any(r => !string.IsNullOrEmpty(impl.AssemblyQualifiedName) && r.IsMatch(impl.AssemblyQualifiedName)) ||
                   s_democriteSystemVGrainIdentifer.Any(r => !string.IsNullOrEmpty(impl.Assembly.FullName) && r.IsMatch(impl.Assembly.FullName));
        }

        #endregion
    }
}
