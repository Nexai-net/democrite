// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Enums
{
    /// <summary>
    /// System storage type
    /// </summary>
    [Flags]
    public enum StorageTypeEnum : ushort
    {
        None = 0,

        /// <summary>
        /// Define a storage used when no specific if definied
        /// </summary>
        Default = 0x0001,

        /// <summary>
        /// Define a storage used by democrite system vgrain.
        /// </summary>
        Democrite = 0x0002,

        /// <summary>
        /// Define a storage used to store reminder information.
        /// </summary>
        Reminders = 0x0004,

        /// <summary>
        /// Define a storage used to store reminder information.
        /// </summary>
        Repositories = 0x0008,

        /// <summary>
        /// The dynamic definition storage
        /// </summary>
        DynamicDefinition = 0x0010,

        /// <summary>
        /// Define a storage used to store democrite high system information.
        /// </summary>
        DemocriteAdmin = 0x8000,

        All = StorageTypeEnum.Democrite | 
              StorageTypeEnum.Reminders | 
              StorageTypeEnum.Default |
              StorageTypeEnum.Repositories |
              StorageTypeEnum.DynamicDefinition |
              StorageTypeEnum.DemocriteAdmin
    }
}
