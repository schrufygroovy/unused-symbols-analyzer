using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace UnusedSymbolsAnalyzer.UseCases.Interactors.AnalyzeSolution
{
    internal class AnalyzeSolutionSymbolVisitorResult
    {
        public IList<INamedTypeSymbol> PotentialSymbols { get; init; }

        public IList<IMethodSymbol> PotentialMethods { get; init; }
    }
}
