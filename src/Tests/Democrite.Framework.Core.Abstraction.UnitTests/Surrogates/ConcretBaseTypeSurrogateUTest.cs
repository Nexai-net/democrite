// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstraction.UnitTests.Surrogates
{
    using AutoFixture;
    using AutoFixture.Kernel;

    using Democrite.Framework.Core.Abstractions.Surrogates;

    using Elvex.Toolbox.Abstractions.Conditions;
    using Elvex.Toolbox.Models;
    using Elvex.Toolbox.UnitTests.ToolKit.Helpers;

    using Newtonsoft.Json;

    using NFluent;

    using System;

    /// <summary>
    /// Test for <see cref="ConcretBaseTypeSurrogate"/>
    /// </summary>
    public sealed class ConcretBaseTypeSurrogateUTest
    {
        private static readonly JsonSerializerSettings s_newtownJsonOption;
        private readonly Fixture _fixture;

        static ConcretBaseTypeSurrogateUTest()
        {
            s_newtownJsonOption = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        public ConcretBaseTypeSurrogateUTest()
        {
            var fixture = ObjectTestHelper.PrepareFixture(supportCyclingReference: true);

            fixture.Register<AbstractType>(() => (ConcretType)typeof(int).GetAbstractType());

            fixture.Register<IConcretTypeSurrogate>(() =>
            {
                var surroagte = ConcretBaseTypeConverter.ConvertToSurrogate((ConcretType)typeof(int).GetAbstractType());
                if (surroagte is ConcretTypeSurrogate concret)
                {
                    surroagte = new ConcretTypeSurrogate(concret.DisplayName,
                                                         concret.NamespaceName,
                                                         concret.AssemblyQualifiedName,
                                                         concret.IsInterface,
                                                         null!);
                }

                return surroagte;
            });
            fixture.Register<object>(() => 42);

            this._fixture = fixture;
        }

        [Theory]
        [InlineData(typeof(ConcretTypeSurrogate))]
        [InlineData(typeof(CollectionConcretTypeSurrogate))]
        public void Serialize(Type condition)
        {
            var obj = this._fixture.Create(condition, new SpecimenContext(this._fixture));

            var serializeJson = JsonConvert.SerializeObject(obj, s_newtownJsonOption);
            Check.That(serializeJson).IsNotNull().And.Not.IsEmpty();

            var reObj = JsonConvert.DeserializeObject(serializeJson, condition, s_newtownJsonOption);
            Check.That(reObj).IsNotNull();

            Check.That(JsonConvert.SerializeObject(reObj, s_newtownJsonOption)).IsNotNull().And.Not.IsEmpty().And.IsEqualTo(serializeJson);
        }

        [Theory]
        [InlineData(typeof(ConcretType))]
        [InlineData(typeof(CollectionType))]
        public void Conversion(Type condition)
        {
            var obj = this._fixture.Create(condition, new SpecimenContext(this._fixture)) as ConcretBaseType;
            Check.That(obj).IsNotNull();

            var surrogate = ConcretBaseTypeConverter.ConvertToSurrogate(obj!);

            var serializeJson = JsonConvert.SerializeObject(surrogate, s_newtownJsonOption);

            var restoreSurrogate = JsonConvert.DeserializeObject(serializeJson, surrogate!.GetType(), s_newtownJsonOption);
            Check.That(restoreSurrogate).IsNotNull();

            var restorObj = ConcretBaseTypeConverter.ConvertFromSurrogate((IConcretTypeSurrogate)restoreSurrogate!) as ConcretBaseType;
            Check.That(restorObj).IsNotNull();

            var reSurrogate = ConcretBaseTypeConverter.ConvertToSurrogate(restorObj);

            var reSerializeJson = JsonConvert.SerializeObject(reSurrogate, s_newtownJsonOption);

            Check.That(reSerializeJson).IsNotNull();
            Check.That(serializeJson).IsNotNull().And.IsEqualTo(reSerializeJson);
        }
    }
}
