// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Models
{
    using System;
    using System.ComponentModel;

    [Serializable]
    [ImmutableObject(true)]
    [Immutable]
    [GenerateSerializer]
    public sealed class SerieSetValueModel<TTypeValue, TSerieIdType>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SerieSetValueModel{TTypeValue}"/> class.
        /// </summary>
        public SerieSetValueModel(TTypeValue? value,
                                  string unit,
                                  string source,
                                  TSerieIdType? serieId,
                                  DateTime tickUtc)
        {
            this.Value = value;
            this.Unit = unit;
            this.Source = source;
            this.SerieId = serieId;
            this.TickUtc = tickUtc;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the value.
        /// </summary>
        [Id(0)]
        public TTypeValue? Value { get; }

        /// <summary>
        /// Gets the unit.
        /// </summary>
        [Id(1)]
        public string Unit { get; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        [Id(2)]
        public string Source { get; }

        /// <summary>
        /// Gets the serie identifier.
        /// </summary>
        [Id(3)]
        public TSerieIdType? SerieId { get; }

        /// <summary>
        /// Gets the tick.
        /// </summary>
        [Id(4)]
        public DateTime TickUtc { get; }

        #endregion
    }
}
