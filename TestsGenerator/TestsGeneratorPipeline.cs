using System.Threading.Tasks.Dataflow;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsGenerator
{
    public class TestsGeneratorPipeline
    {
        private readonly string _inputDir;
        private readonly string _outputDir;
        private readonly int _maxConcurrentInput;
        private readonly int _maxConcurrentProcessing;
        private readonly int _maxConcurrentOutput;

        public TestsGeneratorPipeline(string inputDir, string outputDir, int maxConcurrentInput, int maxConcurrentProcessing, int maxConcurrentOutput)
        {
            _inputDir = inputDir;
            _outputDir = outputDir;
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

            foreach (var file in Directory.EnumerateFiles(_inputDir))
            {
                bufferBlock.Post(file);
            }

            bufferBlock.Complete();

            return writingBlock.Completion;
        }

        private async Task<string> GetSourceCode(string filePath)
        {
            return null;
        }

        private async Task<List<string>> GenerateTests(string code)
        {
            return null;
        }

        private async Task WriteTests(string tests)
        {
            return;
        }
    }
}
