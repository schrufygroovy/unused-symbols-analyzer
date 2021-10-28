﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace UnusedSymbolsAnalyzer.UseCases.Tests
{
    public static class WorkspaceCreator
    {
        public static Task<Solution> CreateDependingSourcesSolutionAsync(
            string source,
            string dependencySource,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var test = new SolutionAnalyzerTest<NUnitVerifier>
            {
                TestState =
                {
                    Sources = { source },
                },
                SolutionTransforms =
                {
                    (solution, projectId) =>
                    {
                        var sideProject = solution.AddProject("DependencyProject", "DependencyProject", LanguageNames.CSharp)
                            .AddDocument("Dependency.cs", dependencySource).Project
                            .AddMetadataReferences(solution.GetProject(projectId).MetadataReferences)
                            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                        return sideProject.Solution.GetProject(projectId)
                            .AddProjectReference(new ProjectReference(sideProject.Id))
                            .Solution;
                    }
                }
            };

            return test.CreateSolutionAsync(cancellationToken);
        }
    }
}
