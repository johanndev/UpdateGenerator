using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace UpdateGenerator
{
    public static class GeneratorHelpers
    {
        static readonly ConcurrentDictionary<Type, bool> IsSimpleTypeCache = new ConcurrentDictionary<Type, bool>();
        public static bool IsSimpleType(Type type)
        {
            return IsSimpleTypeCache.GetOrAdd(type, t =>
                type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) ||
                type == typeof(Guid) ||
                IsNullableSimpleType(type));

            static bool IsNullableSimpleType(Type t)
            {
                var underlyingType = Nullable.GetUnderlyingType(t);
                return underlyingType != null && IsSimpleType(underlyingType);
            }
        }

        static readonly ConcurrentDictionary<ITypeSymbol, bool> IsSimpleTypeSymbolCache = new ConcurrentDictionary<ITypeSymbol, bool>();
        public static bool IsSimpleType(ITypeSymbol typeSymbol)
        {
            return IsSimpleTypeSymbolCache.GetOrAdd(typeSymbol, t =>
                t.IsValueType
            );
        }

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

    public class Scope : IDisposable
    {
        public readonly IndentedTextWriter indentedTextWriter;

        public static Scope Start(IndentedTextWriter indentedTextWriter, string openingLine = null)
        {
            return new Scope(indentedTextWriter, openingLine);
        }

        private Scope(IndentedTextWriter indentedTextWriter, string openingLine = null)
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

    public class MultiLineComment : IDisposable
    {
        public readonly IndentedTextWriter indentedTextWriter;

        public static MultiLineComment Start(IndentedTextWriter indentedTextWriter)
        {
            return new MultiLineComment(indentedTextWriter);
        }

        private MultiLineComment(IndentedTextWriter indentedTextWriter)
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
