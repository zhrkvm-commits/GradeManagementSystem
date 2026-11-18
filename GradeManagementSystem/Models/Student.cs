using System;

namespace GradeManagementSystem.Models
{
    public class Student
    {
        public int StudentID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime BirthDate { get; set; }
        public int GroupID { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime CreatedDate { get; set; }

        public Student()
        {
            LastName = string.Empty;
            FirstName = string.Empty;
            MiddleName = string.Empty;
            BirthDate = DateTime.Now;
            EnrollmentDate = DateTime.Now;
            CreatedDate = DateTime.Now;
        }

        public Student(int studentID, string lastName, string firstName, string middleName,
                       DateTime birthDate, int groupID, DateTime enrollmentDate, DateTime createdDate)
        {
            StudentID = studentID;
            LastName = lastName;
            FirstName = firstName;
            MiddleName = middleName;
            BirthDate = birthDate;
            GroupID = groupID;
            EnrollmentDate = enrollmentDate;
            CreatedDate = createdDate;
        }

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    }
}
