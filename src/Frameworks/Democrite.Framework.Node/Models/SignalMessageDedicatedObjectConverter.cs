// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Models
{
    using Democrite.Framework.Core.Abstractions.Signals;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Models;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Converter used to convert <see cref="SignalMessage"/> to its content
    /// </summary>
    /// <seealso cref="IObjectConverter" />
    internal sealed class SignalMessageDedicatedObjectConverter : IDedicatedObjectConverter
    {
        #region Fields

        private static readonly IReadOnlyCollection<Type> s_managedSourceTypes;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="SignalMessageDedicatedObjectConverter"/> class.
        /// </summary>
        static SignalMessageDedicatedObjectConverter()
        {
            s_managedSourceTypes = new Type[]
            {
                typeof(SignalMessage),
                typeof(SignalSource)
            };
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public IReadOnlyCollection<Type> ManagedSourceTypes
        {
            get { return s_managedSourceTypes; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool TryConvert(object obj, Type targetType, out object? result)
        {
            if (NoneType.Trait == targetType)
            {
                result = NoneType.Instance;
                return true;
            }

            SignalSource? source = null;
            result = null;

            if (obj is SignalMessage signal)
                source = signal.From;

            if (obj is SignalSource signalSource)
                source = signalSource;

            if (source is null || string.IsNullOrEmpty(source.CarryMessageType))
                return false;

            var signalSourceContent = source.GetContent();
            var type = signalSourceContent?.GetType();

            if (type is not null)
            {
                if (type.IsAssignableTo(targetType))
                {
                    result = signalSourceContent;
                    return true;
                }

                var targetConvertType = Convert.ChangeType(signalSourceContent, targetType);

                if (targetConvertType != null)
                {
                    result = targetConvertType;
                    return true;
                }
            }

            return false;
        }
     
        #endregion
    }
}
