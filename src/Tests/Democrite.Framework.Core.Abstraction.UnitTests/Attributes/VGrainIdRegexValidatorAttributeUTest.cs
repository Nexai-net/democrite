// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstraction.UnitTests.Attributes
{
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Enums;

    using Microsoft.Extensions.Logging.Abstractions;

    using NFluent;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Test for <see cref="VGrainIdRegexValidatorAttribute"/>
    /// </summary>
    public sealed class VGrainIdRegexValidatorAttributeUTest
    {
        #region Fields

        private static readonly string s_grainType = "testgrainIntercace";

        private static readonly string s_grainWithGuid = s_grainType + "/" + Guid.NewGuid().ToString().Replace("-", "").ToLower();
        private static readonly string s_grainWithGuidCompound = s_grainWithGuid + "+" + Random.Shared.Next(-1000, 1000) + Guid.NewGuid().ToString().Replace("-", "").ToLower().Substring(0, 10);

        private static readonly string s_primaryRegex = "/[a-zA-Z0-9]{32}";
        private static readonly string s_extRegex = "\\+[-]?[a-zA-Z0-9]{11,15}";

        //private static readonly string s_grainWithInt = s_grainType + "/" + Random.Shared.Next(-1000, 1000);
        //private static readonly string s_grainWithIntCompound = s_grainWithInt + "+" + Guid.NewGuid().ToString().Replace("-", "").ToLower();

        //private static readonly string s_grainWithString = s_grainType + "/" + Guid.NewGuid().ToString().Replace("-", "").ToLower() + Guid.NewGuid().ToString().Replace("-", "").ToLower() + "42Toto";

        #endregion

        #region Methods

        [Fact]
        public void Regex_Simple_AllPart()
        {
            var attr = new VGrainIdRegexValidatorAttribute($"^{s_grainType}{s_primaryRegex}{s_extRegex}$");

            var valid = attr.Validate(GrainId.Parse(s_grainWithGuidCompound), s_grainWithGuidCompound, NullLogger.Instance);
            Check.That(valid).IsTrue();
        }

        [Fact]
        public void Regex_Simple_GrainTypePart()
        {
            var attr = new VGrainIdRegexValidatorAttribute("^" + s_grainType + "$", lookingPart: VGrainIdPartEnum.GrainType);

            var valid = attr.Validate(GrainId.Parse(s_grainWithGuidCompound), s_grainWithGuidCompound, NullLogger.Instance);
            Check.That(valid).IsTrue();
        }

        [Fact]
        public void Regex_Simple_PrimaryPart_With_GrainTypePart()
        {
            var attr = new VGrainIdRegexValidatorAttribute($"^{s_grainType}{s_primaryRegex}$", lookingPart: VGrainIdPartEnum.Primary | VGrainIdPartEnum.GrainType);

            var valid = attr.Validate(GrainId.Parse(s_grainWithGuidCompound), s_grainWithGuidCompound, NullLogger.Instance);
            Check.That(valid).IsTrue();
        }

        [Fact]
        public void Regex_Simple_PrimaryPart()
        {
            var attr = new VGrainIdRegexValidatorAttribute($"^{s_primaryRegex}$", lookingPart: VGrainIdPartEnum.Primary);

            var valid = attr.Validate(GrainId.Parse(s_grainWithGuidCompound), s_grainWithGuidCompound, NullLogger.Instance);
            Check.That(valid).IsTrue();
        }

        [Fact]
        public void Regex_Simple_ExtensionPart_With_PrimaryPart()
        {
            var attr = new VGrainIdRegexValidatorAttribute($"^{s_primaryRegex}{s_extRegex}$", lookingPart: VGrainIdPartEnum.Extension | VGrainIdPartEnum.Primary);

            var valid = attr.Validate(GrainId.Parse(s_grainWithGuidCompound), s_grainWithGuidCompound, NullLogger.Instance);
            Check.That(valid).IsTrue();
        }

        [Fact]
        public void Regex_Simple_ExtensionPart()
        {
            var attr = new VGrainIdRegexValidatorAttribute($"^{s_extRegex}$", lookingPart: VGrainIdPartEnum.Extension);

            var valid = attr.Validate(GrainId.Parse(s_grainWithGuidCompound), s_grainWithGuidCompound, NullLogger.Instance);
            Check.That(valid).IsTrue();
        }

        [Fact]
        public void Regex_Simple_GrainTypePart_With_ExtensionPart()
        {
            var attr = new VGrainIdRegexValidatorAttribute($"^{s_grainType}{s_extRegex}$", lookingPart: VGrainIdPartEnum.GrainType | VGrainIdPartEnum.Extension);

            var valid = attr.Validate(GrainId.Parse(s_grainWithGuidCompound), s_grainWithGuidCompound, NullLogger.Instance);
            Check.That(valid).IsTrue();
        }

        #endregion
    }
}
