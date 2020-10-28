using System.CodeDom.Compiler;
using System.Diagnostics;

namespace UpdateGenerator
{
    public static class IndentedTextWriterExtensions
    {
        public static void OpenScope(this IndentedTextWriter @this)
        {
            @this.WriteLine("{");
            @this.Indent++;
        }

        public static void CloseScope(this IndentedTextWriter @this)
        {
            @this.Indent--;
            @this.WriteLine("}");
        }

        public static void OpenMultiLineComment(this IndentedTextWriter @this)
        {
            @this.WriteLine("/*");
        }

        public static void CloseMultiLineComment(this IndentedTextWriter @this)
        {
            @this.WriteLine("*/");
        }

        public static void WriteTernary(this IndentedTextWriter @this, string condition, string consequent, string alternative)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(condition));
            Debug.Assert(!string.IsNullOrWhiteSpace(consequent));
            Debug.Assert(!string.IsNullOrWhiteSpace(alternative));

            @this.WriteLine($"{condition}");
            @this.Indent++;
            @this.WriteLine($"? { consequent}");
            @this.WriteLine($": { alternative};");
            @this.Indent--;
        }

        public static void WriteTernaryAssignment(this IndentedTextWriter @this, string assignTo, string condition, string consequent, string alternative)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(assignTo));
            @this.Write($"{assignTo} = ");
            @this.WriteTernary(condition, consequent, alternative);
        }

        public static void WriteTernaryReturn(this IndentedTextWriter @this, string condition, string consequent, string alternative)
        {
            @this.Write("return ");
            @this.WriteTernary(condition, consequent, alternative);
        }

        public static Scope StartScope(this IndentedTextWriter @this, string openingLine = null)
        {
            return Scope.Start(@this, openingLine);
        }

        public static MultiLineComment StartMultiLineComment(this IndentedTextWriter @this, string openingLine = null)
        {
            return MultiLineComment.Start(@this);
        }
    }
}
