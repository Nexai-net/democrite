// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Surrogates
{
    using Elvex.Toolbox;

    [GenerateSerializer]
    public record struct AnyTypeSurrogate();

    [RegisterConverter]
    public sealed class AnyTypeSurrogateConverter : IConverter<AnyType, AnyTypeSurrogate>
    {
        /// <inheritdoc />
        public AnyType ConvertFromSurrogate(in AnyTypeSurrogate surrogate)
        {
            return AnyType.Instance;
        }

        /// <inheritdoc />
        public AnyTypeSurrogate ConvertToSurrogate(in AnyType value)
        {
            return new AnyTypeSurrogate();
        }
    }

    [GenerateSerializer]
    public record struct AnyTypeContainerSurrogate<TType>(TType? Data);

    [RegisterConverter]
    public sealed class AnyTypeContainerSurrogateConverter<TType> : IConverter<AnyTypeContainer<TType>, AnyTypeContainerSurrogate<TType>>
    {
        /// <inheritdoc />
        public AnyTypeContainer<TType> ConvertFromSurrogate(in AnyTypeContainerSurrogate<TType> surrogate)
        {
            return new AnyTypeContainer<TType>(surrogate.Data);
        }

        /// <inheritdoc />
        public AnyTypeContainerSurrogate<TType> ConvertToSurrogate(in AnyTypeContainer<TType> value)
        {
            return new AnyTypeContainerSurrogate<TType>(value.Data);
        }
    }

}
