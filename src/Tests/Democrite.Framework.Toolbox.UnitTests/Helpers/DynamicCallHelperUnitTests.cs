// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.UnitTests.Helpers
{
    using Democrite.Framework.Toolbox.Helpers;

    using NFluent;

    using System;

    /// <summary>
    /// Test <see cref="DynamicCallHelper"/>
    /// </summary>
    public sealed class DynamicCallHelperUnitTests
    {
        #region Nested

        public class NestedLeaf
        {
            public int Value { get; set; }

            public string? Name { get; set; }
        }

        public class Leaf
        {
            public Guid Uid { get; set; }

            public NestedLeaf? NestedLeaf { get; set; }
        }

        public class Root
        {
            public Guid Id { get; set; }

            public Leaf? Leaf { get; set; }
        }

        #endregion

        /// <summary>
        /// Dynamics the call helper must return a value root level simple.
        /// </summary>
        [Fact]
        public void DynamicCallHelper_MustReturnAValue_RootLevel_Simple()
        {
            var dynamicId = DynamicCallHelper.GetValueFrom(42, "i", containRoot: true);
            Check.That(dynamicId).IsNotNull().And.IsInstanceOf<int>().And.IsEqualTo(42);

            var uid = Guid.NewGuid();
            var dynamicGuid = DynamicCallHelper.GetValueFrom(uid, "i", containRoot: true);
            Check.That(dynamicGuid).IsNotNull().And.IsInstanceOf<Guid>().And.IsEqualTo(uid);
        }

        /// <summary>
        /// Dynamics the call helper must return a value one level simple.
        /// </summary>
        [Fact]
        public void DynamicCallHelper_MustReturnAValue_OneLevel_Simple()
        {
            var source = new Root
            {
                Id = Guid.NewGuid()
            };

            var dynamicId = DynamicCallHelper.GetValueFrom(source, "Id");
            Check.That(dynamicId).IsNotNull().And.IsInstanceOf<Guid>().And.IsEqualTo(source.Id);

            var dynamicIdMaj = DynamicCallHelper.GetValueFrom(source, "ID");
            Check.That(dynamicIdMaj).IsNotNull().And.IsInstanceOf<Guid>().And.IsEqualTo(source.Id);

            var dynamicIdMin = DynamicCallHelper.GetValueFrom(source, "id");
            Check.That(dynamicIdMin).IsNotNull().And.IsInstanceOf<Guid>().And.IsEqualTo(source.Id);
        }

        /// <summary>
        /// Dynamics the call helper must return a value one level simple.
        /// </summary>
        [Fact]
        public void DynamicCallHelper_MustReturnAValue_OneLevel_Simple_WithRootLevel()
        {
            var source = new Root
            {
                Id = Guid.NewGuid()
            };

            var dynamicId = DynamicCallHelper.GetValueFrom(source, "Root.Id", containRoot: true);
            Check.That(dynamicId).IsNotNull().And.IsInstanceOf<Guid>().And.IsEqualTo(source.Id);

            var dynamicIdMaj = DynamicCallHelper.GetValueFrom(source, "Root.ID", containRoot: true);
            Check.That(dynamicIdMaj).IsNotNull().And.IsInstanceOf<Guid>().And.IsEqualTo(source.Id);

            var dynamicIdMin = DynamicCallHelper.GetValueFrom(source, "Root.id", containRoot: true);
            Check.That(dynamicIdMin).IsNotNull().And.IsInstanceOf<Guid>().And.IsEqualTo(source.Id);
        }

        /// <summary>
        /// Dynamics the call helper must return a value one level simple.
        /// </summary>
        [Fact]
        public void DynamicCallHelper_MustReturnAValue_TwoLevel_Simple()
        {
            var source = new Root
            {
                Id = Guid.NewGuid(),
                Leaf = new Leaf
                {
                    Uid = Guid.NewGuid(),
                }
            };

            var dynamicId = DynamicCallHelper.GetValueFrom(source, "Leaf.Uid");
            Check.That(dynamicId).IsNotNull().And.IsInstanceOf<Guid>().And.IsEqualTo(source.Leaf.Uid);

            var dynamicIdMAJ = DynamicCallHelper.GetValueFrom(source, "LEAF.UID");
            Check.That(dynamicIdMAJ).IsNotNull().And.IsInstanceOf<Guid>().And.IsEqualTo(source.Leaf.Uid);

            var dynamicIdMin = DynamicCallHelper.GetValueFrom(source, "leaf.uid");
            Check.That(dynamicIdMin).IsNotNull().And.IsInstanceOf<Guid>().And.IsEqualTo(source.Leaf.Uid);
        }

        /// <summary>
        /// Test extract chain call
        /// </summary>
        [Fact]
        public void DynamicCallHelper_GetChainCall()
        {
            var source = new Root
            {
                Id = Guid.NewGuid(),
                Leaf = new Leaf
                {
                    Uid = Guid.NewGuid(),
                }
            };

            var dynamicIdChainCall = DynamicCallHelper.GetCallChain((Root s) => s.Leaf!.Uid);
            Check.That(dynamicIdChainCall).IsNotNull().And.IsNotEmpty().And.Equals("s.Leaf.Uid");

            var dynamicIdValue = DynamicCallHelper.GetValueFrom(source, dynamicIdChainCall!, containRoot: true);
            Check.That(dynamicIdValue).IsNotNull().And.IsInstanceOf<Guid>().And.IsEqualTo(source.Leaf.Uid);
        }
    }
}
