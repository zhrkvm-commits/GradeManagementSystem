using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using GradeManagementSystem.Helpers;
using GradeManagementSystem.Models;

namespace GradeManagementSystem.Data
{
    public class GradeRepository
    {
        private readonly string connectionString;

        public GradeRepository()
        {
            connectionString = AppConfig.ConnectionString;
        }

        public GradeRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<Grade> GetAllGrades()
        {
            List<Grade> grades = new List<Grade>();
            string query = "SELECT GradeID, StudentID, SubjectID, SemesterID, TeacherID, " +
                           "GradeValue, GradeDate, Comment FROM Grades ORDER BY GradeDate DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        grades.Add(ReadGrade(reader));
                    }
                }
            }

            return grades;
        }

        public Grade GetGradeById(int gradeID)
        {
            string query = "SELECT GradeID, StudentID, SubjectID, SemesterID, TeacherID, " +
                           "GradeValue, GradeDate, Comment FROM Grades WHERE GradeID = @GradeID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@GradeID", gradeID);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return ReadGrade(reader);
                }
            }

            return null;
        }

        private static Grade ReadGrade(SqlDataReader reader)
        {
            return new Grade
            {
                GradeID = Convert.ToInt32(reader["GradeID"]),
                StudentID = Convert.ToInt32(reader["StudentID"]),
                SubjectID = Convert.ToInt32(reader["SubjectID"]),
                SemesterID = Convert.ToInt32(reader["SemesterID"]),
                TeacherID = Convert.ToInt32(reader["TeacherID"]),
                GradeValue = Convert.ToInt32(reader["GradeValue"]),
                GradeDate = Convert.ToDateTime(reader["GradeDate"]),
                Comment = reader["Comment"] == DBNull.Value ? string.Empty : reader["Comment"].ToString()
            };
        }

        public int AddGrade(Grade grade)
        {
            string query = "INSERT INTO Grades (StudentID, SubjectID, SemesterID, TeacherID, " +
                           "GradeValue, GradeDate, Comment) " +
                           "OUTPUT INSERTED.GradeID " +
                           "VALUES (@StudentID, @SubjectID, @SemesterID, @TeacherID, " +
                           "@GradeValue, @GradeDate, @Comment)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@StudentID", grade.StudentID);
                cmd.Parameters.AddWithValue("@SubjectID", grade.SubjectID);
                cmd.Parameters.AddWithValue("@SemesterID", grade.SemesterID);
                cmd.Parameters.AddWithValue("@TeacherID", grade.TeacherID);
                cmd.Parameters.AddWithValue("@GradeValue", grade.GradeValue);
                cmd.Parameters.AddWithValue("@GradeDate", grade.GradeDate.Date);
                cmd.Parameters.AddWithValue("@Comment", (object)grade.Comment ?? DBNull.Value);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void UpdateGrade(Grade grade)
        {
            string query = "UPDATE Grades SET StudentID = @StudentID, SubjectID = @SubjectID, " +
                           "SemesterID = @SemesterID, GradeValue = @GradeValue, " +
                           "GradeDate = @GradeDate, Comment = @Comment " +
                           "WHERE GradeID = @GradeID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@StudentID", grade.StudentID);
                cmd.Parameters.AddWithValue("@SubjectID", grade.SubjectID);
                cmd.Parameters.AddWithValue("@SemesterID", grade.SemesterID);
                cmd.Parameters.AddWithValue("@GradeValue", grade.GradeValue);
                cmd.Parameters.AddWithValue("@GradeDate", grade.GradeDate.Date);
                cmd.Parameters.AddWithValue("@Comment", (object)grade.Comment ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GradeID", grade.GradeID);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteGrade(int gradeID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("DELETE FROM Grades WHERE GradeID = @GradeID", conn))
            {
                cmd.Parameters.AddWithValue("@GradeID", gradeID);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<int> GetGradesForStatistics(int? groupID, int? studentID, int? subjectID, int? semesterID)
        {
            List<int> grades = new List<int>();

            string query = "SELECT g.GradeValue FROM Grades g " +
                           "INNER JOIN Students s ON g.StudentID = s.StudentID " +
                           "WHERE 1=1";

            if (groupID.HasValue)
                query += " AND s.GroupID = @GroupID";
            if (studentID.HasValue)
                query += " AND g.StudentID = @StudentID";
            if (subjectID.HasValue)
                query += " AND g.SubjectID = @SubjectID";
            if (semesterID.HasValue)
                query += " AND g.SemesterID = @SemesterID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (groupID.HasValue)
                    cmd.Parameters.AddWithValue("@GroupID", groupID.Value);
                if (studentID.HasValue)
                    cmd.Parameters.AddWithValue("@StudentID", studentID.Value);
                if (subjectID.HasValue)
                    cmd.Parameters.AddWithValue("@SubjectID", subjectID.Value);
                if (semesterID.HasValue)
                    cmd.Parameters.AddWithValue("@SemesterID", semesterID.Value);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        grades.Add(Convert.ToInt32(reader["GradeValue"]));
                    }
                }
            }

            return grades;
        }

        public DataTable GetReportData(DateTime startDate, DateTime endDate,
                                       int? groupID, int? subjectID, int? semesterID, int? teacherID)
        {
            string query = "SELECT s.LastName + ' ' + s.FirstName + ' ' + s.MiddleName AS ФИО, " +
                           "gr.GroupName AS Группа, sub.SubjectName AS Предмет, " +
                           "g.GradeValue AS Оценка, g.GradeDate AS Дата, " +
                           "u.FullName AS Преподаватель, g.Comment AS Комментарий " +
                           "FROM Grades g " +
                           "INNER JOIN Students s ON g.StudentID = s.StudentID " +
                           "INNER JOIN Groups gr ON s.GroupID = gr.GroupID " +
                           "INNER JOIN Subjects sub ON g.SubjectID = sub.SubjectID " +
                           "INNER JOIN Users u ON g.TeacherID = u.UserID " +
                           "WHERE g.GradeDate BETWEEN @StartDate AND @EndDate";

            if (groupID.HasValue)
                query += " AND s.GroupID = @GroupID";
            if (subjectID.HasValue)
                query += " AND g.SubjectID = @SubjectID";
            if (semesterID.HasValue)
                query += " AND g.SemesterID = @SemesterID";
            if (teacherID.HasValue)
                query += " AND g.TeacherID = @TeacherID";

            query += " ORDER BY gr.GroupName, s.LastName, g.GradeDate";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Date);
                cmd.Parameters.AddWithValue("@EndDate", endDate.Date);

                if (groupID.HasValue)
                    cmd.Parameters.AddWithValue("@GroupID", groupID.Value);
                if (subjectID.HasValue)
                    cmd.Parameters.AddWithValue("@SubjectID", subjectID.Value);
                if (semesterID.HasValue)
                    cmd.Parameters.AddWithValue("@SemesterID", semesterID.Value);
                if (teacherID.HasValue)
                    cmd.Parameters.AddWithValue("@TeacherID", teacherID.Value);

                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetMonthlyAverage(int? groupID, DateTime startDate, DateTime endDate)
        {
            string query = "SELECT YEAR(g.GradeDate) AS Год, MONTH(g.GradeDate) AS Месяц, " +
                           "ROUND(AVG(CAST(g.GradeValue AS FLOAT)), 2) AS СреднийБалл, " +
                           "COUNT(g.GradeID) AS КоличествоОценок " +
                           "FROM Grades g " +
                           "INNER JOIN Students s ON g.StudentID = s.StudentID " +
                           "WHERE g.GradeDate BETWEEN @StartDate AND @EndDate";

            if (groupID.HasValue)
                query += " AND s.GroupID = @GroupID";

            query += " GROUP BY YEAR(g.GradeDate), MONTH(g.GradeDate) " +
                     "ORDER BY Год, Месяц";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Date);
                cmd.Parameters.AddWithValue("@EndDate", endDate.Date);

                if (groupID.HasValue)
                    cmd.Parameters.AddWithValue("@GroupID", groupID.Value);

                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetStudentStatistics(int? groupID, int? semesterID)
        {
            string query = "SELECT s.StudentID, " +
                           "s.LastName + ' ' + s.FirstName + ' ' + s.MiddleName AS [ФИО], " +
                           "gr.GroupName AS [Группа], " +
                           "CAST(ROUND(AVG(CAST(g.GradeValue AS FLOAT)), 2) AS DECIMAL(4,2)) AS [СреднийБалл], " +
                           "COUNT(g.GradeID) AS [КоличествоОценок] " +
                           "FROM Students s " +
                           "INNER JOIN Groups gr ON s.GroupID = gr.GroupID " +
                           "INNER JOIN Grades g ON g.StudentID = s.StudentID " +
                           "WHERE 1=1";

            if (groupID.HasValue)
                query += " AND s.GroupID = @GroupID";
            if (semesterID.HasValue)
                query += " AND g.SemesterID = @SemesterID";

            query += " GROUP BY s.StudentID, s.LastName, s.FirstName, s.MiddleName, gr.GroupName " +
                     "ORDER BY [СреднийБалл] DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                if (groupID.HasValue)
                    cmd.Parameters.AddWithValue("@GroupID", groupID.Value);
                if (semesterID.HasValue)
                    cmd.Parameters.AddWithValue("@SemesterID", semesterID.Value);

                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetGroupAverages()
        {
            string query = "SELECT gr.GroupName AS [Группа], " +
                           "CAST(ROUND(AVG(CAST(g.GradeValue AS FLOAT)), 2) AS DECIMAL(4,2)) AS [СреднийБалл], " +
                           "CAST(ROUND(ABS(AVG(CAST(g.GradeValue AS FLOAT) * g.GradeValue) - " +
                           "POWER(AVG(CAST(g.GradeValue AS FLOAT)), 2)), 2) AS DECIMAL(4,2)) AS [Дисперсия], " +
                           "COUNT(g.GradeID) AS [КоличествоОценок] " +
                           "FROM Groups gr " +
                           "INNER JOIN Students s ON s.GroupID = gr.GroupID " +
                           "INNER JOIN Grades g ON g.StudentID = s.StudentID " +
                           "GROUP BY gr.GroupName " +
                           "ORDER BY [СреднийБалл] DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public int CountGrades(int? groupID, int? semesterID, int? subjectID)
        {
            string query = "SELECT COUNT(*) FROM Grades g " +
                           "INNER JOIN Students s ON g.StudentID = s.StudentID WHERE 1=1";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (groupID.HasValue)
                {
                    query += " AND s.GroupID = @GroupID";
                    cmd.Parameters.AddWithValue("@GroupID", groupID.Value);
                }
                if (semesterID.HasValue)
                {
                    query += " AND g.SemesterID = @SemesterID";
                    cmd.Parameters.AddWithValue("@SemesterID", semesterID.Value);
                }
                if (subjectID.HasValue)
                {
                    query += " AND g.SubjectID = @SubjectID";
                    cmd.Parameters.AddWithValue("@SubjectID", subjectID.Value);
                }

                cmd.CommandText = query;
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}
