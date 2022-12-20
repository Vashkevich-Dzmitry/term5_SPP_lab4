using NUnit.Framework;
using TestsGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Linq;

namespace Tests
{
    public class Tests
    {
        readonly string inputPath = "C:\\Study\\Term 5\\SPP\\SPP_lab4\\Input";
        readonly string outputPath = "C:\\Study\\Term 5\\SPP\\SPP_lab4\\Output";
        readonly int maxConcurrentInput = 3;
        readonly int maxConcurrentProcessing = 3;
        readonly int maxConcurrentOutput = 3;

        [SetUp]
        public void Setup()
        {
            var TestsGenerator = new TestsGenerator.TestsGeneratorPipeline(inputPath, outputPath, maxConcurrentInput, maxConcurrentProcessing, maxConcurrentOutput);
            TestsGenerator.Run().Wait();
        }

        [Test]
        public void InitialClassAmountEqualsToOutputFilesAmount()
        {
            Assert.AreEqual(Directory.GetFiles(outputPath, "*.cs").ToList().Count, 3);
        }

        [Test]
        public void FileWithFourMethodsProcessedCorrectly()
        {
            string sourceCode;
            var outputFiles = Directory.GetFiles(outputPath);
            using (var streamReader = new StreamReader(Directory.GetFiles(outputPath).ToList()
                .Where(file => file == "C:\\Study\\Term 5\\SPP\\SPP_lab4\\Output\\MyClassTests.cs").First()))
            {
                sourceCode = streamReader.ReadToEnd();
            }

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            var root = syntaxTree.GetCompilationUnitRoot();

            var usings = root.Usings.Select(x => x.Name.ToString()).ToList();

            Assert.Contains("System", usings);
            Assert.Contains("MyCode", usings);
            Assert.Contains("NUnit.Framework", usings);

            var namespases = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Select(x => x.Name.ToString()).ToList();

            Assert.True(namespases.Count == 1);
            Assert.Contains("MyCode.Tests", namespases);

            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();

            Assert.True(classes.Count == 1);
            Assert.AreEqual(classes.First().Identifier.Text, "MyClassTests");

            var methods = classes.First().ChildNodes().
                    Where(x => x.GetType() == typeof(MethodDeclarationSyntax)).
                    Select(x => ((MethodDeclarationSyntax)x).Identifier.ToString()).ToList();

            Assert.AreEqual(methods.Count, 4);
            Assert.Contains("FirstMethodTest", methods);
            Assert.Contains("SecondMethodTest", methods);
            Assert.Contains("ThirdMethod1Test", methods);
            Assert.Contains("ThirdMethod2Test", methods);
        }
    }
}