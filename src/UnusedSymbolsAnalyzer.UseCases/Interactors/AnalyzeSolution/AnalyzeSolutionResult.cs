using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace UnusedSymbolsAnalyzer.UseCases.Interactors.AnalyzeSolution
{
    public class AnalyzeSolutionResult
    {
        public IList<INamedTypeSymbol> UnusedTypes { get; init; }

        public IList<IMethodSymbol> UnusedMethods { get; init; }
    }
}
