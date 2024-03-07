// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.UnitTests.Extensions
{
    using Democrite.Framework.Toolbox.Extensions;

    using NFluent;

    using System.Collections.Generic;

    public class TypeExtensionsUTest
    {
        /// <summary>
        /// Test Extensionses <see cref="ITypeInfoExtesion.IsCollection"/>
        /// </summary>
        [Fact]
        public void Extensions_IsCollection()
        {
            Check.That(typeof(int).GetTypeInfoExtension().IsCollection).IsEqualTo(false);

            Check.That(typeof(string).GetTypeInfoExtension().IsCollection).IsEqualTo(true);
            Check.That(typeof(float[]).GetTypeInfoExtension().IsCollection).IsEqualTo(true);

            Check.That(typeof(TypeExtensionsUTest).GetTypeInfoExtension().IsCollection).IsEqualTo(false);

            Check.That(typeof(List<string>).GetTypeInfoExtension().IsCollection).IsEqualTo(true);
        }

        /// <summary>
        /// Test Extensionses <see cref="ITypeInfoExtesion.CollectionItemType"/>
        /// </summary>
        [Fact]
        public void Extensions_GetItemCollectionType()
        {
            Check.That(typeof(int).GetTypeInfoExtension().CollectionItemType).IsEqualTo(null);

            Check.That(typeof(string).GetTypeInfoExtension().CollectionItemType).IsEqualTo(typeof(char));
            Check.That(typeof(float[]).GetTypeInfoExtension().CollectionItemType).IsEqualTo(typeof(float));
            Check.That(typeof(List<string>).GetTypeInfoExtension().CollectionItemType).IsEqualTo(typeof(string));
        }


        /// <summary>
        /// Test Extensionses <see cref="ITypeInfoExtesion.CollectionItemType"/>
        /// </summary>
        [Fact]
        public void Extensions_IsValueTask()
        {
            Check.That(typeof(ValueTask).GetTypeInfoExtension().IsTask).IsFalse();
            Check.That(typeof(ValueTask).GetTypeInfoExtension().IsValueTask).IsTrue();

            Check.That(typeof(ValueTask<string>).GetTypeInfoExtension().IsTask).IsFalse();
            Check.That(typeof(ValueTask<string>).GetTypeInfoExtension().IsValueTask).IsTrue();

            Check.That(typeof(Task).GetTypeInfoExtension().IsValueTask).IsFalse();
            Check.That(typeof(Task<string>).GetTypeInfoExtension().IsValueTask).IsFalse();
            Check.That(typeof(string).GetTypeInfoExtension().IsValueTask).IsFalse();
            Check.That(typeof(float[]).GetTypeInfoExtension().IsValueTask).IsFalse();
            Check.That(typeof(List<string>).GetTypeInfoExtension().IsValueTask).IsFalse();
        }


    }
}
