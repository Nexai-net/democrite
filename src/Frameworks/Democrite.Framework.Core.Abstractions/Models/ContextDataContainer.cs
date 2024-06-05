// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Models
{
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Surrogates;
    using Elvex.Toolbox.Models;

    public interface IContextDataContainer
    {
        /// <summary>
        /// Determines whether the specified type is match.
        /// </summary>
        bool IsMatch(Type type);

        /// <summary>
        /// Determines whether the specified type is match.
        /// </summary>
        bool IsMatch(ConcretBaseType type);

        /// <summary>
        /// Determines whether the specified context data container is match.
        /// </summary>
        bool IsMatch(IContextDataContainer contextDataContainer);

        /// <summary>
        /// Gets the data.
        /// </summary>
        object? GetData(IDemocriteSerializer democriteSerializer);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <seealso cref="Models.ContextDataContainer" />
    public sealed class ContextDataContainer<TData> : IContextDataContainer
        where TData : struct
    {
        #region Fields

        private readonly ConcretBaseType _type;
        private readonly byte[] _serializedData;
        private bool _dataLoaded;
        private TData? _data;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextDataContainer{TData}"/> class.
        /// </summary>
        public ContextDataContainer(ConcretBaseType type, byte[] data)
        {
            this._type = type;
            this._serializedData = data;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public object? GetData(IDemocriteSerializer democriteSerializer)
        {
            if (!this._dataLoaded)
            {
                this._data = democriteSerializer.Deserialize<TData>(this._serializedData);
                this._dataLoaded = true;
            }

            return this._data;
        }

        /// <inheritdoc />
        public bool IsMatch(Type type)
        {
            return this._type.IsEqualTo(type, true);
        }

        /// <inheritdoc />
        public bool IsMatch(ConcretBaseType type)
        {
            return this._type.Equals(type);
        }

        /// <inheritdoc />
        public bool IsMatch(IContextDataContainer type)
        {
            return type is ContextDataContainer<TData>;
        }

        /// <summary>
        /// Creates the specified data.
        /// </summary>
        public static ContextDataContainer<TData> Create(in TData data, IDemocriteSerializer democriteSerializer)
        {
            return new ContextDataContainer<TData>((ConcretType)data.GetType().GetAbstractType(), democriteSerializer.SerializeToBinary(data).ToArray());
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this._type.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is ContextDataContainer<TData> container)
            {
                return container._type.Equals((AbstractType?)this._type) &&
                       container._serializedData.SequenceEqual(this._serializedData);
            }

            return false;
        }

        /// <summary>
        /// Converts to surrogate.
        /// </summary>
        public ContextDataContainerSurrogate<TData> ToSurrogate()
        {
            return new ContextDataContainerSurrogate<TData>()
            {
                SerializedData = this._serializedData,
                Type = ConcretBaseTypeConverter.ConvertToSurrogate(this._type),
            };
        }

        #endregion
    }

    [GenerateSerializer]
    public record struct ContextDataContainerSurrogate<TData>(IConcretTypeSurrogate Type, byte[] SerializedData);

    [RegisterConverter]
    public sealed class ContextDataContainerConverter<TData> : IConverter<ContextDataContainer<TData>, ContextDataContainerSurrogate<TData>>
        where TData : struct
    {
        /// <inheritdoc />
        public ContextDataContainer<TData> ConvertFromSurrogate(in ContextDataContainerSurrogate<TData> surrogate)
        {
            return new ContextDataContainer<TData>(ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate.Type), surrogate.SerializedData);
        }

        /// <inheritdoc />
        public ContextDataContainerSurrogate<TData> ConvertToSurrogate(in ContextDataContainer<TData> value)
        {
            return value.ToSurrogate();
        }
    }
}
