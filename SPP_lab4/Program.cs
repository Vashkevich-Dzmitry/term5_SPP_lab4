namespace SPP_lab4
{
    static class Program
    {
        static async Task Main()
        {
            string inputDir = "C:\\Study\\Term 5\\SPP\\SPP_lab4\\Input";
            string outputDir = "C:\\Study\\Term 5\\SPP\\SPP_lab4\\Output";
            int maxConcurrentInput = 3;
            int maxConcurrentProcessing = 3;
            int maxConcurrentOutput = 3;


            if (Directory.Exists(inputDir) && Directory.Exists(outputDir))
            {
                var TestsGenerator = new TestsGenerator.TestsGeneratorPipeline(inputDir, outputDir, maxConcurrentInput, maxConcurrentProcessing, maxConcurrentOutput);
                await TestsGenerator.Run();
            }
        }
    }
}