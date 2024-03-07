// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Attributes
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Validator used to ensure <see cref="IVGrainId"/> follow the requested rules
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class VGrainIdRegexValidatorAttribute : VGrainIdBaseValidatorAttribute
    {
        #region Fields

        private readonly string _regexLabel;
        private readonly Regex _regex;

        private readonly VGrainIdPartEnum _lookingPart;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdRegexValidatorAttribute"/> class.
        /// </summary>
        public VGrainIdRegexValidatorAttribute(string regex, string? debugLabel = null, VGrainIdPartEnum lookingPart = VGrainIdPartEnum.All)
        {
            ArgumentNullException.ThrowIfNull(regex);

            this._regexLabel = string.IsNullOrEmpty(debugLabel) ? regex : debugLabel;
            this._regex = new Regex(regex, RegexOptions.Compiled);
            this._lookingPart = lookingPart;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validates the specified grain identifier.
        /// </summary>
        public override bool Validate(GrainId grainId, in ReadOnlySpan<char> grainIdStr, ILogger logger)
        {
            var local = grainIdStr;
            if (this._lookingPart != VGrainIdPartEnum.All)
            {
                if ((this._lookingPart & VGrainIdPartEnum.GrainType) == 0)
                {
                    var splitIndex = local.IndexOf('/');
                    if (splitIndex > -1)
                        local = local.Slice(splitIndex);
                }

                if ((this._lookingPart & VGrainIdPartEnum.Primary) == 0)
                {
                    var splitStartIndex = local.IndexOf('/');
                    var splitExtIndex = local.LastIndexOf('+');

                    var removePrimary = ReadOnlySpan<char>.Empty;

                    if (splitStartIndex > 0)
                        removePrimary = local.Slice(0, splitStartIndex);

                    if (splitExtIndex > -1)
                    {
                        if (removePrimary.Length > 0)
                            removePrimary = string.Concat(removePrimary, local.Slice(splitExtIndex));
                        else
                            removePrimary = local.Slice(splitExtIndex);
                    }

                    local = removePrimary;
                }

                if ((this._lookingPart & VGrainIdPartEnum.Extension) == 0)
                {
                    var splitExtIndex = local.LastIndexOf('+');
                    if (splitExtIndex > -1)
                        local = local.Slice(0, splitExtIndex);
                }
            }

            return this._regex.IsMatch(local);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return this._regexLabel;
        }

        #endregion
    }
}
