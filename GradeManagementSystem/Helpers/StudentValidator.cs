using System;
using System.Collections.Generic;

namespace GradeManagementSystem.Helpers
{
    public static class StudentValidator
    {
        public static List<string> Validate(string lastName, string firstName,
                                            string middleName, DateTime birthDate, int? groupID)
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(lastName))
                errors.Add("Введите фамилию!");

            if (string.IsNullOrWhiteSpace(firstName))
                errors.Add("Введите имя!");

            if (string.IsNullOrWhiteSpace(middleName))
                errors.Add("Введите отчество!");

            if (birthDate >= DateTime.Now.Date)
                errors.Add("Дата рождения должна быть в прошлом!");

            if (birthDate < new DateTime(1900, 1, 1))
                errors.Add("Дата рождения некорректна!");

            if (!groupID.HasValue || groupID.Value <= 0)
                errors.Add("Выберите учебную группу!");

            return errors;
        }

        public static bool IsGradeValueValid(int gradeValue)
        {
            return gradeValue >= 2 && gradeValue <= 5;
        }
    }
}
