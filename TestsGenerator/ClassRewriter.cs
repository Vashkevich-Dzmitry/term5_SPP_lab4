using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsGenerator
{
    public class ClassRewriter : CSharpSyntaxRewriter
    {
        private ConcurrentDictionary<string, int>? _methodNamesDictionary;
        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax methodNode)
        {
            if (methodNode.Modifiers.Where(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)).Any())
            {
                var methodIdentifier = methodNode.Identifier.Text;

                var newMethodIdentifier = "";
                if (_methodNamesDictionary![methodIdentifier] > 0)
                {
                    newMethodIdentifier = methodIdentifier + _methodNamesDictionary[methodIdentifier] + "Test";
                    _methodNamesDictionary[methodIdentifier]++;
                }
                else
                {
                    newMethodIdentifier = methodIdentifier + "Test";
                }

                var newMethodNode = MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.VoidKeyword)
                        ),
                    SyntaxFactory.Identifier(
                        newMethodIdentifier
                        )
                    )
                .WithAttributeLists(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.AttributeList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Attribute(
                                    SyntaxFactory.IdentifierName(
                                        "Test"))))))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement("Assert.Fail(\"autogenerated\");")));

                return base.VisitMethodDeclaration(newMethodNode);
            }
            else
            {
                return null;
            }

        }

        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax classNode)
        {
            var classIdentifier = classNode.Identifier.Text;

            var newClassNode = classNode.WithIdentifier(Identifier(classIdentifier + "Tests"))
                .WithAttributeLists(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.AttributeList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Attribute(
                                    SyntaxFactory.IdentifierName(
                                        "TestFixture"))))));

            var methodNames = classNode.ChildNodes()
                .Where(x => x.GetType() == typeof(MethodDeclarationSyntax) && ((MethodDeclarationSyntax)x).Modifiers
                .Where(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)).Any())
                .Select(x => ((MethodDeclarationSyntax)x).Identifier.ToString()).ToList();

            var methodNamesDictionary = methodNames.GroupBy(p => p).
                ToDictionary(p => p.Key, q => q.Count() == 1 ? 0 : 1);

            _methodNamesDictionary = new ConcurrentDictionary<string, int>(methodNamesDictionary);

            return base.VisitClassDeclaration(newClassNode);
        }
    }
}
