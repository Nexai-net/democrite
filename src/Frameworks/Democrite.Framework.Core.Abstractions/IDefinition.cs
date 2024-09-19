// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Elvex.Toolbox.Abstractions.Supports;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Define a serializable entity used to provide execution information to democrite engine
    /// </summary>
    public interface IDefinition : ISupportDebugDisplayName, IEntityWithId<Guid>
    {
        #region Properties

        /// <summary>
        /// Gets the display name used in log or storage to easily identify the definition
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the meta data.
        /// </summary>
        DefinitionMetaData? MetaData { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Validates this current <see cref="IDefinition"/>
        /// </summary>
        /// <param name="logger">The logger where all the error, warning, critical information must be send.</param>
        /// <param name="matchWarningAsError">if set to <c>true</c> warning are rank up to error.</param>
        /// <returns><c>true</c> if the definition information are enought to a correct execution</returns>
        bool Validate(ILogger logger, bool matchWarningAsError = false);

        #endregion
    }

    /// <summary>
    /// Define a serializable entity accces by a ref id
    /// </summary>
    public interface IRefDefinition : IDefinition
    {
        /// <summary>
        /// Gets the reference identifier.
        /// </summary>
        /// <remarks>
        ///     ref://TYPE@NAMESPACE/REF_ID
        /// </remarks>
        Uri RefId { get; }
    }
}
