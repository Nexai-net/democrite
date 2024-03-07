// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Repositories
{
    using Orleans.Storage;

    using System;
    using System.Diagnostics;
    using System.Reflection;

    public static class DemocriteSerializerExtensions
    {
        #region Fields
        
        private static readonly MethodInfo s_serializeGenericMethd;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DemocriteSerializerExtensions"/> class.
        /// </summary>
        static DemocriteSerializerExtensions()
        {
            var serializeMthd = typeof(IGrainStorageSerializer).GetMethod(nameof(IGrainStorageSerializer.Serialize));
            Debug.Assert(serializeMthd != null);

            s_serializeGenericMethd = serializeMthd;
        }

        #endregion

        public static BinaryData SerializeObject(this IGrainStorageSerializer grainStorageSerializer, object data)
        {
            ArgumentNullException.ThrowIfNull(data);
            return (BinaryData)s_serializeGenericMethd.MakeGenericMethod(data.GetType()).Invoke(grainStorageSerializer, new object[] { data })!;

        }
    }
}
