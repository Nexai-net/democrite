// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Customizations
{
    using Democrite.Framework.Core.Abstractions.Deferred;
    using Democrite.Framework.Core.Abstractions.Signals;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Definition that contains all the execution customization like redirection, ...
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct ExecutionCustomizationDescriptions(IReadOnlyCollection<StageVGrainRedirectionDescription> VGrainRedirection, 
                                                            IReadOnlyCollection<EndSignalFireDescription> SignalFireDescriptions,
                                                            DeferredId? DeferredId,
                                                            bool PreventSequenceExecutorStateStorage);

    /// <summary>
    /// Define a grain redirection dedicated to a specific stage and children
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct StageVGrainRedirectionDescription(Guid? StageUid, VGrainRedirectionDefinition RedirectionDefinition);

    /// <summary>
    /// Define a signal to send at the end of the execution
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct EndSignalFireDescription(SignalId SignalId, bool IncludeResult);
}
