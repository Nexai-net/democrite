// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Inputs
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Node.Abstractions.Inputs;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Factory used to produce a "consumer" associate to a <see cref="InputSourceDefinition"/> to be able to produce input on demand
    /// </summary>
    /// <seealso cref="IInputSourceProviderFactory" />
    public sealed class InputSourceProviderFactory : IInputSourceProviderFactory
    {
        #region Fields

        private static readonly IReadOnlyDictionary<InputSourceTypeEnum, Func<InputSourceDefinition, IInputProvider>> s_builders;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSourceProviderFactory"/> class.
        /// </summary>
        static InputSourceProviderFactory()
        {
            s_builders = new Dictionary<InputSourceTypeEnum, Func<InputSourceDefinition, IInputProvider>>()
            {
                { InputSourceTypeEnum.StaticCollection, BuildStaticCollection }
            };
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IInputProvider GetProvider(InputSourceDefinition inputSourceDefinition)
        {
            ArgumentNullException.ThrowIfNull(inputSourceDefinition);

            if (s_builders.TryGetValue(inputSourceDefinition.InputSourceType, out var builder))
                return builder.Invoke(inputSourceDefinition);

            throw new NotSupportedException(inputSourceDefinition.InputSourceType + " is not supported yet by the factory.");
        }

        /// <inheritdoc />
        public ValueTask<bool> IsStillValidAsync(IInputProvider? provider, InputSourceDefinition inputSourceDefinition, CancellationToken token = default)
        {
            if (provider == null)
                return ValueTask.FromResult(false);

            return provider.IsStillValidAsync(inputSourceDefinition, token);
        }

        #region Tools

        /// <summary>
        /// Builds provider based on <see cref="InputSourceStaticCollectionDefinition{TInputType}"/>.
        /// </summary>
        private static IInputProvider BuildStaticCollection(InputSourceDefinition definition)
        {
            var sourceType = definition.InputType;
            return (IInputProvider)Activator.CreateInstance(typeof(InputSourceStaticCollectionProvider<>).MakeGenericType(sourceType.ToType()), new object[] { definition })!;
        }

        #endregion

        #endregion
    }
}
