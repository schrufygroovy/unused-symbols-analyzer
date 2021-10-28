﻿using System.Threading.Tasks;
using NUnit.Framework;
using UnusedSymbolsAnalyzer.UseCases.Interactors.AnalyzeSolution;

namespace UnusedSymbolsAnalyzer.UseCases.Tests
{
    [TestFixture]
    public class AnalyzeSolutionInteractorTests
    {
        [Test]
        public void AnalyzeSolution_SolutionNull_Throws_ArgumentException()
        {
            var analyzeSolutionInteractor = new AnalyzeSolutionInteractor();

            Assert.That(
                () => analyzeSolutionInteractor.AnalyzeSolution(new AnalyzeSolutionArguments()),
                Throws.ArgumentException);
        }

        [Test]
        public async Task AnalyzeSolution_UsedPublicConstShouldNotBeReported()
        {
            var analyzeSolutionInteractor = new AnalyzeSolutionInteractor();

            var dependencySource = @"
namespace Dependency
{
    public class UsedClass
    {
        public const string Something = ""somevalue"";
    }
}";
            var source = @"
using Dependency;

namespace Hui
{
    public class IsDependingOnUsedClass
    {
        public static readonly IsUsingUsedClass = UsedClass.Something;
    }
}";
            var solution = await WorkspaceCreator.CreateDependingSourcesSolutionAsync(source, dependencySource);
            Assert.That(
                () => analyzeSolutionInteractor.AnalyzeSolution(new AnalyzeSolutionArguments { Solution = solution }),
                Throws.Nothing);
        }
    }
}
