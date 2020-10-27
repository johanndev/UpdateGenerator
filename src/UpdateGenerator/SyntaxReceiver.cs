using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace UpdateGenerator
{
    /// <summary>
    /// Created on demand before each generation pass
    /// </summary>
    class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        /// <summary>
        /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
        /// </summary>
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax &&
                !classDeclarationSyntax.IsAbstract() &&
                classDeclarationSyntax.IsPartial())
            {
                CandidateClasses.Add(classDeclarationSyntax);
            }
        }
    }

    public static class TypeDeclarationSyntaxExtensions
    {
        public static bool IsAbstract(this TypeDeclarationSyntax @this)
        {
            return @this.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        }

        public static bool IsPartial(this TypeDeclarationSyntax @this)
        {
            return @this.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        }
    }
}