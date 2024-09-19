// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.References
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.References;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Streams;

    using Orleans.Runtime;
    using Orleans.Runtime.Services;
    using Orleans.Services;

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Service in charge to solve the <see cref="Uri"/> RefId
    /// </summary>
    internal sealed class DemocriteReferenceSolverService : GrainServiceClient<IDemocriteTypeReferenceGrainService>, IDemocriteReferenceSolverService, IGrainServiceClient<IDemocriteTypeReferenceGrainService>
    {
        #region Fields

        private readonly IStreamQueueDefinitionProvider _streamQueueDefinitionProvider;
        private readonly ISequenceDefinitionProvider _sequenceDefinitionProvider;
        private readonly ISignalDefinitionProvider _signalDefinitionProvider;

        private readonly IDemocriteTypeReferenceGrainService _currentGrainService;

        private readonly Silo _silo;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteReferenceSolverService"/> class.
        /// </summary>
        public DemocriteReferenceSolverService(IServiceProvider serviceProvider,
                                               Silo silo,
                                               IStreamQueueDefinitionProvider streamQueueDefinitionProvider,
                                               ISequenceDefinitionProvider sequenceDefinitionProvider,
                                               ISignalDefinitionProvider signalDefinitionProvider)
            : base(serviceProvider)
        {
            this._silo = silo;
            this._streamQueueDefinitionProvider = streamQueueDefinitionProvider;
            this._sequenceDefinitionProvider = sequenceDefinitionProvider;
            this._signalDefinitionProvider = signalDefinitionProvider;

            this._currentGrainService = base.GetGrainService(this._silo.SiloAddress);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<MethodInfo?> GetReferenceMethodAsync(Uri methodRefId, Type sourceType)
        {
            var target = await GetReferenceTypeImpl(methodRefId);

            if (target is ReferenceTypeMethodTarget mth)
            {
                var method = mth.Method.ToMethod(sourceType);
                if (method is null)
                    throw new InvalidDataException("Could not found method {0} on type {1}".WithArguments(mth.Method, sourceType));

                return method as MethodInfo;
            }

            if (target is not null)
                throw new InvalidDataException("{0} reference doesn't refer to a method kind".WithArguments(target.RefId));

            return null;
        }

        /// <inheritdoc />
        public async ValueTask<Tuple<Type, Uri>?> GetReferenceType(Uri typeRefId)
        {
            var target = await GetReferenceTypeImpl(typeRefId);

            if (target is not null)
            {
                if (target is ReferenceTypeTarget refTypeTarget)
                {
                    var type = refTypeTarget.Type.ToType();
                    if (type is null)
                        throw new InvalidDataException("Type {0} Referenced by ref {1} could not be loaded on this silo".WithArguments(refTypeTarget.Type, refTypeTarget.RefId));

                    return Tuple.Create(type, refTypeTarget.RefId);
                }

                throw new InvalidDataException("{0} reference doesn't refer to a type kind".WithArguments(target.RefId));
            }

            return null;
        }

        /// <inheritdoc />
        public async ValueTask<IDefinition?> TryGetReferenceDefinition(Uri definitionRefId)
        {
            var defs = await GetReferenceDefinitions(definitionRefId);

            if (!defs.Any())
                return null;

            if (defs.Count > 1)
                return null;

            return defs.Single();
        }

        /// <inheritdoc />
        public ValueTask<IReadOnlyCollection<IDefinition>> GetReferenceDefinitions(Uri definitionRefId)
        {
            throw new NotImplementedException();
        }

        #region Tools

        /// <summary>
        /// Gets the reference type implementation.
        /// </summary>
        private async Task<ReferenceTarget?> GetReferenceTypeImpl(Uri methodRefId)
        {
            var target = await this._currentGrainService.GetReferenceTarget(methodRefId);
            return target;
        }

        #endregion

        #endregion

    }
}
