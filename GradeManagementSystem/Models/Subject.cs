namespace GradeManagementSystem.Models
{
    public class Subject
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }

        public Subject()
        {
            SubjectName = string.Empty;
        }

        public Subject(int subjectID, string subjectName)
        {
            SubjectID = subjectID;
            SubjectName = subjectName;
        }

        public override string ToString() => SubjectName;
    }
}
