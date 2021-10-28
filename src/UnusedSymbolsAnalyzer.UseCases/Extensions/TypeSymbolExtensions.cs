using Microsoft.CodeAnalysis;

namespace UnusedSymbolsAnalyzer.UseCases.Extensions
{
    internal static class TypeSymbolExtensions
    {
        private static readonly SymbolDisplayFormat FullyQualifiedNameFormat = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        public static string GetFullyQualifiedName(this ITypeSymbol typeSymbol)
        {
            return typeSymbol.ToDisplayString(FullyQualifiedNameFormat);
        }
    }
}
