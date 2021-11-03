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
        public void Pluralize_WithoutCount_ReturnsInput()
        {
            var input = "thing";
            var output = input.Pluralize();
            Assert.Equal(input, output);
        }

        [Fact]
        public void Pluralize_WithPositiveCount_ReturnsPluralized()
        {
            var input = "2 thing";
            var expected = "2 things";
            var output = input.Pluralize();
            Assert.Equal(expected, output);
        }

        [Fact]
        public void Pluralize_WithNegativeCount_ReturnsPluralized()
        {
            var input = "-2 thing";
            var expected = "-2 things";
            var output = input.Pluralize();
            Assert.Equal(expected, output);
        }

        [Fact]
        public void Pluralize_WithSingleCount_ReturnsNonPluralized()
        {
            var input = "1 thing";
            var expected = "1 thing";
            var output = input.Pluralize();
            Assert.Equal(expected, output);
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
        public void SpacifySentence_WithMultipleUppercase_ShouldReturnKeepThemUppercase()
        {
            var input = "SomeValueXYZHere";
            var expected = "Some Value XYZ Here";

            var actual = input.SpacifySentence();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SpacifySentence_WithMultipleUppercaseSeparatedV1_ShouldReturnKeepThemUppercase()
        {
            var input = "SomeValueXYZ_ABCHere";
            var expected = "Some Value XYZ_ABC Here";

            var actual = input.SpacifySentence();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SpacifySentence_WithMultipleUppercaseSeparatedV2_ShouldReturnKeepThemUppercase()
        {
            var input = "SomeValueXYZ_ABC-GGGHere";
            var expected = "Some Value XYZ_ABC-GGG Here";

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

        [Theory]
        [InlineData("127.0.0.1:8080", "127.0.0.1")]
        [InlineData("[::1]:8001", "::1")]
        [InlineData("[1fff:0:a88:85a3::ac1f]:8001", "1fff:0:a88:85a3::ac1f")]
        public void StripPortNumber_WithPortNumber_ShouldStripPortNumber(string ip, string expected)
        {
            var actual = ip.StripPortNumber();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("127.0.0.1")]
        [InlineData("::1")]
        [InlineData("2001:db8:3333:4444:5555:6666:7777:8888")]
        [InlineData("1fff:0:a88:85a3::ac1f")]
        public void StripPortNumber_WithoutPortNumber_ShouldReturnSame(string ip)
        {
            var actual = ip.StripPortNumber();
            Assert.Equal(ip, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void StripPortNumber_ShouldReturnSameForNullOrWhitespace(string ip)
        {
            var actual = ip.StripPortNumber();
            Assert.Equal(ip, actual);
        }
    }
}
