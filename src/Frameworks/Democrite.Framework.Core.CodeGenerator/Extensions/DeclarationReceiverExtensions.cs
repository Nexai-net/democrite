// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// KEEP : Microsoft.CodeAnalysis
namespace Microsoft.CodeAnalysis
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Exensions method to simplify code generator
    /// </summary>
    public static class DeclarationReceiverExtensions
    {
        #region Methods

        /// <summary>
        /// Determines whether this instance has <typeparamref name="TAttr"/> attribute.
        /// </summary>
        public static bool HasAttribute<TAttr>(this MemberDeclarationSyntax memberDeclaration)
            where TAttr : Attribute
        {
            return memberDeclaration.AttributeLists.Any(a => a.Attributes.HasAttribute<TAttr>());
        }

        /// <summary>
        /// Determines whether this instance has <typeparamref name="TAttr"/> attribute.
        /// </summary>
        public static bool HasAttribute<TAttr>(this ClassDeclarationSyntax classDeclaration)
            where TAttr : Attribute
        {
            return classDeclaration.AttributeLists.Any(a => a.Attributes.HasAttribute<TAttr>());
        }

        /// <summary>
        /// Determines whether this instance has <typeparamref name="TAttr"/> attribute.
        /// </summary>
        public static bool HasAttribute<TAttr>(this AttributeListSyntax attributeListSyntax)
            where TAttr : Attribute
        {
            return attributeListSyntax.Attributes.HasAttribute<TAttr>();
        }

        /// <summary>
        /// Determines whether this instance has <typeparamref name="TAttr"/> attribute.
        /// </summary>
        public static bool HasAttribute<TAttr>(this SeparatedSyntaxList<AttributeSyntax> attributeSyntaxes)
            where TAttr : Attribute
        {
            return attributeSyntaxes.Any(a => a.IsAttribute<TAttr>());
        }

        /// <summary>
        /// Determines whether this instance has <typeparamref name="TAttr"/> attribute.
        /// </summary>
        public static bool IsAttribute<TAttr>(this AttributeSyntax attr)
            where TAttr : Attribute
        {
            var traits = typeof(TAttr);

            var name = traits.Name;
            return attr.IsAttribute(name);
        }

        /// <summary>
        /// Gets the type full name with namespace.
        /// </summary>
        public static string GetTypeFullNameWithNamespace(this ISymbol symbol)
        {
            if (symbol is IParameterSymbol parameter)
                return GetTypeFullNameWithNamespace(parameter.Type);

            if (symbol is INamedTypeSymbol namedType)
            {
                var typeStr = "global::" + namedType.ContainingNamespace.ToString() + "." + namedType.Name;

                if (namedType.IsGenericType)
                {
                    var args = string.Join(", ", namedType.TypeParameters.Select(p => GetTypeFullNameWithNamespace(p)));
                    return typeStr + "<" + args + ">";
                }
                return typeStr;
            }

            if (symbol is ITypeSymbol type)
            {
                if (type.TypeKind == TypeKind.TypeParameter)
                {
                    return "global::Elvex.Toolbox.AnyType";
                }
                return "global::" + type.ToString().Trim('{', '}');
            }

            throw new NotImplementedException("Source Generator Symbole not managed " + symbol);
        }

        /// <summary>
        /// Gets the full name of the type.
        /// </summary>
        public static string GetTypeFullNameWithNamespace(this TypeSyntax type, Func<SyntaxNode, ISymbol> getSymbol)
        {
            var symbole = getSymbol(type.Parent);

            if (symbole != null)
                return GetTypeFullNameWithNamespace(symbole);
            return GetTypeFullName(type);
        }

        /// <summary>
        /// Gets the full name of the type.
        /// </summary>
        public static string GetTypeFullName(this TypeSyntax type, string replaceGenericBy = null)
        {
            switch (type.Kind())
            {
                case Microsoft.CodeAnalysis.CSharp.SyntaxKind.IdentifierName:
                    return ((IdentifierNameSyntax)type).Identifier.ValueText;

                case Microsoft.CodeAnalysis.CSharp.SyntaxKind.PredefinedType:
                    return ((PredefinedTypeSyntax)type).Keyword.ValueText;

                case Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullableType:
                    return ((NullableTypeSyntax)type).ElementType.GetTypeFullName() + "?";

                case Microsoft.CodeAnalysis.CSharp.SyntaxKind.QualifiedName:
                    return GetTypeFullName(((QualifiedNameSyntax)type).Left) + "." + GetTypeFullName(((QualifiedNameSyntax)type).Right);

                case Microsoft.CodeAnalysis.CSharp.SyntaxKind.AliasQualifiedName:
                    return GetTypeFullName(((AliasQualifiedNameSyntax)type).Alias);

                case Microsoft.CodeAnalysis.CSharp.SyntaxKind.GenericName:
                    return ((GenericNameSyntax)type).Identifier.Text + "<" + string.Join(", ", ((GenericNameSyntax)type).TypeArgumentList.Arguments.Select(a => GetTypeFullName(a))) + ">";

                default:
                    throw new NotImplementedException("Unknow type " + type?.GetType() ?? "null type syntax");
            }
        }

        /// <summary>
        /// Gets the full name of the type.
        /// </summary>
        public static string GetTypeSimpleName(this TypeSyntax type)
        {
            switch (type.Kind())
            {
                case Microsoft.CodeAnalysis.CSharp.SyntaxKind.IdentifierName:
                    return ((IdentifierNameSyntax)type).Identifier.ValueText;

                case Microsoft.CodeAnalysis.CSharp.SyntaxKind.PredefinedType:
                    return ((PredefinedTypeSyntax)type).Keyword.ValueText;

                case Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullableType:
                    return ((NullableTypeSyntax)type).ElementType.GetTypeFullName() + "?";

                case Microsoft.CodeAnalysis.CSharp.SyntaxKind.QualifiedName:
                    return GetTypeSimpleName(((QualifiedNameSyntax)type).Right);

                case Microsoft.CodeAnalysis.CSharp.SyntaxKind.GenericName:
                    return ((GenericNameSyntax)type).Identifier.Text + "<" + string.Join(", ", ((GenericNameSyntax)type).TypeArgumentList.Arguments.Select(a => GetTypeFullName(a))) + ">";

                default:
                    throw new NotImplementedException("Unknow type " + type?.GetType() ?? "null type syntax");
            }
        }

        /// <summary>
        /// Determines whether this instance has <typeparamref name="TAttr"/> attribute.
        /// </summary>
        public static bool HasAttribute(this MemberDeclarationSyntax memberDeclaration, string fullAttrName)
        {
            return memberDeclaration.AttributeLists.Any(a => a.Attributes.HasAttribute(fullAttrName));
        }

        /// <summary>
        /// Determines whether this instance has <typeparamref name="TAttr"/> attribute.
        /// </summary>
        public static bool HasAttribute(this ClassDeclarationSyntax classDeclaration, string fullAttrName)
        {
            return classDeclaration.AttributeLists.Any(a => a.Attributes.HasAttribute(fullAttrName));
        }

        /// <summary>
        /// Determines whether this instance has <typeparamref name="TAttr"/> attribute.
        /// </summary>
        public static bool HasAttribute(this AttributeListSyntax attributeListSyntax, string fullAttrName)
        {
            return attributeListSyntax.Attributes.HasAttribute(fullAttrName);
        }

        /// <summary>
        /// Determines whether this instance has <typeparamref name="TAttr"/> attribute.
        /// </summary>
        public static bool HasAttribute(this SeparatedSyntaxList<AttributeSyntax> attributeSyntaxes, string fullAttrName)
        {
            return attributeSyntaxes.Any(a => a.IsAttribute(fullAttrName));
        }

        /// <summary>
        /// Determines whether this instance has <typeparamref name="TAttr"/> attribute.
        /// </summary>
        public static bool IsAttribute(this AttributeSyntax attr, string fullAttrName)
        {
            var shortName = fullAttrName.Replace(nameof(Attribute), "");

            var txt = attr.GetText().ToString();

            return !string.IsNullOrEmpty(txt) && attr.Name is IdentifierNameSyntax id && string.Equals(id.Identifier.ValueText, shortName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        public static TParentNode GetParent<TParentNode>(this SyntaxNode node)
        {
            if (node?.Parent is TParentNode parentNode)
                return parentNode;

            if (node?.Parent != null)
                return node.Parent.GetParent<TParentNode>();

            return default;
        }

        /// <summary>
        /// Gets all properties.
        /// </summary>
        public static IEnumerable<IPropertySymbol> GetAllProperties(this ITypeSymbol sourceRef)
        {
            var props = sourceRef.GetMembers()
                                 .OfType<IPropertySymbol>()
                                 .Where(p => p.IsIndexer == false);

            if (sourceRef.BaseType != null)
            {
                props = props.Concat(GetAllProperties(sourceRef.BaseType));
            }

            return props;
        }

        #endregion
    }
}
