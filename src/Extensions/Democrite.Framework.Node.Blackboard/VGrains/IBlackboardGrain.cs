// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    using Orleans.Concurrency;

    /// <summary>
    /// Blackboard grain 
    /// </summary>
    /// <seealso cref="IVGrain" />
    /// <seealso cref="IGrainWithGuidKey" />
    [DemocriteSystemVGrain]
    internal interface IBlackboardGrain : IVGrain, IGrainWithGuidKey, IBlackboard
    {
        /// <summary>
        /// Gets the <see cref="BlackboardId"/> identifier
        /// </summary>
        [ReadOnly]
        Task<BlackboardId> GetIdentityAsync();

        /// <summary>
        /// Builds from template, the build will occured only once. Calling multiple times will not create exception
        /// </summary>
        /// <remarks>
        ///     Attention this action could one be once.
        ///     
        ///     The blackboard will build using a copy of the template.
        ///     An keep ths copy.
        ///     
        ///     The template contains some data rules and changing on the fly could corrupt the data
        ///     
        ///     TODO : Migration process to transfert data store in blackboard to another one
        ///     
        /// </remarks>
        Task BuildFromTemplateAsync(Guid blackboardTemplateUid, BlackboardId blackboardId, GrainCancellationToken token, Guid? callContextId);
    }
}
