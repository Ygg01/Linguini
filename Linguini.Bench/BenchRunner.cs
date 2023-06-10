using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Linguini.Bench
{
    class BenchRunner
    {
        static void Main(string[] args)
        { 
            BenchmarkSwitcher.FromAssembly(typeof(BenchLinguiniParser).Assembly).Run(args, new DebugInProcessConfig());
        }
    }
}