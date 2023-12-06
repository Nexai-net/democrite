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
            Check.That(typeof(int).GetTypeIntoExtension().IsCollection).IsEqualTo(false);

            Check.That(typeof(string).GetTypeIntoExtension().IsCollection).IsEqualTo(true);
            Check.That(typeof(float[]).GetTypeIntoExtension().IsCollection).IsEqualTo(true);

            Check.That(typeof(TypeExtensionsUTest).GetTypeIntoExtension().IsCollection).IsEqualTo(false);

            Check.That(typeof(List<string>).GetTypeIntoExtension().IsCollection).IsEqualTo(true);
        }

        /// <summary>
        /// Test Extensionses <see cref="ITypeInfoExtesion.CollectionItemType"/>
        /// </summary>
        [Fact]
        public void Extensions_GetItemCollectionType()
        {
            Check.That(typeof(int).GetTypeIntoExtension().CollectionItemType).IsEqualTo(null);

            Check.That(typeof(string).GetTypeIntoExtension().CollectionItemType).IsEqualTo(typeof(char));
            Check.That(typeof(float[]).GetTypeIntoExtension().CollectionItemType).IsEqualTo(typeof(float));
            Check.That(typeof(List<string>).GetTypeIntoExtension().CollectionItemType).IsEqualTo(typeof(string));
        }


        /// <summary>
        /// Test Extensionses <see cref="ITypeInfoExtesion.CollectionItemType"/>
        /// </summary>
        [Fact]
        public void Extensions_IsValueTask()
        {
            Check.That(typeof(ValueTask).GetTypeIntoExtension().IsTask).IsFalse();
            Check.That(typeof(ValueTask).GetTypeIntoExtension().IsValueTask).IsTrue();

            Check.That(typeof(ValueTask<string>).GetTypeIntoExtension().IsTask).IsFalse();
            Check.That(typeof(ValueTask<string>).GetTypeIntoExtension().IsValueTask).IsTrue();

            Check.That(typeof(Task).GetTypeIntoExtension().IsValueTask).IsFalse();
            Check.That(typeof(Task<string>).GetTypeIntoExtension().IsValueTask).IsFalse();
            Check.That(typeof(string).GetTypeIntoExtension().IsValueTask).IsFalse();
            Check.That(typeof(float[]).GetTypeIntoExtension().IsValueTask).IsFalse();
            Check.That(typeof(List<string>).GetTypeIntoExtension().IsValueTask).IsFalse();
        }


    }
}
