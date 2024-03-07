// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using System;

    public static class DemocriteBaseGenericBuilderExtensions
    {
        /// <summary>
        /// Safe way to convert from <see cref="IDemocriteBaseGenericBuilder.SourceOrleanBuilder"/> to <see cref="ISiloBuilder"/>
        /// </summary>
        public static ISiloBuilder GetSiloBuilder(this IDemocriteBaseGenericBuilder wizard)
        {
            ArgumentNullException.ThrowIfNull(wizard);

            // Prever type case before instead on allocating inline variable in condition if it's use outside the scope

#pragma warning disable IDE0019 // Use pattern matching
            var siloBuilder = wizard.SourceOrleanBuilder as ISiloBuilder;
#pragma warning restore IDE0019 // Use pattern matching

            if (wizard.IsClient || siloBuilder == null)
                throw new InvalidOperationException("The auto configurator must only be used by Node/Server side");

            return siloBuilder;
        }
    }
}
