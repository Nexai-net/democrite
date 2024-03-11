// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Elvex.Toolbox.UnitTests.Xunits
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Xunit.Sdk;

    /// <summary>
    /// Provide one test by <typeparamref name="TEnum"/> value
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class EnumDataAttribute<TEnum> : DataAttribute
        where TEnum : struct, System.Enum
    {
        #region Fields
        
        private readonly IReadOnlyCollection<object[]> _values;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumDataAttribute{TEnum}"/> class.
        /// </summary>
        public EnumDataAttribute(params TEnum[] skipValues)
        {
            this._values = Enum.GetValues<TEnum>()
                               .Where(v => !skipValues.Contains(v))
                               .Select(v => new object[] { v })
                               .ToList();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            return _values;
        }

        #endregion
    }
}
