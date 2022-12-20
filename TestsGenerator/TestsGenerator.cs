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

            var classVisitor = new ClassVisitor();
            var classRewriter = new ClassRewriter();

            classVisitor.Visit(root);

            foreach (var classNode in classVisitor.classes)
            {
                if (classNode.Modifiers.Where(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)).Any())
                {
                    var compilationUnit = CompilationUnit().WithUsings(root.Usings);

                    var classNamespace = GetNamespaceFrom(classNode);

                    if (classNamespace != null)
                    {
                        var newUsings = UsingDirective(ParseName(classNamespace));
                        compilationUnit = compilationUnit.AddUsings(newUsings);

                        var customNamespace = FileScopedNamespaceDeclaration(ParseName(classNamespace + ".Tests"));
                        compilationUnit = compilationUnit.AddMembers(customNamespace);
                    }
                    else
                    {
                        var customNamespace = FileScopedNamespaceDeclaration(ParseName("Tests"));
                        compilationUnit = compilationUnit.AddMembers(customNamespace);
                    }
                    var newClassNode = (MemberDeclarationSyntax)classRewriter.Visit(classNode);
                    compilationUnit = compilationUnit.AddMembers(newClassNode);
                    tests.Add(compilationUnit.NormalizeWhitespace().ToFullString());
                }
            }
            return tests;
        }

        private static string? GetNamespaceFrom(SyntaxNode syntaxNode)
        {
            var result = "";
            while (syntaxNode.Parent!.GetType() == typeof(NamespaceDeclarationSyntax) ||
                syntaxNode.Parent!.GetType() == typeof(FileScopedNamespaceDeclarationSyntax))
            {
                result = ((NamespaceDeclarationSyntax)syntaxNode.Parent).Name.ToString() + '.' + result;
                syntaxNode = syntaxNode.Parent;
            }
            if (result != "")
                return result.Remove(result.Length - 1, 1);
            else
                return null;
        }
    }
}
