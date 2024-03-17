// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstraction.UnitTests.Models
{
    using Democrite.Framework.Core.Abstractions.Models;

    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    /// <summary>
    /// Test for <see cref="VGrainMetaData"/>
    /// </summary>
    public sealed class VGrainMetaDataUTest
    {
        /// <summary>
        /// Ensures the structure <see cref="VGrainMetaData"/> is serializable.
        /// </summary>
        [Fact]
        public void Ensure_VGrainMetaData_IsSerializable()
        {
            ObjectTestHelper.IsSerializable<VGrainMetaData>();
        }
    }
}
