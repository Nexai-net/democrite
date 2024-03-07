﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Orleans
{
    using System;

    public static class GrainCancellationTokenExtensions
    {
        /// <summary>
        /// Converts to <see cref="GrainCancellationTokenSource"/>.
        /// </summary>
        public static GrainCancellationTokenSource ToGrainCancellationToken(this CancellationTokenSource token)
        {
            return token.Token.ToGrainCancellationToken();
        }

        /// <summary>
        /// Converts to <see cref="GrainCancellationTokenSource"/>.
        /// </summary>
        public static GrainCancellationTokenSource ToGrainCancellationToken(this CancellationToken token)
        {
            var grainCancellationTokenSource = new GrainCancellationTokenSource();
            token.Register(() =>
            {
                try
                {
                    if (!grainCancellationTokenSource.IsCancellationRequested)
                        grainCancellationTokenSource.Cancel();
                }
                catch (ObjectDisposedException)
                {

                }
            });

            return grainCancellationTokenSource;
        }
    }
}
