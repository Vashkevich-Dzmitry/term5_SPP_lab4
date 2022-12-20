using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsGenerator
{
    public class ClassVisitor : CSharpSyntaxWalker
    {
        public List<ClassDeclarationSyntax> classes = new();

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            base.VisitClassDeclaration(node);

            classes.Add(node);
        }
    }
}
