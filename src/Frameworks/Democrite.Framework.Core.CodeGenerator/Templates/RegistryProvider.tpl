// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace {{ASSEMBLY_NAMESPACE}}.SourceGen
{
	using global::Democrite.Framework.Core.Abstractions.Attributes.MetaData;
	using global::Democrite.Framework.Core.Abstractions.References;
	using global::Democrite.Framework.Core.Abstractions.Enums;

	/// <summary>
	///		Reference provider for assembly {{ASSEMBLY_NAMESPACE}}
	/// </summary>
	internal sealed class {{ASSEMBLY_NAMESPACE_CLASS_NAME}}_DemocriteReferenceProviderAttribute : DemocriteReferenceProviderAttribute
	{
		/// <inheritdoc />
		public override void Populate(DemocriteReferenceRegistry registry)
		{
			{{REGISTRIES}}
		}
	}
}