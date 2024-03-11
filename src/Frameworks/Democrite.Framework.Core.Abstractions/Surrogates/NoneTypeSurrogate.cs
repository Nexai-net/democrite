// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Surrogates
{
    using Elvex.Toolbox;

    [GenerateSerializer]
    public record struct NoneTypeSurrogate();

    [RegisterConverter]
    public sealed class NoneTypeSurrogateConverter : IConverter<NoneType, NoneTypeSurrogate>
    {
        /// <inheritdoc />
        public NoneType ConvertFromSurrogate(in NoneTypeSurrogate surrogate)
        {
            return NoneType.Instance;
        }

        /// <inheritdoc />
        public NoneTypeSurrogate ConvertToSurrogate(in NoneType value)
        {
            return new NoneTypeSurrogate();
        }
    }
}
