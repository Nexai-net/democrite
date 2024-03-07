// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Validator used to ensure <see cref="IVGrainId"/> follow the requested rules
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class VGrainIdTypeValidatorAttribute<TExpectedGrainType> : VGrainIdRegexValidatorAttribute
        where TExpectedGrainType : IGrain
    {
        #region Fields

        private static readonly string s_debugLabel;

        private static readonly bool s_isPrimaryInteger;
        private static readonly bool s_isPrimaryString;
        private static readonly bool s_isPrimaryGuid;

        private static readonly bool s_isCompound;

        private static readonly string s_idValidation;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="VGrainIdTypeValidatorAttribute{TExpectedGrainType}"/> class.
        /// </summary>
        static VGrainIdTypeValidatorAttribute()
        {
            var trait = typeof(TExpectedGrainType);

            var stringKey = typeof(IGrainWithStringKey);

            var guidKey = typeof(IGrainWithGuidKey);
            var guidComponedStrKey = typeof(IGrainWithGuidCompoundKey);

            var intergerKey = typeof(IGrainWithIntegerKey);
            var intergerComponedStrKey = typeof(IGrainWithIntegerCompoundKey);

            s_isPrimaryString = trait.IsAssignableTo(stringKey);
            s_isPrimaryInteger = trait.IsAssignableTo(intergerKey) || trait.IsAssignableTo(intergerComponedStrKey);
            s_isPrimaryGuid = trait.IsAssignableTo(guidKey) || trait.IsAssignableTo(guidComponedStrKey);

            s_isCompound = trait.IsAssignableTo(intergerComponedStrKey) || trait.IsAssignableTo(guidComponedStrKey);

            var str = new StringBuilder();
            var regex = new StringBuilder();
            //regex.Append("(.*)/");

            if (s_isPrimaryGuid)
            {
                str.Append("Primary [Guid]");
                regex.Append("/([a-zA-Z0-9]{32})");
            }
            else if (s_isPrimaryInteger)
            {
                str.Append("Primary [Long]");
                regex.Append("([0-9]{1, 18})");
            }
            else if (s_isPrimaryString)
            {
                str.Append("Primary [string]");
                regex.Append("(.*)");
            }

            if (s_isCompound)
            {
                str.Append(" - Compound");
                regex.Append("\\+([^+].*)");
            }

            s_debugLabel = str.ToString();
            s_idValidation = regex.ToString();
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdTypeValidatorAttribute{TExpectedGrainType}"/> class.
        /// </summary>
        public VGrainIdTypeValidatorAttribute()
            : base(s_idValidation, lookingPart: Enums.VGrainIdPartEnum.Primary | Enums.VGrainIdPartEnum.Extension)
        {
            
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return s_debugLabel;
        }

        #endregion
    }
}
