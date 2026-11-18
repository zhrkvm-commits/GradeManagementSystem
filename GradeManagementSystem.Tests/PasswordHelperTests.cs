using NUnit.Framework;
using GradeManagementSystem.Helpers;

namespace GradeManagementSystem.Tests
{
    [TestFixture]
    public class PasswordHelperTests
    {
        [Test]
        public void HashPassword_ReturnsStringOf64Characters()
        {
            string hash = PasswordHelper.HashPassword("1234");
            Assert.AreEqual(64, hash.Length);
        }

        [Test]
        public void HashPassword_SameInput_ReturnsSameHash()
        {
            string first = PasswordHelper.HashPassword("1234");
            string second = PasswordHelper.HashPassword("1234");
            Assert.AreEqual(first, second);
        }

        [Test]
        public void HashPassword_DifferentInput_ReturnsDifferentHash()
        {
            string first = PasswordHelper.HashPassword("1234");
            string second = PasswordHelper.HashPassword("4321");
            Assert.AreNotEqual(first, second);
        }

        [Test]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            string storedHash = PasswordHelper.HashPassword("1234");
            Assert.IsTrue(PasswordHelper.VerifyPassword("1234", storedHash));
        }

        [Test]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            string storedHash = PasswordHelper.HashPassword("1234");
            Assert.IsFalse(PasswordHelper.VerifyPassword("0000", storedHash));
        }

        [Test]
        public void HashPassword_KnownPassword_MatchesExpectedHash()
        {
            string expected = "03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4";
            Assert.AreEqual(expected, PasswordHelper.HashPassword("1234"));
        }
    }
}
