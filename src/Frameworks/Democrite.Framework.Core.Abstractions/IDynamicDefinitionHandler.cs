namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Models;

    using Elvex.Toolbox.Abstractions.Conditions;
    using Elvex.Toolbox.Models;

    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// VGrain used to inject in the cluster executable definitions with a lifescope the cluster scope
    /// </summary>
    [VGrainIdSingleton]
    internal interface IDynamicDefinitionHandlerVGrain : IVGrain
    {
        #region Methods

        /// <summary>
        /// Push a definition in the dynamic system
        /// </summary>
        /// <returns>
        ///   <c>true<c> if the definition have been injected by creation or update; Otherwise <c>false</c>
        /// </returns>
        /// <remarks>
        ///   If the push is reject for security reason the result will be false. To identify the real reason look at the logs.
        /// </remarks>
        Task<bool> PushDefinitionAsync<TDefinition>(TDefinition definition, bool @override, IIdentityCard identity, GrainCancellationToken token, bool preventNotification) where TDefinition : class, IDefinition;

        /// <summary>
        /// Push a definition in the dynamic system if doesn't exists
        /// </summary>
        /// <returns>
        ///   Return definition uid
        /// </returns>
        /// <remarks>
        ///   If the push is reject for security reason the result will be false. To identify the real reason look at the logs.
        /// </remarks>
        Task<Guid> PushDefinitionAsync<TDefinition>(ConditionExpressionDefinition existFilter, TDefinition definition, IIdentityCard identity, GrainCancellationToken token, bool preventNotification) where TDefinition : class, IDefinition;

        /// <summary>
        /// Gets the etag version of the definition registry
        /// </summary>
        /// <returns>
        ///   Return dynamic registry etag
        /// </returns>
        Task<string> GetHandlerEtagAsync();

        /// <summary>
        /// Change the enable status of a definition
        /// </summary>
        /// <returns>
        ///   <c>true<c> if the definition status have been changed; Otherwise <c>false</c>
        /// </returns>
        /// <remarks>
        ///   If the change is reject for security reason the result will be false. To identify the real reason look at the logs.
        /// </remarks>
        Task<bool> ChangeStatus(Guid definitionUid, bool isEnabled, IIdentityCard identity, GrainCancellationToken token);

        /// <summary>
        ///     Remove from the dynamic environement the definition identitfied by <param cref="definitionId" />
        /// </summary>
        /// <returns>
        ///   <c>true<c> if the definition have been removed correctly; Otherwise <c>false</c>
        /// </returns>
        /// <remarks>
        ///   If the definition id provide is not a dynamic refinition the result will be false.
        ///   If the push is reject for security reason the result will be false. To identify the real reason look at the logs.
        /// </remarks>
        Task<bool> RemoveDefinitionAsync(IIdentityCard identity, GrainCancellationToken token, IReadOnlyCollection<Guid> definitionIds);

        /// <summary>
        /// Get dynamic definition metadata in the system; Could be filter by related type <param cref="typeFilter" /> and/or by <param cref="displayNameRegex" />
        /// </summary>
        Task<EtagContainer<IReadOnlyCollection<DynamicDefinitionMetaData>>> GetDynamicDefinitionMetaDatasAsync(ConcretType? typeFilter, string? displayNameRegex, bool onlyEnabled, GrainCancellationToken token);

        /// <summary>
        /// Get dynamic definition based on is unique identifier
        /// </summary>
        /// <returns>
        ///   <c>null<c> if the definition couldn't be founded
        /// </returns>
        Task<EtagContainer<IReadOnlyCollection<TDefinition>>> GetDefinitionAsync<TDefinition>(GrainCancellationToken token, IReadOnlyCollection<Guid> uid) where TDefinition : class, IDefinition;

        /// <summary>
        /// Get dynamic definition based filter
        /// </summary>
        /// <returns>
        ///   <c>null<c> if the definition couldn't be founded
        /// </returns>
        Task<EtagContainer<IReadOnlyCollection<TDefinition>>> GetDefinitionAsync<TDefinition>(ConditionExpressionDefinition filter, GrainCancellationToken token) where TDefinition : class, IDefinition;

        #endregion
    }

    /// <summary>
    /// Proxy reference to communicate and control a <see cref="IDynamicDefinitionHandlerVGrain"/>
    /// </summary>
    public interface IDynamicDefinitionHandler
    {
        #region Methods

        /// <summary>
        /// Push a definition in the dynamic system
        /// </summary>
        /// <returns>
        ///   <c>true<c> if the definition have been injected by creation or update; Otherwise <c>false</c>
        /// </returns>
        /// <remarks>
        ///   If the push is reject for security reason the result will be false. To identify the real reason look at the logs.
        /// </remarks>
        Task<bool> PushDefinitionAsync<TDefinition>(TDefinition definition, bool @override, IIdentityCard identity, CancellationToken token, bool preventNotification = false) where TDefinition : class, IDefinition;

        /// <summary>
        /// Push a definition in the dynamic system if doesn't exists
        /// </summary>
        /// <returns>
        ///   Return definition uid
        /// </returns>
        /// <remarks>
        ///   If the push is reject for security reason the result will be false. To identify the real reason look at the logs.
        /// </remarks>
        Task<Guid> PushDefinitionAsync<TDefinition>(ConditionExpressionDefinition existFilter, TDefinition definition, IIdentityCard identity, CancellationToken token, bool preventNotification = false) where TDefinition : class, IDefinition;

        /// <summary>
        /// Gets the etag version of the definition registry
        /// </summary>
        /// <returns>
        ///   Return dynamic registry etag
        /// </returns>
        Task<string> GetHandlerEtagAsync();

        /// <summary>
        /// Change the enable status of a definition
        /// </summary>
        /// <returns>
        ///   <c>true<c> if the definition status have been changed; Otherwise <c>false</c>
        /// </returns>
        /// <remarks>
        ///   If the change is reject for security reason the result will be false. To identify the real reason look at the logs.
        /// </remarks>
        Task<bool> ChangeStatus(Guid definitionUid, bool isEnabled, IIdentityCard identity, CancellationToken token);

        /// <summary>
        ///     Remove from the dynamic environement the definition identitfied by <param cref="definitionId" />
        /// </summary>
        /// <returns>
        ///   <c>true<c> if the definition have been removed correctly; Otherwise <c>false</c>
        /// </returns>
        /// <remarks>
        ///   If the definition id provide is not a dynamic refinition the result will be false.
        ///   If the push is reject for security reason the result will be false. To identify the real reason look at the logs.
        /// </remarks>
        Task<bool> RemoveDefinitionAsync(IIdentityCard identity, CancellationToken token, params Guid[] definitionIds);

        /// <summary>
        /// Get dynamic definition metadata in the system; Could be filter by related type <param cref="typeFilter" /> and/or by <param cref="displayNameRegex" />
        /// </summary>
        Task<EtagContainer<IReadOnlyCollection<DynamicDefinitionMetaData>>> GetDynamicDefinitionMetaDatasAsync(ConcretType? typeFilter = null, string? displayNameRegex = null, bool onlyEnabled = false, CancellationToken token = default);

        /// <summary>
        /// Get dynamic definition based on is unique identifier
        /// </summary>
        /// <returns>
        ///   <c>null<c> if the definition couldn't be founded
        /// </returns>
        Task<EtagContainer<IReadOnlyCollection<TDefinition>>> GetDefinitionAsync<TDefinition>(CancellationToken token, params Guid[] uid) where TDefinition : class, IDefinition;

        /// <summary>
        /// Get dynamic definition based filter
        /// </summary>
        /// <returns>
        ///   <c>null<c> if the definition couldn't be founded
        /// </returns>
        Task<EtagContainer<IReadOnlyCollection<TDefinition>>> GetDefinitionAsync<TDefinition>(Expression<Func<TDefinition, bool>> filter, CancellationToken token) where TDefinition : class, IDefinition;

        /// <inheritdoc cref="GetDefinitionAsync{TDefinition}(CancellationToken, Guid[])" />
        Task<TDefinition?> GetDefinitionAsync<TDefinition>(Guid uid, CancellationToken token) where TDefinition : class, IDefinition;

        /// <inheritdoc cref="RemoveDefinitionAsync(IIdentityCard, CancellationToken, Guid[])" />
        Task<bool> RemoveDefinitionAsync(Guid definitionId, IIdentityCard identity, CancellationToken token);

        /// <inheritdoc cref="PushDefinitionAsync{TDefinition}(ConditionExpressionDefinition, TDefinition, IIdentityCard, CancellationToken)" />
        Task<Guid> PushDefinitionAsync<TDefinition>(Expression<Func<TDefinition, bool>> filter, TDefinition definition, IIdentityCard identity, CancellationToken token, bool preventNotification = false) where TDefinition : class, IDefinition;

        #endregion

    }
}
