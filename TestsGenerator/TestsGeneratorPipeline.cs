using System.Threading.Tasks.Dataflow;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsGenerator
{
    public class TestsGeneratorPipeline
    {
        private readonly string _inputPath;
        private readonly string _outputPath;
        private readonly int _maxConcurrentInput;
        private readonly int _maxConcurrentProcessing;
        private readonly int _maxConcurrentOutput;

        public TestsGeneratorPipeline(string inputPath, string outputPath, int maxConcurrentInput, int maxConcurrentProcessing, int maxConcurrentOutput)
        {
            _inputPath = inputPath;
            _outputPath = outputPath;
            _maxConcurrentInput = maxConcurrentInput;
            _maxConcurrentProcessing = maxConcurrentProcessing;
            _maxConcurrentOutput = maxConcurrentOutput;
        }

        public Task Run()
        {
            var bufferBlock = new BufferBlock<string>();

            var readingBlock = new TransformBlock<string, string>(
                async path => await GetSourceCode(path),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxConcurrentInput });

            var processingBlock = new TransformManyBlock<string, string>(
                async code => await GenerateTests(code),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxConcurrentProcessing });

            var writingBlock = new ActionBlock<string>(
                async tests => await WriteTests(tests),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxConcurrentOutput });

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            bufferBlock.LinkTo(readingBlock, linkOptions);
            readingBlock.LinkTo(processingBlock, linkOptions);
            processingBlock.LinkTo(writingBlock, linkOptions);

            foreach (var file in Directory.EnumerateFiles(_inputPath))
            {
                bufferBlock.Post(file);
            }

            bufferBlock.Complete();

            return writingBlock.Completion;
        }

        private async Task<string> GetSourceCode(string filePath)
        {
            string sourceCode;
            using (var streamReader = new StreamReader(filePath))
            {
                sourceCode = await streamReader.ReadToEndAsync();
            }
            return sourceCode;
        }

        private Task<List<string>> GenerateTests(string code)
        {
            return Task.FromResult(TestsGenerator.Generate(code));
        }

        private async Task WriteTests(string tests)
        {
            var fileName = CSharpSyntaxTree.ParseText(tests).GetRoot()
                .DescendantNodes().OfType<ClassDeclarationSyntax>().First().Identifier.Text;

            using var streamWriter = new StreamWriter($"{_outputPath}\\{fileName}.cs");

            await streamWriter.WriteAsync(tests);
        }
    }
}
