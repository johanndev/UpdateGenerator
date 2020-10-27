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

        public static void StartMultiLineComment(this IndentedTextWriter @this)
        {
            @this.WriteLine("/*");
        }

        public static void EndMultiLineComment(this IndentedTextWriter @this)
        {
            @this.WriteLine("*/");
        }
    }
}
