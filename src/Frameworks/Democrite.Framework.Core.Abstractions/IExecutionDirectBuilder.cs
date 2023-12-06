// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Setup the condition about an <typeparamref name="TVGrain"/> direct method call
    /// </summary>
    /// <typeparam name="TVGrain">The type of the vgrain.</typeparam>
    public interface IExecutionDirectBuilder<TVGrain>
        where TVGrain : IVGrain
    {
        /// <summary>
        /// Sets the configuration.
        /// </summary>
        IExecutionDirectBuilderWithConfiguration<TVGrain, TConfig> SetConfiguration<TConfig>(TConfig? config);

        /// <summary>
        /// Sets the input.
        /// </summary>
        IExecutionDirectBuilder<TVGrain, TInput> SetInput<TInput>(TInput? input);

        /// <summary>
        /// Setup the method that will be called using the vgrain with a specific return
        /// </summary>
        IExecutionLauncher<TResult> Call<TResult>(Expression<Func<TVGrain, IExecutionContext, Task<TResult>>> call);

        /// <summary>
        /// Setup the method that will be called using the vgrain without any return
        /// </summary>
        IExecutionLauncher Call(Expression<Func<TVGrain, IExecutionContext, Task>> call);
    }

    /// <summary>
    /// Setup the condition about an <typeparamref name="TVGrain"/> direct method call with an <typeparamref name="TInput"/> parameter
    /// </summary>
    /// <typeparam name="TVGrain">The type of the vgrain.</typeparam>
    /// <typeparam name="TInput">Input type pass to method call</typeparam>
    public interface IExecutionDirectBuilder<TVGrain, TInput>
        where TVGrain : IVGrain
    {
        /// <summary>
        /// Sets the configuration.
        /// </summary>
        IExecutionDirectBuilderWithConfiguration<TVGrain, TInput, TConfig> SetConfiguration<TConfig>(TConfig? config);

        /// <summary>
        /// Setup the method that will be called using the vgrain with a specific return
        /// </summary>
        IExecutionLauncher<TResult> Call<TResult>(Expression<Func<TVGrain, TInput, IExecutionContext, Task<TResult>>> call);

        /// <summary>
        /// Setup the method that will be called using the vgrain without any return
        /// </summary>
        IExecutionLauncher Call(Expression<Func<TVGrain, TInput, IExecutionContext, Task>> call);
    }

    /// <summary>
    /// Setup the condition about an <typeparamref name="TVGrain"/> direct method call with a configuration <typeparamref name="TConfig"/>
    /// </summary>
    /// <typeparam name="TVGrain">The type of the vgrain.</typeparam>
    /// <typeparam name="TConfig">Configuration used to specific the vgrain; could be used to determine the vgrain unique id</typeparam>
    public interface IExecutionDirectBuilderWithConfiguration<TVGrain, TConfig>
        where TVGrain : IVGrain
    {
        /// <summary>
        /// Sets the input.
        /// </summary>
        IExecutionDirectBuilderWithConfiguration<TVGrain, TInput, TConfig> SetInput<TInput>(TInput? input);

        /// <summary>
        /// Setup the method that will be called using the vgrain with a specific return
        /// </summary>
        IExecutionLauncher<TResult> Call<TResult>(Expression<Func<TVGrain, IExecutionContext<TConfig>, Task<TResult>>> call);

        /// <summary>
        /// Setup the method that will be called using the vgrain without any return
        /// </summary>
        IExecutionLauncher Call(Expression<Func<TVGrain, IExecutionContext<TConfig>, Task>> call);
    }

    /// <summary>
    /// Setup the condition about an <typeparamref name="TVGrain"/> direct method call with a configuration <typeparamref name="TConfig"/> and an <typeparamref name="TInput"/> parameter
    /// </summary>
    /// <typeparam name="TVGrain">The type of the vgrain.</typeparam>
    /// <typeparam name="TConfig">Configuration used to specific the vgrain; could be used to determine the vgrain unique id</typeparam>
    /// <typeparam name="TInput">Input type pass to method call</typeparam>
    public interface IExecutionDirectBuilderWithConfiguration<TVGrain, TInput, TConfig>
        where TVGrain : IVGrain
    {
        /// <summary>
        /// Setup the method that will be called using the vgrain with a specific return
        /// </summary>
        IExecutionLauncher<TResult> Call<TResult>(Expression<Func<TVGrain, TInput, IExecutionContext<TConfig>, Task<TResult>>> call);

        /// <summary>
        /// Setup the method that will be called using the vgrain without any return
        /// </summary>
        IExecutionLauncher Call(Expression<Func<TVGrain, TInput, IExecutionContext<TConfig>, Task>> call);
    }
}
