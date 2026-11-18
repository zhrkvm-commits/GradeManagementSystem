using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using GradeManagementSystem.Helpers;
using GradeManagementSystem.Models;

namespace GradeManagementSystem.Data
{
    public class LookupRepository
    {
        private readonly string connectionString;

        public LookupRepository()
        {
            connectionString = AppConfig.ConnectionString;
        }

        public List<Group> GetGroups()
        {
            List<Group> groups = new List<Group>();
            string query = "SELECT GroupID, GroupName, Course, Specialty FROM Groups ORDER BY GroupName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        groups.Add(new Group(
                            Convert.ToInt32(reader["GroupID"]),
                            reader["GroupName"].ToString(),
                            Convert.ToInt32(reader["Course"]),
                            reader["Specialty"].ToString()));
                    }
                }
            }

            return groups;
        }

        public List<Subject> GetSubjects()
        {
            List<Subject> subjects = new List<Subject>();
            string query = "SELECT SubjectID, SubjectName FROM Subjects ORDER BY SubjectName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        subjects.Add(new Subject(
                            Convert.ToInt32(reader["SubjectID"]),
                            reader["SubjectName"].ToString()));
                    }
                }
            }

            return subjects;
        }

        public List<Semester> GetSemesters()
        {
            List<Semester> semesters = new List<Semester>();
            string query = "SELECT SemesterID, SemesterName, StartDate, EndDate " +
                           "FROM Semesters ORDER BY StartDate DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        semesters.Add(new Semester(
                            Convert.ToInt32(reader["SemesterID"]),
                            reader["SemesterName"].ToString(),
                            Convert.ToDateTime(reader["StartDate"]),
                            Convert.ToDateTime(reader["EndDate"])));
                    }
                }
            }

            return semesters;
        }

        public string GetGroupName(int groupID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT GroupName FROM Groups WHERE GroupID = @GroupID", conn))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupID);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result == null ? string.Empty : result.ToString();
            }
        }

        public int GetStudentsCount()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Students", conn))
            {
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int GetGradesCount()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Grades", conn))
            {
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public double GetOverallAverage()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT AVG(CAST(GradeValue AS FLOAT)) FROM Grades", conn))
            {
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result == DBNull.Value ? 0 : Convert.ToDouble(result);
            }
        }

        public int GetGradesCountForStudent(int studentID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Grades WHERE StudentID = @StudentID", conn))
            {
                cmd.Parameters.AddWithValue("@StudentID", studentID);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int GetStudentsCountForGroup(int groupID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Students WHERE GroupID = @GroupID", conn))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupID);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public DataTable GetUsersData()
        {
            string query = "SELECT u.UserID, u.Login, u.FullName AS [ФИО], u.Role AS [Роль], " +
                           "g.GroupName AS [Группа] " +
                           "FROM Users u LEFT JOIN Groups g ON u.GroupID = g.GroupID " +
                           "ORDER BY u.UserID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
    }
}
