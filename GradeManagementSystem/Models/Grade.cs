using System;

namespace GradeManagementSystem.Models
{
    public class Grade
    {
        public int GradeID { get; set; }
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public int SemesterID { get; set; }
        public int TeacherID { get; set; }
        public int GradeValue { get; set; }
        public DateTime GradeDate { get; set; }
        public string Comment { get; set; }

        public Grade()
        {
            Comment = string.Empty;
            GradeDate = DateTime.Now;
        }

        public bool IsPassing => GradeValue >= 3;
    }
}
