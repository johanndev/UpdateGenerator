using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpdateGenerator
{
    [Generator]
    public class UpdateGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var sourceText = SourceText.From("[assembly: System.Reflection.AssemblyMetadata(\"Setup\", \"is working\")]", Encoding.UTF8);
            context.AddSource("Generated.cs", sourceText);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
