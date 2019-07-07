using Xunit;
using Xunit.Abstractions;
using HealthCheck.Core.Extensions;

namespace HealthCheck.Core.Tests.Extensions
{
    public class StringExtensionsTests
    {
        public ITestOutputHelper Output { get; }

        public StringExtensionsTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void SpacifySentence_WithSingleWord_ShouldReturnCapitalizedWord()
        {
            var input = "word";
            var expected = "Word";
            
            var actual = input.SpacifySentence();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SpacifySentence_WithWordsWithNumbersAfterThem_ShouldAddSpaceAroundNumbers()
        {
            var input = "testThatTakes123Seconds";
            var expected = "Test That Takes 123 Seconds";

            var actual = input.SpacifySentence();
            Assert.Equal(expected, actual);
        }
    }
}
