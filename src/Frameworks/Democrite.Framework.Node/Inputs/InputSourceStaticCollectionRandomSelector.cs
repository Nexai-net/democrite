﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Inputs
{
    using Democrite.Framework.Core.Abstractions.Inputs;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TInputType">The type of the input type.</typeparam>
    /// <seealso cref="InputSourceStaticCollectionBaseSelector{TInputType}" />
    internal sealed class InputSourceStaticCollectionRandomSelector<TInputType> : InputSourceStaticCollectionBaseSelector<TInputType>
    {
        #region Fields

        private readonly Random _random;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSourceStaticCollectionRandomSelector{TInputType}"/> class.
        /// </summary>
        public InputSourceStaticCollectionRandomSelector(InputSourceStaticCollectionDefinition<TInputType> definition)
            : base(definition)
        {
            this._random = new Random();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override Task<object?> GetNextAsync(CancellationToken token = default)
        {
            object? result = null;
            var collection = this.Definition.Collection;

            if (collection.Count > 0)
                result = this.Definition.Collection[this._random.Next(0, collection.Count)];

            return Task.FromResult(result);
        }

        #endregion
    }
}
