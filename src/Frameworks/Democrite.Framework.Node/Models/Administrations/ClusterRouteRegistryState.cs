// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Models.Administrations
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Exceptions;

    using Elvex.Toolbox.Models;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// State used to store route information
    /// </summary>
    internal sealed class ClusterRouteRegistryState
    {
        #region Fields

        private readonly Dictionary<Guid, VGrainRedirectionDefinition> _redirections;
        private readonly Dictionary<ConcretType, HashSet<VGrainRedirectionDefinition>> _indexRedirection;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterRouteRegistryState"/> class.
        /// </summary>
        public ClusterRouteRegistryState(IReadOnlyCollection<VGrainRedirectionDefinition> redirectionDefinitions, string etag)
        {
            this._redirections = redirectionDefinitions?.ToDictionary(k => k.Uid) ?? new Dictionary<Guid, VGrainRedirectionDefinition>();
            this._indexRedirection = this._redirections.Select(kv => kv.Value)
                                                       .GroupBy(k => k.Source)
                                                       .ToDictionary(k => k.Key, v => v.ToHashSet());

            this.Etag = string.IsNullOrEmpty(etag) ? Guid.NewGuid().ToString() : etag;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the etag.
        /// </summary>
        public string Etag { get; private set; }

        #endregion

        #region Methods

        /// <returns></returns>
        public IReadOnlyCollection<VGrainRedirectionDefinition> GetVGrainRedirections()
        {
            return this._redirections.Values;
        }

        /// <summary>
        /// Pushes a new redirection.
        /// </summary>
        
        // TODO : Change params [] into params IReadConlyCollection
        public bool PushRedirections(params VGrainRedirectionDefinition[] redirectionDefinition)
        {
            var result = false;
            if (redirectionDefinition.Length == 1)
            {
                var def = redirectionDefinition.First();
                def.ValidateWithException();

                result = TryPushOneRedirection(def, this._redirections, this._indexRedirection);
            }
            else if (redirectionDefinition.Length > 1)
            {
                /*
                 * When multiple element try to be injected we use two pass to check if no issue will result before doing it
                 */
                var localRedirectionTransactionCopy = new Dictionary<Guid, VGrainRedirectionDefinition>(this._redirections);
                var localIndexTransactionCopy = new Dictionary<ConcretType, HashSet<VGrainRedirectionDefinition>>(this._indexRedirection);

                bool transactionResults = false;
                foreach (var red in redirectionDefinition)
                {
                    red.ValidateWithException();
                    transactionResults |= TryPushOneRedirection(red, localRedirectionTransactionCopy, localIndexTransactionCopy);
                }

                // To this point if transactionResults == true it means no conflict and at least one change
                if (transactionResults)
                {
                    result = true;

                    // Really apply transaction
                    foreach (var red in redirectionDefinition)
                        TryPushOneRedirection(red, this._redirections, this._indexRedirection);
                }
            }

            if (result)
                this.Etag = Guid.NewGuid().ToString();
            return result;
        }

        /// <summary>
        /// Pops the redirection.
        /// </summary>
        
        // TODO : Change params [] into params IReadConlyCollection
        public bool PopRedirections(params Guid[] redirectionIds)
        {
            var result = false;
            foreach (var redirectionId in redirectionIds)
            {
                if (this._redirections.TryGetValue(redirectionId, out var redirection) &&
                    this._indexRedirection.TryGetValue(redirection.Source, out var hashset))
                {
                    this._redirections.Remove(redirectionId);
                    result |= hashset.Remove(redirection);
                }
            }

            if (result)
                this.Etag = Guid.NewGuid().ToString();
            return result;
        }

        /// <summary>
        /// Converts to surrogate.
        /// </summary>
        public ClusterRouteRegistryStateSurrogate ToSurrogate()
        {
            return new ClusterRouteRegistryStateSurrogate(this._redirections.Values, this.Etag);
        }

        #region Tools

        /// <summary>
        /// Pushes a new redirection.
        /// </summary>
        private static bool TryPushOneRedirection(VGrainRedirectionDefinition redirectionDefinition,
                                                  Dictionary<Guid, VGrainRedirectionDefinition> targetRedirection,
                                                  Dictionary<ConcretType, HashSet<VGrainRedirectionDefinition>> indexedTargetRedirection)
        {
            if (targetRedirection.ContainsKey(redirectionDefinition.Uid))
                return false;

            if (indexedTargetRedirection.TryGetValue(redirectionDefinition.Source, out var redirections))
            {
                var conflict = redirections.FirstOrDefault(r => r.Conflict(redirectionDefinition));
                if (conflict is not null)
                    throw new ConflictDefinitionException(redirectionDefinition.GetType(), Newtonsoft.Json.JsonConvert.SerializeObject(redirectionDefinition), Newtonsoft.Json.JsonConvert.SerializeObject(conflict));
            }

            targetRedirection[redirectionDefinition.Uid] = redirectionDefinition;

            HashSet<VGrainRedirectionDefinition>? redirectionDefinitions = null;
            if (!indexedTargetRedirection.TryGetValue(redirectionDefinition.Source, out redirectionDefinitions))
            {
                redirectionDefinitions = new HashSet<VGrainRedirectionDefinition>();
                indexedTargetRedirection.Add(redirectionDefinition.Source, redirectionDefinitions);
            }

            return redirectionDefinitions.Add(redirectionDefinition);
        }

        #endregion

        #endregion
    }

    [GenerateSerializer]
    internal record struct ClusterRouteRegistryStateSurrogate(IReadOnlyCollection<VGrainRedirectionDefinition> RedirectionDefinitions, string Etag);

    [RegisterConverter]
    internal sealed class ClusterRouteRegistryStateConverter : IConverter<ClusterRouteRegistryState, ClusterRouteRegistryStateSurrogate>
    {
        #region Ctor

        static ClusterRouteRegistryStateConverter()
        {
            Instance = new ClusterRouteRegistryStateConverter();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static ClusterRouteRegistryStateConverter Instance { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ClusterRouteRegistryState ConvertFromSurrogate(in ClusterRouteRegistryStateSurrogate surrogate)
        {
            return new ClusterRouteRegistryState(surrogate.RedirectionDefinitions, surrogate.Etag);
        }

        /// <inheritdoc />
        public ClusterRouteRegistryStateSurrogate ConvertToSurrogate(in ClusterRouteRegistryState value)
        {
            return value.ToSurrogate();
        }

        #endregion
    }
}
