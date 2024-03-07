// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.UnitTests.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Services;

    using Microsoft.Extensions.Logging.Abstractions;

    using Moq;

    using NFluent;

    using System;
    using System.Reflection;

    /// <summary>
    /// Unit test <see cref="VGrainIdFactoryDedicatedTemplates"/>
    /// </summary>
    public sealed class VGrainIdFactoryDedicatedTemplatesUnitTests
    {
        #region Fields

        private const string FALLBACK_GUID = "BA790314-02ED-4488-B301-830B67066E4F";
        private const string FIX_VALUE_FALLBACK_GUID = "6BA8E199-C767-4F55-A0F1-5CF5DB5185A8";

        private const string BEFORE_SAMPLE = "Before42:";

        private const string AFTER_SAMPLE = ":42After";

        private static readonly MethodInfo s_genericBuildIdMethod;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="VGrainIdFactoryDedicatedTemplatesDedicatedTemplatesUnitTests"/> class.
        /// </summary>
        static VGrainIdFactoryDedicatedTemplatesUnitTests()
        {
            var genericBuildIdMethod = typeof(VGrainIdFactoryDedicatedTemplatesUnitTests).GetMethod(nameof(BuildId), BindingFlags.NonPublic | BindingFlags.Static);
            ArgumentNullException.ThrowIfNull(genericBuildIdMethod);

            s_genericBuildIdMethod = genericBuildIdMethod;
        }

        #endregion

        #region Nested

        [VGrainIdFormat(IdFormatTypeEnum.Guid)]
        internal interface ISimpleGuidWithoutTemplateVGrain : IVGrain { }

        [VGrainIdFormat(IdFormatTypeEnum.Guid, FirstParameterTemplate = "{new}")]
        internal interface ISimpleGuidWithNewTemplateVGrain : IVGrain { }

        [VGrainIdFormat(IdFormatTypeEnum.Guid, FirstParameterTemplate = "{input.Uid}")]
        internal interface ISimpleGuidWithInputTemplateVGrain : IVGrain { }

        [VGrainIdFormat(IdFormatTypeEnum.Guid, FirstParameterTemplate = "{input.Uid}", FirstParameterFallback = FALLBACK_GUID)]
        internal interface ISimpleGuidWithInputFallbackTemplateVGrain : IVGrain { }

        [VGrainIdFormat(IdFormatTypeEnum.String, FirstParameterTemplate = BEFORE_SAMPLE + "{input.Uid}", FirstParameterFallback = BEFORE_SAMPLE + FALLBACK_GUID)]
        internal interface ISimpleStringWithInputBeforeFallbackTemplateVGrain : IVGrain { }

        [VGrainIdFormat(IdFormatTypeEnum.String, FirstParameterTemplate = "{input.Uid}" + AFTER_SAMPLE, FirstParameterFallback = FALLBACK_GUID + AFTER_SAMPLE)]
        internal interface ISimpleStringWithInputAfterFallbackTemplateVGrain : IVGrain { }

        [VGrainIdFormat(IdFormatTypeEnum.String, FirstParameterTemplate = BEFORE_SAMPLE + "{input.Uid}" + AFTER_SAMPLE, FirstParameterFallback = BEFORE_SAMPLE + FALLBACK_GUID + AFTER_SAMPLE)]
        internal interface ISimpleStringWithInputBeforeAndAfterFallbackTemplateVGrain : IVGrain { }

        [VGrainIdFormat(IdFormatTypeEnum.Guid, FirstParameterTemplate = "{executionContext.Configuration}")]
        internal interface ISimpleGuidWithExecContextTemplateVGrain : IVGrain { }

        [VGrainIdFormat(IdFormatTypeEnum.Guid, FirstParameterTemplate = "{executionContext.Configuration}", FirstParameterFallback = FALLBACK_GUID)]
        internal interface ISimpleGuidWithExecContextFallbackTemplateVGrain : IVGrain { }

        [VGrainIdFormat(IdFormatTypeEnum.Guid, FirstParameterTemplate = FIX_VALUE_FALLBACK_GUID, FirstParameterFallback = FALLBACK_GUID)]
        internal interface ISimpleGuidWithFixValueTemplateVGrain : IVGrain { }

        #endregion

        #region Methods

        /// <summary>
        /// Ensure that <see cref="VGrainIdFactoryDedicatedTemplates"/> could be disposed and public call correctly protected
        /// </summary>
        [Fact]
        public void VGrainIdFactoryDedicatedTemplates_DisposableCheck()
        {
            var factory = new VGrainIdFactoryDedicatedTemplates();
            using (factory)
            {
                Check.That(factory).IsNotNull();
                Check.That(factory.IsDisposed).IsFalse();
            }

            Check.That(factory).IsNotNull();
            Check.That(factory.IsDisposed).IsTrue();

            Check.ThatCode(() => factory.BuildNewId<ISimpleGuidWithNewTemplateVGrain>(null, new Mock<IExecutionContext>().Object, NullLogger.Instance)).Throws<ObjectDisposedException>();
        }

        /// <summary>
        /// Ensure that <see cref="VGrainIdFactoryDedicatedTemplates"/> correctly used fallback value of the first id template
        /// </summary>
        [Theory]
        [InlineData(typeof(ISimpleGuidWithInputFallbackTemplateVGrain), FALLBACK_GUID)]

        [InlineData(typeof(ISimpleStringWithInputBeforeFallbackTemplateVGrain), BEFORE_SAMPLE + FALLBACK_GUID)]
        [InlineData(typeof(ISimpleStringWithInputAfterFallbackTemplateVGrain), FALLBACK_GUID + AFTER_SAMPLE)]
        [InlineData(typeof(ISimpleStringWithInputBeforeAndAfterFallbackTemplateVGrain), BEFORE_SAMPLE + FALLBACK_GUID + AFTER_SAMPLE)]

        [InlineData(typeof(ISimpleGuidWithExecContextFallbackTemplateVGrain), FALLBACK_GUID)]
        [InlineData(typeof(ISimpleGuidWithFixValueTemplateVGrain), FIX_VALUE_FALLBACK_GUID)]
        public void VGrainIdFactoryDedicatedTemplates_IdGenerationFromTemplateFallback(Type vgrainType, string expectedId)
        {
            BuildIdFlat(vgrainType, a => string.Equals(a.GetVGrainIdPrimaryValue<string>(), expectedId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Ensure that <see cref="VGrainIdFactoryDedicatedTemplates"/> will correctly recover through fallback value even if correct builder have been put in cache
        /// </summary>
        [Fact]
        public void VGrainIdFactoryDedicatedTemplates_ExecutionContextIdProperty_WithCache()
        {
            using (var factory = new VGrainIdFactoryDedicatedTemplates())
            {
                var executionWithoutConfig = new Models.ExecutionContext(Guid.NewGuid(), Guid.NewGuid(), null);

                // vgrain need the execution context to expose a configuration field
                // test to generate id wihtout the configuration to see if the fall is well used

                var genId = factory.BuildNewId< ISimpleGuidWithExecContextFallbackTemplateVGrain>(null, executionWithoutConfig, NullLogger.Instance);

                Check.That(genId.GetVGrainIdPrimaryValue<string>()).IsEqualTo(FALLBACK_GUID, StringComparer.OrdinalIgnoreCase);

                // Send correct executionContext with configuration
                var configurationId = Guid.NewGuid();
                var executionWithConfig = new Models.ExecutionContextWithConfiguration<Guid>(Guid.NewGuid(), Guid.NewGuid(), null, configurationId);

                var genWithCfgId = factory.BuildNewId<ISimpleGuidWithExecContextFallbackTemplateVGrain>(null, executionWithConfig, NullLogger.Instance);

                // Check configuration is well used
                Check.That(genWithCfgId.GetVGrainIdPrimaryValue<string>()).IsEqualTo(configurationId.ToString(), StringComparer.OrdinalIgnoreCase);

                // factory must now have put the vgrain id generation expresion in cache
                // Test to send invalid execution context to force fallback usage

                var genRedoId = factory.BuildNewId<ISimpleGuidWithExecContextFallbackTemplateVGrain>(null, executionWithoutConfig, NullLogger.Instance);
                Check.That(genRedoId.GetVGrainIdPrimaryValue<string>()).IsEqualTo(FALLBACK_GUID, StringComparer.OrdinalIgnoreCase);
            }
        }

        #region Tools

        /// <summary>
        /// Call <see cref="IVGrainIdFactoryDedicatedTemplates.BuildNewId{TVGrain}(object?, IExecutionContext)"/> and compare result
        /// </summary>
        private static void BuildId<TVGrain>(Func<IVGrainId, bool> validation, object? input = null, IExecutionContext? execution = null)
            where TVGrain : IVGrain
        {
            using (var factory = new VGrainIdFactoryDedicatedTemplates())
            {
                execution ??= new Models.ExecutionContext(Guid.NewGuid(), Guid.NewGuid(), null);

                var genId = factory.BuildNewId<TVGrain>(input, execution, NullLogger.Instance);
                Check.ThatCode(() => validation(genId)).DoesNotThrow().And.WhichResult().IsTrue();
            }
        }

        /// <summary>
        /// Proxy above generic method <see cref=""/>
        /// </summary>
        private static void BuildIdFlat(Type vgrainType, Func<IVGrainId, bool> validation, object? input = null, IExecutionContext? execution = null)
        {
            s_genericBuildIdMethod!.MakeGenericMethod(vgrainType)
                                   .Invoke(null, new object?[] { validation, input, execution });
        }

        #endregion

        #endregion
    }
}
