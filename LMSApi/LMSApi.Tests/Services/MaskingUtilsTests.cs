using NUnit.Framework;
using LMSApi.BALLibrary.Utils;

namespace LMSApi.Tests.Services
{
    [TestFixture]
    public class MaskingUtilsTests
    {
        [Test]
        [TestCase("john.doe@example.com", "j******e@example.com")]
        [TestCase("ab@example.com", "a*@example.com")]
        [TestCase("a@example.com", "a*@example.com")]
        [TestCase("abc@example.com", "a*c@example.com")]
        [TestCase("student@domain.co.uk", "s*****t@domain.co.uk")]
        [TestCase("", "")]
        [TestCase("invalid-email", "invalid-email")]
        public void MaskEmail_ShouldCorrectlyMaskEmail(string input, string expected)
        {
            var result = MaskingUtils.MaskEmail(input);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void MaskEmail_WithNullInput_ShouldReturnEmptyString()
        {
            var result = MaskingUtils.MaskEmail(null!);
            Assert.That(result, Is.EqualTo(string.Empty));
        }
    }
}
