// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Helpers
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Models;

    using Orleans.Concurrency;

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

        private static readonly Type s_vgrainTrait = typeof(IVGrain);
        private static readonly Type s_intergerComponedStrIdKeyTrait;
        private static readonly Type s_guidComponedStrIdKeyTrait;
        private static readonly Type s_intergerIdKeyTrait;
        private static readonly Type s_stringIdKeyTrait;
        private static readonly Type s_guidIdKeyTrait;

        private static readonly IReadOnlyCollection<Regex> s_democriteSystemVGrainIdentifer;
        private static readonly IReadOnlyCollection<Regex> s_systemVGrainIdentifer;

        private static readonly Dictionary<Type, VGrainMetaData> s_metadataCache;
        private static readonly ReaderWriterLockSlim s_metaDataCacheLocker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="VGrainMetaDataHelper"/> class.
        /// </summary>
        static VGrainMetaDataHelper()
        {
            s_stringIdKeyTrait = typeof(IGrainWithStringKey);
            s_guidIdKeyTrait = typeof(IGrainWithGuidKey);
            s_guidComponedStrIdKeyTrait = typeof(IGrainWithGuidCompoundKey);
            s_intergerIdKeyTrait = typeof(IGrainWithIntegerKey);
            s_intergerComponedStrIdKeyTrait = typeof(IGrainWithIntegerCompoundKey);

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

            s_metadataCache = new Dictionary<Type, VGrainMetaData>();
            s_metaDataCacheLocker = new ReaderWriterLockSlim();
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

        /// <summary>
        /// Gets VGrain meta data.
        /// </summary>
        public static VGrainMetaData GetVGrainMetaDataType<TGrain>()
            where TGrain : IVGrain
        {
            return GetVGrainMetaDataType(typeof(TGrain));
        }

        /// <summary>
        /// Gets VGrain meta data.
        /// </summary>
        public static VGrainMetaData GetVGrainMetaDataType(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            s_metaDataCacheLocker.EnterReadLock();
            try
            {
                if (s_metadataCache.TryGetValue(type, out var metaData))
                    return metaData;
            }
            finally
            {
                s_metaDataCacheLocker.ExitReadLock();
            }

            s_metaDataCacheLocker.EnterWriteLock();
            try
            {
                if (s_metadataCache.TryGetValue(type, out var metaData))
                    return metaData;

                if (type.IsAssignableTo(s_vgrainTrait) == false)
                    throw new InvalidCastException($"Type {type} must be a IVGrain");

                var isDemcriteSystemVGrain = type.GetCustomAttribute<DemocriteSystemVGrainAttribute>();
                var categories = type.GetCustomAttributes<VGrainCategoryAttribute>();

                var interfaces = new List<string>();
                var idFormats = new List<IdFormatTypeEnum>();

                Type? mainInterface = null;
                bool haveState = false;

                foreach (var compatibleType in type.GetTypeInfoExtension().GetAllCompatibleTypes())
                {
                    if (compatibleType.IsInterface && compatibleType.IsAssignableTo(s_vgrainTrait) && compatibleType != s_vgrainTrait)
                    {
                        interfaces.Add(compatibleType.Name);
                        continue;
                    }

                    if (compatibleType.Name.StartsWith("VGrainBase`"))
                    {
                        var args = compatibleType.GetGenericArguments();
                        mainInterface = args.Last();
                        haveState |= args.Length > 1;
                    }

                    if (compatibleType == s_guidIdKeyTrait)
                        idFormats.Add(IdFormatTypeEnum.Guid);

                    if (compatibleType == s_guidComponedStrIdKeyTrait)
                        idFormats.Add(IdFormatTypeEnum.CompositionGuidString);

                    if (compatibleType == s_intergerIdKeyTrait)
                        idFormats.Add(IdFormatTypeEnum.Long);

                    if (compatibleType == s_intergerComponedStrIdKeyTrait)
                        idFormats.Add(IdFormatTypeEnum.CompositionLongString);

                    if (compatibleType == s_stringIdKeyTrait)
                        idFormats.Add(IdFormatTypeEnum.String);
                }

                var newMetaData = new VGrainMetaData(interfaces.Distinct(),
                                                     mainInterface?.Name,
                                                     haveState,
                                                     categories?.Select(c => c.Category).Where(c => !string.IsNullOrEmpty(c)),
                                                     idFormats,
                                                     
                                                     (mainInterface?.GetCustomAttribute<VGrainIdSingletonAttribute>() ?? type.GetCustomAttribute<VGrainIdSingletonAttribute>()) is not null,
                                                     
                                                     (type.GetCustomAttribute<StatelessWorkerAttribute>() is not null || type.GetCustomAttribute<VGrainStatelessWorkerAttribute>() is not null ||
                                                        mainInterface?.GetCustomAttribute<VGrainStatelessWorkerAttribute>() is not null),

                                                     isDemcriteSystemVGrain is not null);

                s_metadataCache.Add(type, newMetaData);
                return newMetaData;
            }
            finally
            {
                s_metaDataCacheLocker.ExitWriteLock();
            }
        }

        #endregion
    }
}
