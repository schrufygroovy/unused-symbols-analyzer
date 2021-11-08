using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace UnusedSymbolsAnalyzer.UseCases.Interactors.AnalyzeSolution
{
    internal class AnalyzeSolutionSymbolVisitorResult
    {
        public IList<INamedTypeSymbol> PotentialTypes { get; init; }

        public IList<IMethodSymbol> PotentialMethods { get; init; }
    }
}
