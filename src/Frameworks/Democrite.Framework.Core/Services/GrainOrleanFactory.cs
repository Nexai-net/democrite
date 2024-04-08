// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Services
{
    using Democrite.Framework.Core.Abstractions;

    using Orleans.Runtime;

    using System;

    internal sealed class GrainOrleanFactory : IGrainOrleanFactory
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GrainOrleanFactory"/> class.
        /// </summary>
        public GrainOrleanFactory(IGrainFactory grainFactory)
        {
            this.GrainFactory = grainFactory;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the grain factory.
        /// </summary>
        public IGrainFactory GrainFactory { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public TGrainObserverInterface CreateObjectReference<TGrainObserverInterface>(IGrainObserver obj)
            where TGrainObserverInterface : IGrainObserver
        {
            return this.GrainFactory.CreateObjectReference<TGrainObserverInterface>(obj);
        }

        /// <inheritdoc />
        public void DeleteObjectReference<TGrainObserverInterface>(IGrainObserver obj)
            where TGrainObserverInterface : IGrainObserver
        {
            this.GrainFactory.DeleteObjectReference<TGrainObserverInterface>(obj);
        }

        /// <inheritdoc />
        public TGrainInterface GetGrain<TGrainInterface>(Guid primaryKey, string? grainClassNamePrefix = null)
            where TGrainInterface : IGrainWithGuidKey
        {
            return this.GrainFactory.GetGrain<TGrainInterface>(primaryKey, grainClassNamePrefix);
        }

        /// <inheritdoc />
        public TGrainInterface GetGrain<TGrainInterface>(long primaryKey, string? grainClassNamePrefix = null)
            where TGrainInterface : IGrainWithIntegerKey
        {
            return this.GrainFactory.GetGrain<TGrainInterface>(primaryKey, grainClassNamePrefix);
        }

        /// <inheritdoc />
        public TGrainInterface GetGrain<TGrainInterface>(string primaryKey, string? grainClassNamePrefix = null)
            where TGrainInterface : IGrainWithStringKey
        {
            return this.GrainFactory.GetGrain<TGrainInterface>(primaryKey, grainClassNamePrefix);
        }

        /// <inheritdoc />
        public TGrainInterface GetGrain<TGrainInterface>(Guid primaryKey, string keyExtension, string? grainClassNamePrefix = null)
            where TGrainInterface : IGrainWithGuidCompoundKey
        {
            return this.GrainFactory.GetGrain<TGrainInterface>(primaryKey, keyExtension, grainClassNamePrefix);
        }

        /// <inheritdoc />
        public TGrainInterface GetGrain<TGrainInterface>(long primaryKey, string keyExtension, string? grainClassNamePrefix = null)
            where TGrainInterface : IGrainWithIntegerCompoundKey
        {
            return this.GrainFactory.GetGrain<TGrainInterface>(primaryKey, keyExtension, grainClassNamePrefix);
        }

        /// <inheritdoc />
        public IGrain GetGrain(Type grainInterfaceType, Guid grainPrimaryKey)
        {
            return this.GrainFactory.GetGrain(grainInterfaceType, grainPrimaryKey);
        }

        /// <inheritdoc />
        public IGrain GetGrain(Type grainInterfaceType, long grainPrimaryKey)
        {
            return this.GrainFactory.GetGrain(grainInterfaceType, grainPrimaryKey);
        }

        /// <inheritdoc />
        public IGrain GetGrain(Type grainInterfaceType, string grainPrimaryKey)
        {
            return this.GrainFactory.GetGrain(grainInterfaceType, grainPrimaryKey);
        }

        /// <inheritdoc />
        public IGrain GetGrain(Type grainInterfaceType, Guid grainPrimaryKey, string keyExtension)
        {
            return this.GrainFactory.GetGrain(grainInterfaceType, grainPrimaryKey, keyExtension);
        }

        /// <inheritdoc />
        public IGrain GetGrain(Type grainInterfaceType, long grainPrimaryKey, string keyExtension)
        {
            return this.GrainFactory.GetGrain(grainInterfaceType, grainPrimaryKey, keyExtension);
        }

        /// <inheritdoc />
        public TGrainInterface GetGrain<TGrainInterface>(GrainId grainId)
            where TGrainInterface : IAddressable
        {
            return this.GrainFactory.GetGrain<TGrainInterface>(grainId);
        }

        /// <inheritdoc />
        public IAddressable GetGrain(GrainId grainId)
        {
            return this.GrainFactory.GetGrain(grainId);
        }

        /// <inheritdoc />
        public IAddressable GetGrain(GrainId grainId, GrainInterfaceType interfaceType)
        {
            return this.GrainFactory.GetGrain(grainId, interfaceType);
        }

        #endregion
    }
}
