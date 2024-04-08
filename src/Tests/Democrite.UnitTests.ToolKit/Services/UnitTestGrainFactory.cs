// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Services
{
    using Orleans.Runtime;

    using System;

    public sealed class UnitTestGrainFactory : IGrainFactory
    {

        #region Methods

        public TGrainObserverInterface CreateObjectReference<TGrainObserverInterface>(IGrainObserver obj) where TGrainObserverInterface : IGrainObserver
        {
            throw new NotImplementedException();
        }

        public void DeleteObjectReference<TGrainObserverInterface>(IGrainObserver obj) where TGrainObserverInterface : IGrainObserver
        {
            throw new NotImplementedException();
        }

        public TGrainInterface GetGrain<TGrainInterface>(Guid primaryKey, string? grainClassNamePrefix = null) where TGrainInterface : IGrainWithGuidKey
        {
            throw new NotImplementedException();
        }

        public TGrainInterface GetGrain<TGrainInterface>(long primaryKey, string? grainClassNamePrefix = null) where TGrainInterface : IGrainWithIntegerKey
        {
            throw new NotImplementedException();
        }

        public TGrainInterface GetGrain<TGrainInterface>(string primaryKey, string? grainClassNamePrefix = null) where TGrainInterface : IGrainWithStringKey
        {
            throw new NotImplementedException();
        }

        public TGrainInterface GetGrain<TGrainInterface>(Guid primaryKey, string keyExtension, string? grainClassNamePrefix = null) where TGrainInterface : IGrainWithGuidCompoundKey
        {
            throw new NotImplementedException();
        }

        public TGrainInterface GetGrain<TGrainInterface>(long primaryKey, string keyExtension, string? grainClassNamePrefix = null) where TGrainInterface : IGrainWithIntegerCompoundKey
        {
            throw new NotImplementedException();
        }

        public IGrain GetGrain(Type grainInterfaceType, Guid grainPrimaryKey)
        {
            throw new NotImplementedException();
        }

        public IGrain GetGrain(Type grainInterfaceType, long grainPrimaryKey)
        {
            throw new NotImplementedException();
        }

        public IGrain GetGrain(Type grainInterfaceType, string grainPrimaryKey)
        {
            throw new NotImplementedException();
        }

        public IGrain GetGrain(Type grainInterfaceType, Guid grainPrimaryKey, string keyExtension)
        {
            throw new NotImplementedException();
        }

        public IGrain GetGrain(Type grainInterfaceType, long grainPrimaryKey, string keyExtension)
        {
            throw new NotImplementedException();
        }

        public TGrainInterface GetGrain<TGrainInterface>(GrainId grainId) where TGrainInterface : IAddressable
        {
            throw new NotImplementedException();
        }

        public IAddressable GetGrain(GrainId grainId)
        {
            throw new NotImplementedException();
        }

        public IAddressable GetGrain(GrainId grainId, GrainInterfaceType interfaceType)
        {
            throw new NotImplementedException();
        }

        #region Tools

        /// <summary>
        /// Important private method intercept by the <see cref="IGrainFactoryScope"/>
        /// </summary>
        private IAddressable GetGrain(Type interfaceType, IdSpan grainKey, string grainClassNamePrefix)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}
