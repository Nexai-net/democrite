// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Extensions.Types
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <inheritdoc />
    internal class ValueTaskTypeInfoEnhancer : IValueTaskTypeInfoEnhancer
    {
        #region Methods

        /// <summary>
        /// Creates the specified <see cref="IValueTaskTypeInfoEnhancer"/>.
        /// </summary>
        public static IValueTaskTypeInfoEnhancer Create(Type trait)
        {
            if (trait.IsGenericType && trait.GetGenericTypeDefinition() == typeof(ValueTask<>))
                return (IValueTaskTypeInfoEnhancer)Activator.CreateInstance(typeof(ValueTaskTypeInfoEnhancer<>).MakeGenericType(trait.GetGenericArguments().First()))!;

            return new ValueTaskTypeInfoEnhancer();
        }

        /// <inheritdoc />
        public virtual Task AsTask(object? valueTask)
        {
            if (valueTask is null)
                return Task.CompletedTask;

            if (valueTask is ValueTask val)
                return val.AsTask();

            throw new InvalidCastException("Expected ValueTask get " + valueTask);
        }

        /// <inheritdoc />
        public virtual object? GetValueTaskFromResult(object? resultInst)
        {
            if (resultInst is null)
                return ValueTask.CompletedTask;

            return ValueTask.FromResult(resultInst);
        }

        #endregion
    }

    /// <inheritdoc />
    file sealed class ValueTaskTypeInfoEnhancer<TResult> : ValueTaskTypeInfoEnhancer, IValueTaskTypeInfoEnhancer
    {
        #region Methods

        /// <inheritdoc />
        public sealed override Task AsTask(object? valueTask)
        {
            if (valueTask is ValueTask<TResult> val)
                return val.AsTask();

            // Call base class to perfom error handling because differt rom Task ValueTask{TResul} doesn't inherite from ValueTask
            return base.AsTask(valueTask);
        }

        /// <inheritdoc />
        public override object? GetValueTaskFromResult(object? resultInst)
        {
#pragma warning disable CA2012 // Use ValueTasks correctly
#pragma warning disable CS8604 // Possible null reference argument.
            return ValueTask.FromResult<TResult>((TResult?)resultInst);
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CA2012 // Use ValueTasks correctly
        }

        #endregion
    }
}
