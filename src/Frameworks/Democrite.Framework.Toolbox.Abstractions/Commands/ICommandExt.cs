// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Commands
{
    using System.Windows.Input;

    /// <summary>
    /// Extend the class <see cref="ICommand"/>
    /// </summary>
    /// <seealso cref="ICommand" />
    public interface ICommandExt : ICommand
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        bool IsRunning { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Refreshes the can execute status.
        /// </summary>
        void RefreshCanExecuteStatus();

        #endregion
    }

    /// <summary>
    /// Extend the class <see cref="ICommand"/> with a content <typeparamref name="TState"/>
    /// </summary>
    /// <seealso cref="ICommand" />
    public interface ICommandExt<TState> : ICommandExt
    {
    }
}
