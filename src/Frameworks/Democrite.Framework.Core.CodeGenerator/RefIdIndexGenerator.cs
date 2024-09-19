// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.CodeGenerator
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    [Generator]
    public sealed class RefIdIndexGenerator : ISourceGenerator, ISyntaxContextReceiver
    {
        #region Fields

        private static readonly HashSet<string> s_attributesSNI;

        private static readonly string s_assemblyInfoTemplate;
        private static readonly string s_providerTemplate;

        private readonly HashSet<MemberDeclarationSyntax> _vgrainRefToBuild;

        #endregion

        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        static RefIdIndexGenerator()
        {
            var attrSNI = new[]
            {
                "VGrainMetaDataAttribute",
                "VGrainMetaDataMethodAttribute",
                "RefSimpleNameIdentifierAttribute",
            };

            s_attributesSNI = new HashSet<string>(attrSNI.SelectMany(a => new[] { a, a.Replace(nameof(Attribute), "") }));

            var assembly = typeof(RefIdIndexGenerator).Assembly;
            var tmpl = assembly.GetManifestResourceNames()
                               .FirstOrDefault(f => f.EndsWith("RegistryProvider.tpl"));

            using (var stream = assembly.GetManifestResourceStream(tmpl))
            using (var reader = new StreamReader(stream))
            {
                s_providerTemplate = reader.ReadToEnd();
            }

            var assemblyInfoTmpl = assembly.GetManifestResourceNames()
                                           .FirstOrDefault(f => f.EndsWith("RegistryProvider.AssemblyInfo.tpl"));

            using (var stream = assembly.GetManifestResourceStream(assemblyInfoTmpl))
            using (var reader = new StreamReader(stream))
            {
                s_assemblyInfoTemplate = reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefIdIndexGenerator"/> class.
        /// </summary>
        public RefIdIndexGenerator()
        {
            this._vgrainRefToBuild = new HashSet<MemberDeclarationSyntax>();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver != this && this._vgrainRefToBuild.Any())
                return;

            var errors = new List<Diagnostic>();

            var semanticModels = context.Compilation.SyntaxTrees.Select(t => context.Compilation.GetSemanticModel(t))
                                                                 .ToArray();

            var sourceNamespace = context.Compilation.AssemblyName;

            var indexedByType = this._vgrainRefToBuild.GroupBy(s => (s.Kind(), IsVGrain: IsVGrain(s)))
                                                      .ToDictionary(k => k.Key, v => v.ToArray());

            var registryLines = new StringBuilder();

            foreach (var byType in indexedByType.OrderBy(i => i.Key.Item1 == SyntaxKind.InterfaceDeclaration
                                                                         ? 0
                                                                         : i.Key.Item1 == SyntaxKind.ClassDeclaration
                                                                                    ? 1
                                                                                    : i.Key.Item1 == SyntaxKind.StructDeclaration
                                                                                                ? 2
                                                                                                : 3))
            {
                registryLines.Append("/* --- ");
                registryLines.Append(byType.Key.ToString().Replace("Declaration", ""));
                registryLines.AppendLine(" --- */");

                var definitionType = ConvertKindTORefType(byType.Key.Item1, byType.Key.IsVGrain);

                foreach (var member in byType.Value)
                {
                    var attr = member.AttributeLists.SelectMany(a => a.Attributes)
                                                    .FirstOrDefault(a => s_attributesSNI.Any(n => a.Name is IdentifierNameSyntax id && id.Identifier.ValueText == n));

                    if (attr is null)
                        continue;

                    var attrData = member.AttributeLists.SelectMany(a => GetAttributes(a, context.Compilation))
                                                        .FirstOrDefault(a => a.ApplicationSyntaxReference?.GetSyntax() == attr);

                    var ctrParameters = attrData.AttributeConstructor.Parameters.Select(p => p.Name).ToArray();

                    var args = attr.ArgumentList.Arguments
                                                .Select((a, i) => (Arg: a, Indx: i))
                                                .ToDictionary(k => k.Arg.NameColon?.Name.Identifier.Text ??
                                                                   k.Arg.NameEquals?.Name.Identifier.Text ??
                                                                   (k.Indx < attrData.NamedArguments.Length
                                                                            ? attrData.NamedArguments[k.Indx].Key
                                                                            : ctrParameters[k.Indx]),
                                                          v => attrData.ConstructorArguments[v.Indx].ToCSharpString());

                    var memberIdentifier = (member as ClassDeclarationSyntax)?.Identifier ??
                                           (member as InterfaceDeclarationSyntax)?.Identifier ??
                                           (member as StructDeclarationSyntax)?.Identifier ??
                                           (member as RecordDeclarationSyntax)?.Identifier ??
                                           (member as EnumDeclarationSyntax)?.Identifier ??
                                           (member as MethodDeclarationSyntax)?.Identifier;

                    if (memberIdentifier is null)
                    {
                        errors.Add(Diagnostic.Create(ErrorManager.EDG002, member.GetLocation(), member.GetType()));
                        continue;
                    }

                    var model = context.Compilation.GetSemanticModel(memberIdentifier.Value.SyntaxTree).GetDeclaredSymbol(member);

                    registryLines.Append(' ', 12);
                    List<string> arguments = null;
                    int nbGenericArgs = 0;

                    if (byType.Key.Item1 == SyntaxKind.MethodDeclaration)
                    {
                        registryLines.Append("registry.RegisterMethod(");
                        var mthdMemberIdentifier = (member.Parent as ClassDeclarationSyntax)?.Identifier ?? (member.Parent as InterfaceDeclarationSyntax)?.Identifier ?? (member.Parent as StructDeclarationSyntax)?.Identifier;
                        model = context.Compilation.GetSemanticModel(mthdMemberIdentifier.Value.SyntaxTree).GetDeclaredSymbol(member.Parent);

                        var mthd = (MethodDeclarationSyntax)member;

                        arguments = new List<string>();

                        nbGenericArgs = mthd.TypeParameterList?.Parameters.Count ?? 0;

                        arguments.AddRange(mthd.ParameterList.Parameters
                                                             .Select(p => (p.Type?.GetTypeFullNameWithNamespace(t => semanticModels.Select(s =>
                                                                                                                                           {
                                                                                                                                               try
                                                                                                                                               {
                                                                                                                                                   return s.GetDeclaredSymbol(t);
                                                                                                                                               }
                                                                                                                                               catch (Exception ex)
                                                                                                                                               {
                                                                                                                                                   return null;
                                                                                                                                               }
                                                                                                                                           })
                                                                                                                                           .Where(s => s != null)
                                                                                                                                           .FirstOrDefault()) ?? "global::Elvex.Toolbox.AnyType"))
                                                             .Select(s => "typeof(" + s + ")"));
                    }
                    else
                    {
                        registryLines.Append("registry.Register(");
                    }

                    var type = (ITypeSymbol)model;

                    if (byType.Key.Item1 != SyntaxKind.MethodDeclaration)
                    {
                        registryLines.Append("RefTypeEnum.");
                        registryLines.Append(definitionType);
                        registryLines.Append(", ");
                    }

                    if (args.TryGetValue("simpleNameIdentifier", out var sni))
                    {
                        registryLines.Append(sni.Trim());
                    }
                    else
                    {
                        errors.Add(Diagnostic.Create(ErrorManager.EDG001, attr.GetLocation()));
                        continue;
                    }

                    registryLines.Append(", ");

                    if (byType.Key.Item1 != SyntaxKind.MethodDeclaration)
                    {
                        if (args.TryGetValue("namespaceIdentifier", out var @namespace))
                            registryLines.Append(@namespace.Trim());
                        else
                            registryLines.Append("null");
                    }
                    else
                    {
                        registryLines.Append("\"");
                        registryLines.Append(memberIdentifier.Value.Text.Trim());
                        registryLines.Append("\"");
                    }

                    var typeName = type.ToString().Trim('{', '}');

                    if (type.IsType && type is INamedTypeSymbol namedType && namedType.IsGenericType)
                    {
                        // Search if type is generic
                        if (namedType.TypeArguments.Any(t => t.IsDefinition))
                        {
                            var supIndexOf = typeName.IndexOf('<');
                            if (supIndexOf > 0)
                                typeName = typeName.Substring(0, supIndexOf) + "<" + new string(',', namedType.TypeParameters.Length - 1) + ">";
                        }
                    }

                    registryLines.Append(", typeof(global::" + typeName + ")");

                    if (byType.Key.Item1 == SyntaxKind.MethodDeclaration)
                    {
                        registryLines.Append(", ");
                        registryLines.Append(nbGenericArgs);
                    }

                    if (arguments != null && arguments.Any())
                    {
                        foreach (var arg in arguments)
                        {
                            registryLines.Append(',');
                            registryLines.Append(arg);
                        }
                    }

                    registryLines.AppendLine(");");
                }

                registryLines.AppendLine();
                registryLines.Append(' ', 12);
            }

            foreach (var error in errors)
                context.ReportDiagnostic(error);

            var registrySource = s_providerTemplate.Replace("{{ASSEMBLY_NAMESPACE}}", sourceNamespace)
                                                   .Replace("{{ASSEMBLY_NAMESPACE_CLASS_NAME}}", sourceNamespace.Replace(".", "_"))
                                                   .Replace("{{REGISTRIES}}", registryLines.ToString());

            context.AddSource("DemocriteReferenceRegistry.g.cs", registrySource);
            context.AddSource("DemocriteReferenceRegistry.AssemblyInfo.g.cs", s_assemblyInfoTemplate.Replace("{{ASSEMBLY_NAMESPACE}}", sourceNamespace)
                                                                                                    .Replace("{{ASSEMBLY_NAMESPACE_CLASS_NAME}}", sourceNamespace.Replace(".", "_")));
        }

        public static IReadOnlyList<AttributeData> GetAttributes(AttributeListSyntax attributes, Compilation compilation)
        {
            // Collect pertinent syntax trees from these attributes
            var acceptedTrees = new HashSet<SyntaxTree>();
            foreach (var attribute in attributes.Attributes)
                acceptedTrees.Add(attribute.SyntaxTree);

            var parentSymbol = GetDeclaredSymbol(attributes.Parent, compilation);
            var parentAttributes = parentSymbol.GetAttributes();
            var ret = new List<AttributeData>();
            foreach (var attribute in parentAttributes)
            {
                if (acceptedTrees.Contains(attribute.ApplicationSyntaxReference.SyntaxTree))
                    ret.Add(attribute);
            }

            return ret;
        }

        public static ISymbol GetDeclaredSymbol(SyntaxNode node, Compilation compilation)
        {
            var model = compilation.GetSemanticModel(node.SyntaxTree);
            return model.GetDeclaredSymbol(node);
        }

        /// <inheritdoc />
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => this);
        }

        /// <inheritdoc />
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is MemberDeclarationSyntax classDeclaration)
            {
                if (s_attributesSNI.Any(a => classDeclaration.HasAttribute(a)))
                    this._vgrainRefToBuild.Add(classDeclaration);
            }
        }

        #region Tools

        private string ConvertKindTORefType(SyntaxKind key, bool isVGrain)
        {
            switch (key)
            {
                case SyntaxKind.ClassDeclaration:
                    return isVGrain ? "VGrainImplementation" : "Type";

                case SyntaxKind.StructDeclaration:
                case SyntaxKind.RecordDeclaration:
                case SyntaxKind.RecordStructDeclaration:
                case SyntaxKind.EnumDeclaration:
                    return "Type";

                case SyntaxKind.InterfaceDeclaration:
                    return "VGrain";
            }

            return "Other";
        }

        /// <summary>
        /// Detect if the type is a vgrain
        /// </summary>
        private bool IsVGrain(MemberDeclarationSyntax s)
        {
            return (s is ClassDeclarationSyntax || s is InterfaceDeclarationSyntax) &&
                    s.Ancestors()
                     .SelectMany(m => (s is ClassDeclarationSyntax cls
                                             ? cls.BaseList?.Types
                                                 : s is InterfaceDeclarationSyntax i
                                                         ? (IEnumerable<BaseTypeSyntax>)i.BaseList?.Types
                                                         : Array.Empty<BaseTypeSyntax>()) ?? Array.Empty<BaseTypeSyntax>())
                     .Any(c => c?.Type != null && c.Type.GetTypeSimpleName() == "IVGrain");
        }

        #endregion

        #endregion
    }
}
