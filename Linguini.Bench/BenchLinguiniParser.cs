using System.IO;
using BenchmarkDotNet.Attributes;
using Linguini.Syntax.Parser;

namespace Linguini.Bench
{
    public class BenchLinguiniParser
    {
        [Params(1, 25, 50)] public int N;
        private string _largeFtl = "";

        [GlobalSetup]
        public void Setup()
        {
            var baseFolder = Path.Combine(BenchRunner.BaseDir, "bench_ftl");
            var filePath = Path.GetFullPath(Path.Combine(baseFolder, "large.ftl"));
            _largeFtl = File.ReadAllText(filePath);
        }

        [Benchmark]
        public void BenchmarkParser()
        {
            LinguiniParser parser;
            for (var i = 0; i < N; i++)
            {
                parser = LinguiniParser.FromFragment(_largeFtl, "bench_ftl/large.ftl");
                parser.Parse();
            }
        }
    }
}