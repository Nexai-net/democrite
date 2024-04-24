// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Models;
    using Elvex.Toolbox.Disposables;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Factory implementation used to generate singleton <see cref="GrainId"/>
    /// </summary>
    /// <seealso cref="IVGrainIdFactory" />
    public sealed class VGrainIdFactorySingleton : SafeDisposable, IVGrainIdDedicatedFactory
    {
        #region Fields

        private static readonly string s_singletonString;
        private static readonly Guid s_singletonGuid;
        private static readonly long s_singletonLong;
        private static readonly string s_defaultExtention;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="VGrainIdFactorySingleton"/> class.
        /// </summary>
        static VGrainIdFactorySingleton()
        {
            s_singletonGuid = new Guid("D97CE77B-3852-41E3-9FE3-79FE53B7D92D");
            s_singletonString = "Singleton";
            s_singletonLong = long.MaxValue;

            s_defaultExtention = "42";
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool CanHandled(VGrainIdBaseFormatorAttribute attr)
        {
            return attr is VGrainIdSingletonAttribute;
        }

        /// <inheritdoc />
        public IVGrainId BuildNewId(VGrainIdBaseFormatorAttribute attr,
                                    Type vgrainType,
                                    object? input,
                                    IExecutionContext? executionContext,
                                    ILogger? logger = null)
        {
            return BuildNewId(attr.FormatType, vgrainType, (attr as VGrainIdSingletonAttribute)?.SingletonValue);
        }

        /// <inheritdoc />
        public IVGrainId BuildNewId(IdFormatTypeEnum format, Type vgrainType, string? wantedSerializedValue = null)
        {
            object primary;
            string? extensionKey = null;

            switch (format)
            {
                case IdFormatTypeEnum.Guid:
                    primary = string.IsNullOrEmpty(wantedSerializedValue) ? s_singletonGuid : Guid.Parse(wantedSerializedValue);
                    break;

                case IdFormatTypeEnum.CompositionGuidString:
                    primary = string.IsNullOrEmpty(wantedSerializedValue) ? s_singletonGuid : Guid.Parse(wantedSerializedValue);
                    extensionKey = s_defaultExtention;
                    break;

                case IdFormatTypeEnum.CompositionStringString:
                    primary = string.IsNullOrEmpty(wantedSerializedValue) ? s_singletonString : wantedSerializedValue;
                    extensionKey = s_defaultExtention;
                    break;

                case IdFormatTypeEnum.Long:
                    primary = string.IsNullOrEmpty(wantedSerializedValue) ? s_singletonLong : long.Parse(wantedSerializedValue);
                    break;

                case IdFormatTypeEnum.CompositionLongString:
                    primary = string.IsNullOrEmpty(wantedSerializedValue) ? s_singletonLong : long.Parse(wantedSerializedValue);
                    extensionKey = s_defaultExtention;
                    break;

                case IdFormatTypeEnum.String:
                default:
                    primary = string.IsNullOrEmpty(wantedSerializedValue) ? s_singletonString : wantedSerializedValue;
                    break;
            }

            var vgrainId = new VGrainId(vgrainType, format, primary!, extensionKey);
            return vgrainId;
        }

        #endregion
    }
}
