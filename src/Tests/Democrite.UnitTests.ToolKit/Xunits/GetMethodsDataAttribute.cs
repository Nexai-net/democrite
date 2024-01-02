// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Xunits
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Xunit.Sdk;

    /// <summary>
    /// Provide all methods of type <typeparamref name="TType"/> value
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class GetMethodsDataAttribute : DataAttribute
    {
        #region Fields
        
        private readonly IReadOnlyCollection<object[]> _values;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumDataAttribute{TEnum}"/> class.
        /// </summary>
        public GetMethodsDataAttribute(BindingFlags flags = BindingFlags.Instance | BindingFlags.Public, params Type[] types)
        {
            this._values = types.SelectMany(trait => trait.GetAllMethodInfos(flags))
                                .Distinct()
                                .Select(v => new object[] { v })
                                .ToArray();
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
