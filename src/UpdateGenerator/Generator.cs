using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace UpdateGenerator
{
    [Generator]
    public class UpdateGenerator : ISourceGenerator
    {
        static readonly IComparer<ClassDeclarationSyntax> clsComparer = Comparer<ClassDeclarationSyntax>.Create((x, y) =>
        {
            return x.Identifier.ToString().CompareTo(y.Identifier.ToString());
        });

        public void Initialize(GeneratorInitializationContext context)
        {
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
            var baseType = compilation.GetTypeByMetadataName("Entities.Entity");

            SourceText sourceText;

            using var stringWriter = new StringWriter();
            using var itw = new IndentedTextWriter(stringWriter, "    ");

            var entityClasses = receiver.CandidateClasses
                .Where(cls =>
                {
                    var semanticModel = compilation.GetSemanticModel(cls.SyntaxTree);
                    var classSymbol = semanticModel.GetDeclaredSymbol(cls);
                    return classSymbol.IsDerivedFrom(baseType);
                })
                .OrderBy(cls => cls, clsComparer)
                .ToList();

            itw.WriteLine("using System;");
            itw.WriteLine();

            using (new ScopeWriter(itw, "namespace Entities"))
            {
                foreach (var cls in entityClasses)
                {
                    using (new ScopeWriter(itw, $"partial class {cls.Identifier.ValueText}"))
                    {
                        using (new ScopeWriter(itw, $"public override void Update({cls.Identifier.ValueText} other)"))
                        {
                            foreach (var member in cls.Members)
                            {
                                
                                //itw.WriteLine($"// {member.GetType()}");
                                var property = member as PropertyDeclarationSyntax;
                                itw.WriteLine($"// {property.Identifier}");
                                using (new ScopeWriter(itw, $"if (this.{property.Identifier} != other.{property.Identifier})"))
                                {
                                    itw.WriteLine($"this.{property.Identifier} = other.{property.Identifier};");
                                }
                            }
                        }
                    }
                }
            }

            itw.WriteLine();

            itw.Flush();
            stringWriter.Flush();

            sourceText = SourceText.From(stringWriter.ToString(), Encoding.UTF8);

            // Adds the generated source code to the compilation
            context.AddSource("Generated.cs", sourceText);

            // For debugging purposes, we write the generated code into a text file.
            var baseDir = @"C:\code\UpdateGenerator\src\Entities\";
            var fileName = Path.Combine(baseDir, "Generated.txt");
            using var writer = new StreamWriter(fileName);
            sourceText.Write(writer);

        }
    }
}
