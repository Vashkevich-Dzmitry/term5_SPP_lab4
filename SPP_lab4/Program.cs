namespace SPP_lab4
{
    static class Program
    {
        static async Task Main()
        {
            string inputPath = "C:\\Study\\Term 5\\SPP\\SPP_lab4\\Input";
            string outputPath = "C:\\Study\\Term 5\\SPP\\SPP_lab4\\Output";
            int maxConcurrentInput = 3;
            int maxConcurrentProcessing = 3;
            int maxConcurrentOutput = 3;


            if (Directory.Exists(inputPath) && Directory.Exists(outputPath))
            {
                var TestsGenerator = new TestsGenerator.TestsGeneratorPipeline(inputPath, outputPath, maxConcurrentInput, maxConcurrentProcessing, maxConcurrentOutput);
                await TestsGenerator.Run();
            }
        }
    }
}