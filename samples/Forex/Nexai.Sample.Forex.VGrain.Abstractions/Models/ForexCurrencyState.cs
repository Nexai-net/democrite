// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Nexai.Sample.Forex.VGrain.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions.Models;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Currency state
    /// </summary>
    public sealed class ForexCurrencyState
    {
        #region Fields

        private readonly LinkedList<SerieSetValueModel<double?, string>> _lastValues;
        private readonly LinkedList<SerieSetValueModel<double?, string>> _historyValues;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ForexCurrencyState"/> class.
        /// </summary>
        public ForexCurrencyState(string configuration)
            : this(EnumerableHelper<SerieSetValueModel<double?, string>>.ReadOnlyArray,
                   EnumerableHelper<SerieSetValueModel<double?, string>>.ReadOnlyArray,
                   60 * 60 * 24,
                   configuration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForexCurrencyState"/> class.
        /// </summary>
        public ForexCurrencyState(IEnumerable<SerieSetValueModel<double?, string>>? lastValues,
                                  IEnumerable<SerieSetValueModel<double?, string>>? historyValues,
                                  int lastValuesSize,
                                  string configuration)
        {
            this.Configuration = configuration;
            this.LastValuesSize = Math.Max(10, lastValuesSize);

            var allInfo = historyValues?.Concat(lastValues ?? EnumerableHelper<SerieSetValueModel<double?, string>>.ReadOnlyArray)
                                       .OrderBy(s => s.TickUtc)
                                       .ToArray() ?? EnumerableHelper<SerieSetValueModel<double?, string>>.ReadOnlyArray;

            this._lastValues = new LinkedList<SerieSetValueModel<double?, string>>(allInfo.Take(this.LastValuesSize));
            this._historyValues = new LinkedList<SerieSetValueModel<double?, string>>(allInfo.Skip(this.LastValuesSize));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public string Configuration { get; private set; }

        /// <summary>
        /// Gets the history values.
        /// </summary>
        public IReadOnlyCollection<SerieSetValueModel<double?, string>> LastValues
        {
            get { return this._lastValues; }
        }

        /// <summary>
        /// Gets the history values.
        /// </summary>
        public IReadOnlyCollection<SerieSetValueModel<double?, string>> HistoryValues
        {
            get { return this._historyValues; }
        }

        /// <summary>
        /// Gets the last value.
        /// </summary>
        [JsonInclude]
        [IgnoreDataMember]
        public SerieSetValueModel<double?, string>? LastValue
        {
            get { return this._lastValues.Last?.Value; }
        }

        /// <summary>
        /// Gets the collection <see cref="LastValues"/> size.
        /// </summary>
        /// <remarks>
        ///     Min 10
        /// </remarks>
        public int LastValuesSize { get; }

        /// <summary>
        /// Gets the total count.
        /// </summary>
        public int Count
        {
            get { return (this._lastValues?.Count ?? 0) + (this._historyValues?.Count ?? 0); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds new value place based on <see cref="SerieSetValueModel{double?, string>}.TickUtc"/>
        /// </summary>
        public void PushNewValue(SerieSetValueModel<double?, string> value)
        {
            var success = PushNewValueIn(value, this._lastValues);

            while (this._lastValues.Count > this.LastValuesSize)
            {
                var last = this._lastValues.Last?.Value;
                this._lastValues.RemoveLast();

                if (last != null)
                    PushNewValueIn(last, this._historyValues);
            }

            if (success == false)
                PushNewValueIn(value, this._historyValues);
        }

        /// <summary>
        /// Setups the configuration.
        /// </summary>
        public void SetupConfiguration(string pair)
        {
            this.Configuration = pair;
        }

        /// <summary>
        /// Pushes a new value in linked collection <paramref name="linkedList"/> based on TickUtc if possible.
        /// </summary>
        private bool PushNewValueIn(SerieSetValueModel<double?, string> value, LinkedList<SerieSetValueModel<double?, string>> linkedList)
        {
            if (linkedList.Count == 0)
            {
                linkedList.AddFirst(value);
                return true;
            }

            if (value.TickUtc >= linkedList.Last!.Value.TickUtc)
            {
                linkedList.AddLast(value);
                return true;
            }

            if (value.TickUtc <= linkedList.First!.Value.TickUtc)
            {
                linkedList.AddFirst(value);
                return true;
            }

            var previousNode = linkedList.Nodes()
                                         .FirstOrDefault(n => n.Value != null && n.Value.TickUtc <= value.TickUtc &&
                                                              (n.Next == null || n.Next.Value.TickUtc >= value.TickUtc));

            if (previousNode != null)
            {
                linkedList.AddAfter(previousNode, value);
                return true;
            }

            return false;
        }

        #endregion
    }
}
