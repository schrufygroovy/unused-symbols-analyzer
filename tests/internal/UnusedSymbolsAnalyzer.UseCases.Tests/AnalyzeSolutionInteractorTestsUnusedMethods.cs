using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using UnusedSymbolsAnalyzer.UseCases.Interactors.AnalyzeSolution;

namespace UnusedSymbolsAnalyzer.UseCases.Tests
{
    [TestFixture]
    public class AnalyzeSolutionInteractorTestsUnusedMethods
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private CancellationToken CancellationToken => this.cancellationTokenSource.Token;

        [Test]
        public async Task AnalyzeSolution_ShouldNotReportDefaultPublicConstructor()
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
            Assert.That(result.UnusedMethods, Is.Null.Or.Empty);
        }

        [Test]
        public async Task AnalyzeSolution_PartialClass_PublicMethodOnlyUsedInSameClass_ShouldBeReported()
        {
            var analyzeSolutionInteractor = new AnalyzeSolutionInteractor();

            var source1 = @"
namespace Dependency
{
    public partial static class PartialClass
    {
        public static void OnlyInternallyUsed()
        {
        }
    }
}";
            var source2 = @"
namespace Dependency
{
    public partial static class PartialClass
    {
        private static void SomeMethod()
        {
            OnlyInternallyUsed();
        }
    }
}";
            var sourceFileList = new SourceFileList(string.Empty, "cs")
            {
                ("PartialClass.Public.cs", source1),
                ("PartialClass.Private.cs", source2)
            };

            var solution = await WorkspaceCreator.CreateSimpleSolutionAsync(
                sourceFileList,
                this.CancellationToken);
            var result = await analyzeSolutionInteractor.AnalyzeSolution(new AnalyzeSolutionArguments { Solution = solution }, this.CancellationToken);
            Assert.That(result.UnusedTypes, Has.Count.EqualTo(1));
            AssertUnusedMethods(result, new[] { "Dependency.PartialClass.OnlyInternallyUsed()" });
        }

        [Test]
        public async Task AnalyzeSolution_IgnoreOverriddenMethods_True_Override_ShouldBeIgnored()
        {
            var analyzeSolutionInteractor = new AnalyzeSolutionInteractor();

            var solution = await this.PrepareSolutionWithOverrideMethod();

            var result = await analyzeSolutionInteractor.AnalyzeSolution(new AnalyzeSolutionArguments { Solution = solution, IgnoreOverriddenMethods = true }, this.CancellationToken);

            Assert.That(result.UnusedTypes, Has.Count.EqualTo(1));
            Assert.That(result.UnusedMethods, Is.Null.Or.Empty);
        }

        [Test]
        public async Task AnalyzeSolution_IgnoreOverriddenMethods_False_Override_ShouldBeReported()
        {
            var analyzeSolutionInteractor = new AnalyzeSolutionInteractor();

            var solution = await this.PrepareSolutionWithOverrideMethod();

            var result = await analyzeSolutionInteractor.AnalyzeSolution(new AnalyzeSolutionArguments { Solution = solution, IgnoreOverriddenMethods = false }, this.CancellationToken);

            Assert.That(result.UnusedTypes, Has.Count.EqualTo(1));
            AssertUnusedMethods(result, new[] { "Dependency.ClassWithOverride.VisitNamespace(INamespaceSymbol)" });
        }

        private static void AssertUnusedMethods(
            AnalyzeSolutionResult result,
            string[] expectedMethods)
        {
            Assert.That(
                result.UnusedMethods.Select(m => m.ToString()),
                Is.EqualTo(expectedMethods));
        }

        private Task<Solution> PrepareSolutionWithOverrideMethod()
        {
            var source1 = @"
using Microsoft.CodeAnalysis;

namespace Dependency
{
    public class ClassWithOverride : SymbolVisitor
    {
        public override void VisitNamespace(INamespaceSymbol symbol)
        {
        }
    }
}";
            var sourceFileList = new SourceFileList(string.Empty, "cs")
            {
                source1
            };

            return WorkspaceCreator.CreateSimpleSolutionAsync(
                sourceFileList,
                ReferenceAssemblies.Default
                    .AddPackages(ImmutableArray.Create(new PackageIdentity("Microsoft.CodeAnalysis.Analyzers", "3.3.3"))),
                this.CancellationToken);
        }
    }
}
