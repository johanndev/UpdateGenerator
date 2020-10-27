using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            //Debugger.Launch();
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

            //var entityClasses = receiver.CandidateClasses
            //    .Where(cls =>
            //    {
            //        var semanticModel = compilation.GetSemanticModel(cls.SyntaxTree);
            //        var classSymbol = semanticModel.GetDeclaredSymbol(cls);
            //        itw.WriteLine($"// {classSymbol}");
            //        return classSymbol.IsDerivedFrom(baseType);
            //    })
            //    .OrderBy(cls => cls, clsComparer)
            //    .ToList();

            var complex = receiver.CandidateClasses
                .Select(candidateClass =>
                {
                    var semanticModel = compilation.GetSemanticModel(candidateClass.SyntaxTree);
                    var classSymbol = semanticModel.GetDeclaredSymbol(candidateClass);
                    var isDerived = classSymbol.IsDerivedFrom(baseType);

                    return isDerived
                        ? new { candidateClass, semanticModel, classSymbol, isDerived }
                        : null;
                })
                .Where(complexCls => complexCls != null)
                .OrderBy(complexCls => complexCls.candidateClass, clsComparer)
                .ToList();

            itw.WriteLine("using System;");
            itw.WriteLine();

            using (itw.StartScope("namespace Entities"))
            {
                foreach (var cls in complex)
                {
                    using (itw.StartScope($"partial class {cls.candidateClass.Identifier.ValueText}"))
                    {
                        using (itw.StartScope($"public override void Update({cls.candidateClass.Identifier.ValueText} other)"))
                        {
                            //var classes = string.Join(", ", complex.Select(e => e.candidateClass.Identifier));
                            //using (MultiLineComment.Start(itw))
                            //{
                            //    itw.WriteLine($"{classes}");
                            //}
                            //itw.WriteLine($"// {cls.classSymbol}");

                            foreach (var member in cls.candidateClass.Members)
                            {
                                var property = member as PropertyDeclarationSyntax;
                                var propModel = compilation.GetSemanticModel(member.SyntaxTree);
                                var propDeclared = propModel.GetDeclaredSymbol(member) as IPropertySymbol;

                                var isEntity = GeneratorHelpers.IsDerivedFrom(propDeclared.Type, baseType);

                                if (isEntity)
                                {
                                    itw.WriteLine($"this.{property.Identifier}.Update({property.Identifier} other);");

                                }
                                else
                                {
                                    using (itw.StartScope($"if (this.{property.Identifier} != other.{property.Identifier})"))
                                    {
                                        itw.WriteLine($"this.{property.Identifier} = other.{property.Identifier};");
                                    }
                                }

                            }
                        }
                    }
                }
                //var outputDirTemp = @"C:foo code UpdateGenerator src Entities";
                //itw.WriteLine($"// outputDir: {outputDirTemp}");
            }

            itw.Flush();
            stringWriter.Flush();

            sourceText = SourceText.From(stringWriter.ToString(), Encoding.UTF8);

            // Adds the generated source code to the compilation
            context.AddSource("Generated.cs", sourceText);

            var fileName = "Generated.txt";
            var entityPath = baseType.DeclaringSyntaxReferences.First().SyntaxTree.FilePath;
            var outputDirectory = Path.GetDirectoryName(entityPath);
            var filePath = Path.Combine(outputDirectory, fileName);
            try
            {
                if (File.Exists(filePath))
                {
                    var fileText = File.ReadAllText(filePath);
                    var sourceFileText = SourceText.From(fileText, Encoding.UTF8);
                    if (sourceText.ContentEquals(sourceFileText))
                        return;
                }

                // For debugging purposes, we write the generated code into a text file.
                using var writer = new StreamWriter(fileName);
                sourceText.Write(writer);
            }
            catch (Exception ex)
            {
                itw.Flush();
                stringWriter.Flush();
                itw.WriteLine(ex.Message);
                itw.WriteLine(ex.StackTrace);

                var errorSourceText = SourceText.From(stringWriter.ToString(), Encoding.UTF8);
                var errorFilePath = Path.Combine(outputDirectory, "error.txt");

                using var writer = new StreamWriter(errorFilePath);
                errorSourceText.Write(writer);

                throw;
            }
        }
    }
}
