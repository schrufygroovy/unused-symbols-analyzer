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
    }
}
