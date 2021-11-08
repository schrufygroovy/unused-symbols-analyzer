using Microsoft.CodeAnalysis;

namespace UnusedSymbolsAnalyzer.UseCases.Interactors.AnalyzeSolution
{
    public class AnalyzeSolutionArguments
    {
        public Solution Solution { get; init; }

        public bool IgnoreOverriddenMethods { get; init; }
    }
}
