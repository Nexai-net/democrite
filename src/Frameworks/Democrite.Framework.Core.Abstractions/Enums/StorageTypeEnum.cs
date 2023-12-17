// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Enums
{
    /// <summary>
    /// System storage type
    /// </summary>
    [Flags]
    public enum StorageTypeEnum
    {
        None = 0,

        /// <summary>
        /// Define a storage used when no specific if definied
        /// </summary>
        Default = 1,

        /// <summary>
        /// Define a storage used by democrite system vgrain.
        /// </summary>
        Democrite = 2,

        /// <summary>
        /// Define a storage used to store reminder information.
        /// </summary>
        Reminders = 4,

        All = StorageTypeEnum.Democrite | 
              StorageTypeEnum.Reminders | 
              StorageTypeEnum.Default
    }
}
