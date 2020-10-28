//#define LAUNCH_DEBUGGER

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace UpdateGenerator
{
    [Generator]
    public class UpdateGenerator : ISourceGenerator
    {
        static readonly IComparer<ClassDeclarationSyntax> classDeclarationSyntaxComparer = Comparer<ClassDeclarationSyntax>.Create((x, y) =>
        {
            return x.Identifier.ToString().CompareTo(y.Identifier.ToString());
        });

        public void Initialize(GeneratorInitializationContext context)
        {
#if LAUNCH_DEBUGGER
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif 
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // retrieve the populated receiver 
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            var compilation = (CSharpCompilation)context.Compilation;
            // TODO: Get baseType programmatically
            var baseType = compilation.GetTypeByMetadataName("Entities.Entity");

            SourceText sourceText;

            using var stringWriter = new StringWriter();
            using var itw = new IndentedTextWriter(stringWriter, "    ");

            var candidateClasses = receiver.CandidateClasses
                .Where(classDeclarationSyntax =>
                {
                    var semanticClassModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
                    var classSymbol = semanticClassModel.GetDeclaredSymbol(classDeclarationSyntax);
                    return classSymbol.IsDerivedFrom(baseType);
                })
                .OrderBy(classDeclarationSyntax => classDeclarationSyntax, classDeclarationSyntaxComparer)
                .ToList();

            itw.WriteLine("using System;");
            itw.WriteLine();

            // TODO: Get Namespace from compilation
            using (itw.StartScope("namespace Entities"))
            {
                foreach (var candidateClass in candidateClasses)
                {
                    using (itw.StartScope($"partial class {candidateClass.Identifier.ValueText}"))
                    {
                        using (itw.StartScope($"public override {candidateClass.Identifier.ValueText} Update({candidateClass.Identifier.ValueText} other)"))
                        {
                            using (itw.StartScope($"if (other is null)"))
                            {
                                itw.WriteLine("return this;");
                            }

                            foreach (var classMemberDeclarationSyntax in candidateClass.Members)
                            {
                                var propertyDeclarationSyntax = classMemberDeclarationSyntax as PropertyDeclarationSyntax;

                                var propertySemanticModel = compilation.GetSemanticModel(classMemberDeclarationSyntax.SyntaxTree);
                                var propertySymbol = propertySemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax);

                                if (propertySymbol.IsReadOnly)
                                {
                                    continue;
                                }

                                ITypeSymbol propertyTypeSymbol = propertySymbol.Type;
                                var isDerivedFromEntity = propertyTypeSymbol.IsDerivedFrom(baseType);

                                //var isSimpleType = GeneratorHelpers.IsSimpleType(typeSymbol);

                                if (isDerivedFromEntity)
                                {
                                    // The member is another type derived from entity, so we can call the update method on it.
                                    itw.WriteTernaryAssignment(
                                        $"this.{propertyDeclarationSyntax.Identifier}",
                                        $"this.{propertyDeclarationSyntax.Identifier} is null",
                                        $"other.{propertyDeclarationSyntax.Identifier}",
                                        $"this.{propertyDeclarationSyntax.Identifier}.Update(other.{propertyDeclarationSyntax.Identifier})");
                                }
                                //else if (!isSimpleType)
                                //{
                                //    // The member is complex type not derived from entity, so we try to set its properties
                                //    var typeSymbolName = compilation.GetTypeByMetadataName(typeSymbol.ToString());
                                //    var members = typeSymbolName.GetMembers().OfType<IPropertySymbol>();
                                //}
                                else
                                {
                                    // The type is a simple type - we test for equality and overwrite, if necessary
                                    using (itw.StartScope($"if (this.{propertyDeclarationSyntax.Identifier} != other.{propertyDeclarationSyntax.Identifier})"))
                                    {
                                        itw.WriteLine($"this.{propertyDeclarationSyntax.Identifier} = other.{propertyDeclarationSyntax.Identifier};");
                                    }
                                }
                            }

                            itw.WriteLine("return this;");
                        }
                    }
                }
            }

            itw.Flush();
            stringWriter.Flush();

            sourceText = SourceText.From(stringWriter.ToString(), Encoding.UTF8);

            // Adds the generated source code to the compilation
            context.AddSource("Generated.cs", sourceText);
        }
    }
}
