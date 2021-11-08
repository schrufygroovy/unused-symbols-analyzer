using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        private static void AssertUnusedMethods(
            AnalyzeSolutionResult result,
            string[] expectedMethods)
        {
            Assert.That(
                result.UnusedMethods.Select(m => m.ToString()),
                Is.EqualTo(expectedMethods));
        }
    }
}
