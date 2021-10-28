using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Model;

namespace UnusedSymbolsAnalyzer.UseCases.Tests
{
    public class SolutionAnalyzerTest<TVerifier> : AnalyzerTest<TVerifier>
        where TVerifier : IVerifier, new()
    {
        private static readonly LanguageVersion DefaultLanguageVersion =
            Enum.TryParse("Default", out LanguageVersion version) ? version : LanguageVersion.CSharp6;

        public override string Language => LanguageNames.CSharp;

        protected override string DefaultFileExt => "cs";

        public async Task<Solution> CreateSolutionAsync(CancellationToken cancellationToken)
        {
            var testState = this.TestState;
            var additionProjects = testState.AdditionalProjects.Values.Select(additionalProject => new EvaluatedProjectState(additionalProject, this.ReferenceAssemblies)).ToImmutableArray();
            var project = await this.CreateProjectAsync(
                new EvaluatedProjectState(testState, this.ReferenceAssemblies),
                additionProjects,
                cancellationToken);
            return project.Solution;
        }

        protected override CompilationOptions CreateCompilationOptions()
            => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);

        protected override ParseOptions CreateParseOptions()
            => new CSharpParseOptions(DefaultLanguageVersion, DocumentationMode.Diagnose);

        protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
            => new DiagnosticAnalyzer[] { new EmptyDiagnosticAnalyzer() };
    }
}
