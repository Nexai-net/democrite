// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.CodeGenerator
{
    using Democrite.Framework.Core.Abstractions.Attributes.MetaData;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.References;
    using Democrite.Framework.Core.CodeGenerator.UnitTests;
    using Democrite.Framework.Core.CodeGenerator.UnitTests.Models;

    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Loggers;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using NFluent;

    using System.Reflection;

    using Xunit;

    /// <summary>
    /// Test for Codegen, ensure all the code gen are valid
    /// </summary>
    public sealed class CodeGenUTest
    {
        #region Fields

        private readonly static Assembly s_current;
        private static readonly DemocriteReferenceProviderAttribute? s_attr;
        private static readonly Type[] s_expectedType;
        private static readonly Type[] s_expectedVGrainType;

        private static readonly Dictionary<MethodInfo, VGrainMetaDataMethodAttribute> s_expectedVGrainMethods;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="CodeGenUTest"/> class.
        /// </summary>
        static CodeGenUTest()
        {
            s_current = typeof(CodeGenUTest).Assembly;
            s_attr = s_current.GetCustomAttributes().OfType<DemocriteReferenceProviderAttribute>().SingleOrDefault();

            s_expectedType = new Type[]
            {
                typeof(SimpleClass),
                typeof(SimpleStruct),
                typeof(ClassicClass),
                typeof(ClassicStruct),
                typeof(EnumType)
            };

            s_expectedVGrainType = new Type[]
            {
                typeof(ISimpleGrain),
                typeof(IChildSimpleGrain),
                typeof(IGenericGrain<>),
                typeof(IGenericMultipleGrain<,>),
                typeof(IGenericWithConstraintGrain<>),
                typeof(ISimpleWithMethodGrain),
                typeof(IInheriteWithMethodGrain),
                typeof(IGenericSimpleWithMethodGrain<>)
            };

            s_expectedVGrainMethods = s_expectedVGrainType.SelectMany(v => v.GetMethods())
                                                          .Where(m => m.GetCustomAttribute<VGrainMetaDataMethodAttribute>() != null)
                                                          .ToDictionary(m => m, kv => kv.GetCustomAttribute<VGrainMetaDataMethodAttribute>()!);
        }

        #endregion

        #region Methods

        [Fact]
        public void CodeGen_Validated()
        {
            Check.That(s_attr).IsNotNull();

            var testLogger = new InMemoryLogger(new LoggerFilterOptions().ToMonitorOption());

            var textCtx = new DemocriteReferenceRegistry(testLogger);
            s_attr!.Populate(textCtx);

            var logs = testLogger.GetLogsCopy();
            Check.That(logs).IsNullOrEmpty();

            var allReferences = textCtx.GetReferences();

            var indexByRefType = allReferences.GroupBy(k => k.RefType)
                                              .ToDictionary(k => k.Key, v => v.Select(s => s).ToArray());

            CheckValidType(RefTypeEnum.Type, indexByRefType, s_expectedType);
            CheckValidType(RefTypeEnum.VGrain, indexByRefType, s_expectedVGrainType);
            CheckMethod(indexByRefType);
        }

        #region Tools

        private static void CheckMethod(Dictionary<RefTypeEnum, ReferenceTarget[]> indexByType)
        {
            var methodFoundes = indexByType.TryGetValue(RefTypeEnum.Method, out var methodRefs);
            Check.WithCustomMessage("Expected " + RefTypeEnum.Method + " Ref").That(methodFoundes).IsTrue();

            Check.That(methodRefs).IsNotNull()
                                  .And
                                  .CountIs(s_expectedVGrainMethods.Count)
                                  .And
                                  .ContainsOnlyInstanceOfType(typeof(ReferenceTypeMethodTarget));

            var hashMthd = methodRefs!.OfType<ReferenceTypeMethodTarget>().ToDictionary(m => m.RefId.Fragment.Trim('#'));

            foreach (var mth in s_expectedVGrainMethods)
            {
                var found = hashMthd.TryGetValue(mth.Value!.SimpleNameIdentifier, out var refMthd);

                Check.That(found).IsTrue();
                Check.That(refMthd).IsNotNull();

                var refMthType = refMthd!.Type.ToType();
                Check.That(refMthType).IsNotNull();

                var abstractMthd = mth.Key.GetAbstractMethod();
                Check.That(refMthd.Method).IsEqualTo(abstractMthd);
            }
        }

        private static void CheckValidType(RefTypeEnum expectedRefType, Dictionary<RefTypeEnum, ReferenceTarget[]> indexByType, IReadOnlyCollection<Type> expectedTypes)
        {
            var typeFounded = indexByType.TryGetValue(expectedRefType, out var typeRefs);
            Check.WithCustomMessage("Expected " + expectedRefType + " Ref").That(typeFounded).IsTrue();

            Check.That(typeRefs).IsNotNull()
                                .And
                                .CountIs(expectedTypes.Count)
                                .And
                                .ContainsOnlyInstanceOfType(typeof(ReferenceTypeTarget));

            foreach (var expectedType in expectedTypes)
            {
                var compareType = expectedType.GetAbstractType();

                var targetRefs = typeRefs!.OfType<ReferenceTypeTarget>()
                                          .Where(tr => tr.Type.ToType() == expectedType)
                                          .ToArray();

                Check.WithCustomMessage("Ref Type " + expectedType).That(targetRefs).IsNotNull().And.CountIs(1);

                var targetRef = targetRefs.Single();
                Check.That(targetRef).IsNotNull();

                RefIdHelper.Explode(targetRef.RefId, out var type, out var ns, out var sni);

                Check.That(type).IsEqualTo(expectedRefType);
                Check.That(ns).IsNotNull().And.IsNotEmpty().And.IsEqualTo(CodeGenTestConstants.BagNamespace);

                var compareName = expectedType.Name;

                if (expectedType.IsGenericType)
                    compareName = expectedType.Name.Substring(0, expectedType.Name.IndexOf('`'));

                Check.That(sni).IsNotNull().And.IsNotEmpty().And.IsEqualTo(compareName.ToLowerWithSeparator('-'));
            }
        }

        #endregion
     
        #endregion
    }
}
