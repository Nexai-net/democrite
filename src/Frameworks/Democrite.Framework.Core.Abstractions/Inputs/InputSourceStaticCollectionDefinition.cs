// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Inputs
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Toolbox.Extensions;

    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Define a input source using a predefined static collection.
    /// </summary>
    [Serializable]
    [Immutable]
    [ImmutableObject(true)]
    public sealed class InputSourceStaticCollectionDefinition<TInputType> : InputSourceDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSourceStaticCollectionDefinition{TInputType}"/> class.
        /// </summary>
        public InputSourceStaticCollectionDefinition(IEnumerable<TInputType?> collection, PullModeEnum pullMode)
            : base(InputSourceTypeEnum.StaticCollection, typeof(TInputType))
        {
            this.Collection = collection.ToReadOnlyList();
            this.PullMode = pullMode;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public IReadOnlyList<TInputType?> Collection { get; }

        /// <inheritdoc />
        public PullModeEnum PullMode { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEqualds(InputSourceDefinition other)
        {
            return other is InputSourceStaticCollectionDefinition<TInputType> otherTyped &&
                   this.PullMode == otherTyped.PullMode &&
                   this.Collection.SequenceEqual(otherTyped.Collection);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.PullMode.GetHashCode() ^
                   (this.Collection?.Aggregate(0, (acc, i) => acc ^ (i?.GetHashCode() ?? 0)) ?? 0);
        }

        #endregion
    }
}
