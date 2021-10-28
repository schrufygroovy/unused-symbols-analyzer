using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace UnusedSymbolsAnalyzer.UseCases.Interactors.AnalyzeSolution
{
    public class AnalyzeSolutionInteractor
    {
        public async Task<AnalyzeSolutionResult> AnalyzeSolution(
            AnalyzeSolutionArguments arguments,
            CancellationToken cancellationToken)
        {
            var solution = arguments.Solution;
            var allPublicTypes = new List<INamedTypeSymbol>();

            var compilationTasks = solution.Projects.Select(project => project.GetCompilationAsync(cancellationToken));

            var compilations = await Task.WhenAll(compilationTasks);

            foreach (var compilation in compilations)
            {
                allPublicTypes.AddRange(GetPublicTypes(compilation.Assembly));
            }

            return new AnalyzeSolutionResult
            {
                UnusedTypes = allPublicTypes
            };
        }

        private static IEnumerable<INamedTypeSymbol> GetPublicTypes(IAssemblySymbol assembly)
        {
            var visitor = new GetAllSymbolsVisitor();
            visitor.Visit(assembly.GlobalNamespace);
            return visitor.Symbols;
        }

        private class GetAllSymbolsVisitor : SymbolVisitor
        {
            public BlockingCollection<INamedTypeSymbol> Symbols { get; } = new BlockingCollection<INamedTypeSymbol>();

            public override void VisitNamespace(INamespaceSymbol symbol)
            {
                Parallel.ForEach(symbol.GetMembers(), s => s.Accept(this));
            }

            public override void VisitNamedType(INamedTypeSymbol symbol)
            {
                if (symbol.DeclaredAccessibility == Accessibility.Public)
                {
                    this.Symbols.Add(symbol);
                }
            }
        }
    }
}
