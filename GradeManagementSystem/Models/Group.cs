namespace GradeManagementSystem.Models
{
    public class Group
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public int Course { get; set; }
        public string Specialty { get; set; }

        public Group()
        {
            GroupName = string.Empty;
            Course = 1;
            Specialty = string.Empty;
        }

        public Group(int groupID, string groupName, int course, string specialty)
        {
            GroupID = groupID;
            GroupName = groupName;
            Course = course;
            Specialty = specialty;
        }

        public override string ToString() => GroupName;
    }
}
