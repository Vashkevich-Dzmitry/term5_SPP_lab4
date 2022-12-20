using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsGenerator
{
    public static class TestsGenerator
    {
        public static List<string> Generate(string code)
        {
            var tests = new List<string>();
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            var usings = root.Usings.Select(x => x.Name.ToString()).ToList();

           

            return tests;
        }
    }
}
