// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Enums;

    /// <summary>
    /// Define a instance available to execute a <see cref="SequenceDefinition"/>
    /// </summary>

    [VGrainIdFormat(IdFormatTypeEnum.Guid,
                    FirstParameterTemplate = "{executionContext." + nameof(IExecutionContext.CurrentExecutionId) + "}")]

    public interface ISequenceExecutorVGrain : IVGrain, IGrainWithGuidKey, IGenericContextedExecutor<Guid>
    {
    }
}
