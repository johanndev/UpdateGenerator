using System.CodeDom.Compiler;

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
