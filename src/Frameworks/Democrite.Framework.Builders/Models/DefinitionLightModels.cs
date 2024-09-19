// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Models
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Root object deserialized by the <see cref="IDefinitionCompiler"/> from yaml to build
    /// </summary>
    internal record class LightDefinitions(GlobalOptionDefintion Global,
                                           IReadOnlyCollection<TriggerBaseDef> Triggers,
                                           IReadOnlyCollection<SignalDef> Signals,
                                           IReadOnlyCollection<StreamQueueDef> Streams,
                                           IReadOnlyCollection<SequenceDef> Sequences);

    /// <summary>
    /// Define meta data applied to all compilations
    /// </summary>
    internal record class GlobalOptionDefintion(string? Namespace);

    /// <summary>
    /// Define meta data information apply to each <see cref="IDefinition"/> if needed
    /// </summary>
    internal record class MetaDataDef(string? Description,
                                      string? Category,
                                      string? Namespace,
                                      string[] Tags);

    /// <summary>
    /// Base class to any definition, signal, stream, sequence ...
    /// </summary>
    internal abstract record class BaseDef(Guid? Uid,
                                           string sni,
                                           MetaDataDef? MetaData);

    /// <summary>
    /// Define base class to any trigger to defined trigger targets
    /// </summary>
    internal sealed record class TriggerTargets(IReadOnlyCollection<string> Sequences,
                                                IReadOnlyCollection<string> Signals,
                                                IReadOnlyCollection<string> Streams);

    /// <summary>
    /// Base class to group data provider kind
    /// </summary>
    internal abstract record class TriggerAbstractOutput(string OutputType);

    /// <summary>
    /// Define a data provider using a static collection
    /// </summary>
    internal record class TriggerStaticCollectionOutput(string Type,
                                                        PullModeEnum Mode,
                                                        IReadOnlyCollection<string> Values) : TriggerAbstractOutput("static");
             
    /// <summary>
    /// Base class to any Trigger
    /// </summary>
    internal abstract record class TriggerBaseDef(Guid? Uid,
                                                  string sni,
                                                  string Type,
                                                  MetaDataDef? MetaData,
                                                  TriggerTargets Targets) : BaseDef(Uid, sni, MetaData);

    /// <summary>
    /// Define a trigger using data from a stream as input and starter
    /// </summary>
    internal record class TriggerStreamDef(Guid? Uid,
                                           string Stream,
                                           string From,
                                           uint? MaxConsumer,
                                           uint? MaxConsumerByNode,
                                           MetaDataDef? MetaData,
                                           TriggerTargets Targets) : TriggerBaseDef(Uid, Stream, "stream", MetaData, Targets);

    /// <summary>
    /// Define a trigger using signal as starter
    /// </summary>
    internal record class TriggerSignalDef(Guid? Uid,
                                           string Signal,
                                           string From,
                                           TriggerAbstractOutput? Output,
                                           MetaDataDef? MetaData,
                                           TriggerTargets Targets) : TriggerBaseDef(Uid, Signal, "signal", MetaData, Targets);

    /// <summary>
    /// Define a trigger using a period describe in 'cron' format to start
    /// </summary>
    internal record class TriggerCronDef(Guid? Uid,
                                         string Cron,
                                         string Period,
                                         TriggerAbstractOutput? Output,
                                         MetaDataDef? MetaData,
                                         TriggerTargets Targets) : TriggerBaseDef(Uid, Cron, "cron", MetaData, Targets);

    /// <summary>
    /// Define a signal definition
    /// </summary>
    internal record class SignalDef(Guid? Uid,
                                    string Signal,
                                    MetaDataDef? MetaData) : BaseDef(Uid, Signal, MetaData);

    /// <summary>
    /// Define a stream queue
    /// </summary>
    internal record class StreamQueueDef(Guid? Uid,
                                         string Stream,
                                         string? configName,
                                         string QueueNamespace,
                                         string QueueName,
                                         MetaDataDef? MetaData) : BaseDef(Uid, Stream, MetaData);

    /// <summary>
    /// Define a sequence definition
    /// </summary>
    internal record class SequenceDef(Guid? Uid,
                                      string Sequence,
                                      string RequiredInput,
                                      MetaDataDef? MetaData,
                                      IReadOnlyCollection<SequenceBaseStageDef> Stages) : BaseDef(Uid, Sequence, MetaData);

    /// <summary>
    /// Define data are get
    /// </summary>
    internal enum SelectionModeEnum
    {
        None,
        Property,
        Build
    }

    /// <summary>
    /// Define an association between type and how to build it, or get it
    /// </summary>
    internal record class TypeConfig(string Type,
                                     SelectionModeEnum Mode,
                                     ReadOnlyCollection<string> From);

    /// <summary>
    /// Base class of every sequence steps
    /// </summary>
    internal abstract record class SequenceBaseStageDef(string Type);

    /// <summary>
    /// Sequence step used to call a method in a grain
    /// </summary>
    internal record class SequenceUseStageDef(string Use,
                                             string Call,
                                             TypeConfig? Config) : SequenceBaseStageDef("use");

    /// <summary>
    /// Sequence step 'Select' to manipulate the input to provide a different output
    /// </summary>
    internal record class SequenceSelectStageDef(string Select,
                                                SelectionModeEnum Mode,
                                                IReadOnlyCollection<string> From) : SequenceBaseStageDef("select");
}
