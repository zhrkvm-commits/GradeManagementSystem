using System;

namespace GradeManagementSystem.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public int? GroupID { get; set; }
        public DateTime CreatedDate { get; set; }

        public User()
        {
            Login = string.Empty;
            Password = string.Empty;
            Role = "Teacher";
            FullName = string.Empty;
            CreatedDate = DateTime.Now;
        }

        public User(int userID, string login, string password, string role,
                    string fullName, int? groupID, DateTime createdDate)
        {
            UserID = userID;
            Login = login;
            Password = password;
            Role = role;
            FullName = fullName;
            GroupID = groupID;
            CreatedDate = createdDate;
        }

        public bool IsCurator => Role == "Curator";
        public bool IsAdmin => Role == "Admin";
        public bool IsTeacher => Role == "Teacher";
    }
}
