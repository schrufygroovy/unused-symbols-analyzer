using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using UnusedSymbolsAnalyzer.UseCases.Interactors.AnalyzeSolution;

namespace UnusedSymbolsAnalyzer.UseCases.Tests
{
    [TestFixture]
    public class AnalyzeSolutionInteractorTests
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private CancellationToken CancellationToken => this.cancellationTokenSource.Token;

        [Test]
        public void AnalyzeSolution_SolutionNull_Throws_ArgumentException()
        {
            var analyzeSolutionInteractor = new AnalyzeSolutionInteractor();
            Assert.That(
                () => analyzeSolutionInteractor.AnalyzeSolution(new AnalyzeSolutionArguments(), this.CancellationToken),
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
    private class IsDependingOnUsedClass
    {
        public static readonly IsUsingUsedClass = UsedClass.Something;
    }
}";
            var solution = await WorkspaceCreator.CreateDependingSourcesSolutionAsync(source, dependencySource, this.CancellationToken);
            var result = await analyzeSolutionInteractor.AnalyzeSolution(new AnalyzeSolutionArguments { Solution = solution }, this.CancellationToken);
            Assert.That(result.UnusedTypes, Is.Null.Or.Empty);
        }

        [Test]
        public async Task AnalyzeSolution_UnusedPublicClassShouldBeReported()
        {
            var analyzeSolutionInteractor = new AnalyzeSolutionInteractor();

            var source = @"
namespace Dependency
{
    public class UnusedClass
    {
    }
}";
            var solution = await WorkspaceCreator.CreateOneFileSolutionAsync(source, this.CancellationToken);
            var result = await analyzeSolutionInteractor.AnalyzeSolution(new AnalyzeSolutionArguments { Solution = solution }, this.CancellationToken);
            Assert.That(result.UnusedTypes, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task AnalyzeSolution_TestFixturePublicClassShouldNotBeReported()
        {
            var analyzeSolutionInteractor = new AnalyzeSolutionInteractor();

            var source = @"
using NUnit.Framework;

namespace Dependency
{
    [TestFixture]
    public class UnusedClass
    {
    }
}";
            var solution = await WorkspaceCreator.CreateOneFileSolutionAsync(
                source,
                ReferenceAssemblies.Default
                    .AddPackages(ImmutableArray.Create(new PackageIdentity("nunit", "3.13.1"))),
                this.CancellationToken);
            var result = await analyzeSolutionInteractor.AnalyzeSolution(new AnalyzeSolutionArguments { Solution = solution }, this.CancellationToken);
            Assert.That(result.UnusedTypes, Is.Null.Or.Empty);
        }
    }
}
