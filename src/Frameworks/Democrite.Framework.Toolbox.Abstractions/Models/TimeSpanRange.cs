// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Models
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Range between time information carry by TimeSpan
    /// </summary>
    public readonly struct TimeSpanRange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpanRange"/> struct.
        /// </summary>
        public TimeSpanRange(TimeSpan min, TimeSpan max)
        {
            if (max <= min)
                throw new InvalidDataException("Max MUST be strictely above Min");

            this.Min = min;
            this.Max = max;
            this.Interval = max - min;
        }

        #region Properties

        /// <summary>
        /// Determines the minimum of the parameters.
        /// </summary>
        public TimeSpan Min { get; }

        /// <summary>
        /// Determines the maximum exclude of the parameters.
        /// </summary>
        public TimeSpan Max { get; }

        /// <summary>
        /// Gets the interval.
        /// </summary>
        public TimeSpan Interval { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the random <see cref="TimeSpan"/> in range
        /// </summary>
        public TimeSpan GetRandomValue(Random? random = null)
        {
            var rnd = random ?? Random.Shared;
            var sec = rnd.NextDouble() * this.Interval.TotalSeconds;

            return this.Min.Add(TimeSpan.FromSeconds(sec));
        }

        /// <summary>
        /// Creates the specified <see cref="TimeSpanRange"/> between <paramref name="minMilliSecond"/> and <paramref name="maxMilliSeconds"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpanRange Create(double minMilliSecond, double maxMilliSeconds)
        {
            return new TimeSpanRange(TimeSpan.FromMilliseconds(minMilliSecond), TimeSpan.FromMilliseconds(maxMilliSeconds));
        }

        #endregion
    }
}
