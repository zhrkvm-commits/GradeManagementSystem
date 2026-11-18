using System;
using System.Collections.Generic;
using NUnit.Framework;
using GradeManagementSystem.Helpers;

namespace GradeManagementSystem.Tests
{
    [TestFixture]
    public class StudentValidatorTests
    {
        [Test]
        public void Validate_AllFieldsCorrect_ReturnsNoErrors()
        {
            List<string> errors = StudentValidator.Validate(
                "Иванов", "Иван", "Иванович", new DateTime(2004, 5, 15), 1);

            Assert.IsEmpty(errors);
        }

        [TestCase("", "Иван", "Иванович")]
        [TestCase("Иванов", "", "Иванович")]
        [TestCase("Иванов", "Иван", "")]
        [TestCase("   ", "Иван", "Иванович")]
        public void Validate_EmptyRequiredField_ReturnsError(string lastName, string firstName, string middleName)
        {
            List<string> errors = StudentValidator.Validate(
                lastName, firstName, middleName, new DateTime(2004, 5, 15), 1);

            Assert.IsNotEmpty(errors);
        }

        [Test]
        public void Validate_FutureBirthDate_ReturnsError()
        {
            List<string> errors = StudentValidator.Validate(
                "Иванов", "Иван", "Иванович", DateTime.Now.AddDays(1), 1);

            Assert.Contains("Дата рождения должна быть в прошлом!", errors);
        }

        [Test]
        public void Validate_NoGroupSelected_ReturnsError()
        {
            List<string> errors = StudentValidator.Validate(
                "Иванов", "Иван", "Иванович", new DateTime(2004, 5, 15), null);

            Assert.Contains("Выберите учебную группу!", errors);
        }

        [TestCase(2, true)]
        [TestCase(3, true)]
        [TestCase(4, true)]
        [TestCase(5, true)]
        [TestCase(1, false)]
        [TestCase(6, false)]
        [TestCase(0, false)]
        public void IsGradeValueValid_VariousValues_ReturnsExpected(int value, bool expected)
        {
            Assert.AreEqual(expected, StudentValidator.IsGradeValueValid(value));
        }
    }
}
