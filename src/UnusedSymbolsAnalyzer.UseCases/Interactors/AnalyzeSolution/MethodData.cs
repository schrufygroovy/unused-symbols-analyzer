using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace UnusedSymbolsAnalyzer.UseCases.Interactors.AnalyzeSolution
{
    internal class MethodData
    {
        public IMethodSymbol MethodSymbol { get; init; }

        public IList<ReferenceLocation> ExternalReferenceLocations { get; init; }

        public bool IsExternallyReferenced()
        {
            return this.ExternalReferenceLocations.Count > 0;
        }
    }
}
