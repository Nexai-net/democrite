// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Inputs
{
    using Democrite.Framework.Core.Abstractions.Inputs;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Selector simply used to use as input all the static collection
    /// </summary>
    /// <typeparam name="TInputType">The type of the input type.</typeparam>
    /// <seealso cref="InputSourceStaticCollectionBaseSelector{TInputType}" />
    internal sealed class InputSourceStaticCollectionBroadcastSelector<TInputType> : InputSourceStaticCollectionBaseSelector<TInputType>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSourceStaticCollectionBroadcastSelector{TInputType}"/> class.
        /// </summary>
        public InputSourceStaticCollectionBroadcastSelector(InputSourceStaticCollectionDefinition<TInputType> definition)
            : base(definition)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Provide all the collection elements
        /// </summary>
        public override Task<object?> GetNextAsync(CancellationToken token = default)
        {
            return Task.FromResult<object?>(this.Definition.Collection);
        }

        #endregion
    }
}
