using System;

namespace GradeManagementSystem.Models
{
    public class Semester
    {
        public int SemesterID { get; set; }
        public string SemesterName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Semester()
        {
            SemesterName = string.Empty;
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
        }

        public Semester(int semesterID, string semesterName, DateTime startDate, DateTime endDate)
        {
            SemesterID = semesterID;
            SemesterName = semesterName;
            StartDate = startDate;
            EndDate = endDate;
        }

        public override string ToString() => SemesterName;
    }
}
