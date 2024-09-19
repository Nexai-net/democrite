// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Models;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Define information reference by the <paramref name="RefId"/>
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    internal abstract record class ReferenceTarget(Uri RefId, RefTypeEnum RefType);

    /// <summary>
    /// Define Definition (Sequence, Trigger, Signal, ...) reference by the <paramref name="RefId"/>
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    internal record class ReferenceDefinitionTarget(Uri RefId, RefTypeEnum RefType, Guid DefinitionId) : ReferenceTarget(RefId, RefType);

    /// <summary>
    /// Define Type reference by the <paramref name="RefId"/>
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    internal record class ReferenceTypeTarget(Uri RefId, RefTypeEnum RefType, ConcretType Type) : ReferenceTarget(RefId, RefType);

    /// <summary>
    /// Define Method reference by the <paramref name="RefId"/>
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    internal sealed record class ReferenceTypeMethodTarget(Uri RefId, RefTypeEnum RefType, ConcretType Type, AbstractMethod Method) : ReferenceTypeTarget(RefId, RefType, Type);

}
