using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

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

            var unusedPublicTypes = new List<INamedTypeSymbol>();
            foreach (var type in allPublicTypes)
            {
                var references = await SymbolFinder.FindReferencesAsync(type, solution, cancellationToken);
                var locations = references.SelectMany(reference => reference.Locations).ToList();
                if (!locations.Any())
                {
                    unusedPublicTypes.Add(type);
                }
            }

            return new AnalyzeSolutionResult
            {
                UnusedTypes = unusedPublicTypes
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
                if (IsRelevantType(symbol))
                {
                    this.Symbols.Add(symbol);
                }
            }

            private static bool IsRelevantType(INamedTypeSymbol symbol)
            {
                return IsPublicOrInternal(symbol)
                    && !HasForbiddenAttribute(symbol);
            }

            private static bool HasForbiddenAttribute(INamedTypeSymbol symbol)
            {
                return symbol.GetAttributes().Any(
                    attributeData => IsTextFixture(attributeData) || IsCompilerGenerated(attributeData));
            }

            private static bool IsTextFixture(AttributeData attributeData)
            {
                return attributeData.AttributeClass.Name.Equals("TestFixtureAttribute");
            }

            private static bool IsCompilerGenerated(AttributeData attributeData)
            {
                return attributeData.AttributeClass.Name.Equals("CompilerGeneratedAttribute");
            }

            private static bool IsPublicOrInternal(INamedTypeSymbol symbol)
            {
                return symbol.DeclaredAccessibility is
                    Accessibility.Public or Accessibility.Internal;
            }
        }
    }
}
