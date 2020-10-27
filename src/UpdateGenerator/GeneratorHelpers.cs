using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace UpdateGenerator
{
    public static class GeneratorHelpers
    {
        public static IReadOnlyList<INamedTypeSymbol> GetAllTypes(IAssemblySymbol symbol)
        {
            var result = new List<INamedTypeSymbol>();
            GetAllTypes(result, symbol.GlobalNamespace);
            return result;
        }

        public static void GetAllTypes(ICollection<INamedTypeSymbol> result, INamespaceOrTypeSymbol symbol)
        {
            if (symbol is INamedTypeSymbol type)
            {
                result.Add(type);
            }

            foreach (var child in symbol.GetMembers())
            {
                if (child is INamespaceOrTypeSymbol nsChild)
                {
                    GetAllTypes(result, nsChild);
                }
            }
        }

        public static bool IsDerivedFrom(this ITypeSymbol type, INamedTypeSymbol baseType)
        {
            var orgType = type.Name;
            while (type != null)
            {
                if (SymbolEqualityComparer.Default.Equals(type, baseType))
                {
                    return true;
                }
                type = type.BaseType;
            }

            return false;
        }

        public static bool IsPartial(this INamedTypeSymbol type)
        {
            foreach (var declaration in type.DeclaringSyntaxReferences)
            {
                var syntax = declaration.GetSyntax();
                if (syntax is TypeDeclarationSyntax typeDeclaration)
                {
                    foreach (var modifier in typeDeclaration.Modifiers)
                    {
                        if (modifier.ValueText == "partial")
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    public struct ScopeWriter : IDisposable
    {
        public readonly IndentedTextWriter indentedTextWriter;

        public ScopeWriter(IndentedTextWriter indentedTextWriter, string openingLine = null)
        {
            this.indentedTextWriter = indentedTextWriter 
                ?? throw new ArgumentNullException(nameof(indentedTextWriter));

            if (!string.IsNullOrEmpty(openingLine))
            {
                this.indentedTextWriter.WriteLine(openingLine);
            }

            this.indentedTextWriter.WriteLine("{");
            this.indentedTextWriter.Indent++;
        }

        public void Dispose()
        {
            indentedTextWriter.Indent--;
            indentedTextWriter.WriteLine("}");
        }
    }

    public struct CommentWriter : IDisposable
    {
        public readonly IndentedTextWriter indentedTextWriter;

        public CommentWriter(IndentedTextWriter indentedTextWriter)
        {
            this.indentedTextWriter = indentedTextWriter
                ?? throw new ArgumentNullException(nameof(indentedTextWriter));

            this.indentedTextWriter.WriteLine("/*");
        }

        public void Dispose()
        {
            indentedTextWriter.WriteLine("*/");
        }
    }
}
