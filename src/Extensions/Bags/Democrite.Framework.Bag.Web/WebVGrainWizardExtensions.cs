// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Configurations
{
    using Democrite.Framework.Configurations;
    using Democrite.VGrains.Web;
    using Democrite.VGrains.Web.Abstractions;

    using Microsoft.Extensions.DependencyInjection;

    public static class WebVGrainWizardExtensions
    {
        /// <summary>
        /// Add web vgrains
        /// </summary>
        public static IDemocriteNodeVGrainWizard UseWebVGrains(this IDemocriteNodeVGrainWizard inst)
        {
            inst.Root.ConfigureServices(services => services.AddHttpClient());
            inst.Add<IHtmlCollectorVGrain, HtmlCollectorVGrain>();
            return inst;
        }
    }
}
