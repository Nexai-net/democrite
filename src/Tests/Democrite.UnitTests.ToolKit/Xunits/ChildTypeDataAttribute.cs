// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Xunits
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Xunit.Sdk;

    /// <summary>
    /// Extract all the child type as test
    /// </summary>
    /// <seealso cref="Xunit.Sdk.DataAttribute" />
    /// <typeparam name="TAsssemblyMarkerType">Source used to specify in witch assembly to search</typeparam>
    public sealed class ChildTypeDataAttribute<TParentType, TAsssemblyMarkerType> : DataAttribute
    {
        #region Fields

        private static readonly IReadOnlyCollection<object[]> s_childrenTypes;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ChildTypeDataAttribute{TParentType, TAsssemblyMarkerType}"/> class.
        /// </summary>
        static ChildTypeDataAttribute()
        {
            var refType = typeof(TParentType);
            s_childrenTypes = typeof(TAsssemblyMarkerType).Assembly
                                        .GetTypes()
                                        .Where(t => t.IsAssignableTo(refType) && t.IsAbstract == false && t.IsClass == true)
                                        .Distinct()
                                        .Select(t => new object[] { t })
                                        .ToArray();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            return s_childrenTypes;
        }

        #endregion
    }
}
