﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using UnusedSymbolsAnalyzer.UseCases.Extensions;

namespace UnusedSymbolsAnalyzer.UseCases.Interactors.AnalyzeSolution
{
    internal class AnalyzeSolutionSymbolVisitor : SymbolVisitor
    {
        public AnalyzeSolutionSymbolVisitor(
            HashSet<string> skippedNamespaces,
            HashSet<string> skippedAttributes)
            : base()
        {
            this.SkippedNamespaces = skippedNamespaces;
            this.SkippedAttributes = skippedAttributes;
        }

        private HashSet<string> SkippedNamespaces { get; }

        private HashSet<string> SkippedAttributes { get; }

        private BlockingCollection<INamedTypeSymbol> PotentialSymbols { get; } = new BlockingCollection<INamedTypeSymbol>();

        private BlockingCollection<IMethodSymbol> PotentialMethods { get; } = new BlockingCollection<IMethodSymbol>();

        public AnalyzeSolutionSymbolVisitorResult GetResult()
        {
            return new AnalyzeSolutionSymbolVisitorResult
            {
                PotentialTypes = this.PotentialSymbols.ToList(),
                PotentialMethods = this.PotentialMethods.ToList(),
            };
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            if (!this.IsSkippedNamespace(symbol))
            {
                Parallel.ForEach(symbol.GetMembers(), s => s.Accept(this));
            }
        }

        public override void VisitNamedType(INamedTypeSymbol namedTypeSymbol)
        {
            if (this.IsRelevantType(namedTypeSymbol))
            {
                Parallel.ForEach(namedTypeSymbol.GetMembers(), s => s.Accept(this));
                this.PotentialSymbols.Add(namedTypeSymbol);
            }
        }

        public override void VisitMethod(IMethodSymbol methodSymbol)
        {
            if (this.IsRelevantMethod(methodSymbol))
            {
                this.PotentialMethods.Add(methodSymbol);
            }
        }

        private static bool IsPublicOrInternal(ISymbol symbol)
        {
            return symbol.DeclaredAccessibility is
                Accessibility.Public or Accessibility.Internal;
        }

        private bool IsRelevantMethod(IMethodSymbol methodSymbol)
        {
            return IsPublicOrInternal(methodSymbol);
        }

        private bool IsSkippedNamespace(INamespaceSymbol namespaceSymbol)
        {
            return this.SkippedNamespaces.Contains(namespaceSymbol.ToDisplayString());
        }

        private bool IsRelevantType(INamedTypeSymbol symbol)
        {
            return IsPublicOrInternal(symbol)
                && !this.HasSkippedAttribute(symbol);
        }

        private bool HasSkippedAttribute(INamedTypeSymbol symbol)
        {
            var attributes = symbol.GetAttributes();

            return attributes.Any(
                attributeData =>
                    this.SkippedAttributes.Contains(attributeData.AttributeClass.GetFullyQualifiedName()));
        }
    }
}
