// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    /// <summary>
    /// Virtual grain in charge to control an virtual grain build by different language (Python, C++, ...)
    /// </summary>
    public interface IVGrainRemoteController<TContextInfo> : IVGrain, IGrainWithGuidCompoundKey, IGenericContextedExecutor<TContextInfo>
    {
    }
}
