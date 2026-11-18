using System;
using System.Configuration;
using System.Data.SqlClient;
using NUnit.Framework;

namespace GradeManagementSystem.Tests
{
    [TestFixture]
    public class GradeStudentIntegrationTests
    {
        private string connectionString;
        private int testGroupID;
        private int testStudentID;
        private int testSubjectID;
        private int testSemesterID;
        private int testTeacherID;

        [SetUp]
        public void SetUp()
        {
            connectionString = ConfigurationManager
                .ConnectionStrings["GradeManagementDB_Test"].ConnectionString;

            testGroupID = ExecuteScalarInt(
                "INSERT INTO Groups (GroupName, Course, Specialty) " +
                "OUTPUT INSERTED.GroupID " +
                "VALUES ('ТЕСТ-99', 1, 'Тестовая специальность')");

            testStudentID = ExecuteScalarInt(
                "INSERT INTO Students (LastName, FirstName, MiddleName, BirthDate, GroupID) " +
                "OUTPUT INSERTED.StudentID " +
                "VALUES ('Тестов', 'Тест', 'Тестович', '2004-01-01', @p)", testGroupID);

            testSubjectID = ExecuteScalarInt("SELECT TOP 1 SubjectID FROM Subjects");
            testSemesterID = ExecuteScalarInt("SELECT TOP 1 SemesterID FROM Semesters");
            testTeacherID = ExecuteScalarInt("SELECT TOP 1 UserID FROM Users");
        }

        [TearDown]
        public void TearDown()
        {
            ExecuteNonQuery("DELETE FROM Students WHERE StudentID = @p", testStudentID);
            ExecuteNonQuery("DELETE FROM Groups WHERE GroupID = @p", testGroupID);
        }

        [Test]
        public void AddGrade_ForExistingStudent_GradeIsLinkedToStudent()
        {
            int gradeID = InsertTestGrade(5);

            string studentLastName = ExecuteScalarString(
                "SELECT s.LastName FROM Grades g " +
                "INNER JOIN Students s ON g.StudentID = s.StudentID " +
                "WHERE g.GradeID = @p", gradeID);

            Assert.AreEqual("Тестов", studentLastName);
        }

        [Test]
        public void DeleteStudent_WithGrades_CascadesGradesDeletion()
        {
            InsertTestGrade(5);
            InsertTestGrade(4);

            int beforeDelete = ExecuteScalarInt(
                "SELECT COUNT(*) FROM Grades WHERE StudentID = @p", testStudentID);
            Assert.AreEqual(2, beforeDelete);

            ExecuteNonQuery("DELETE FROM Students WHERE StudentID = @p", testStudentID);

            int afterDelete = ExecuteScalarInt(
                "SELECT COUNT(*) FROM Grades WHERE StudentID = @p", testStudentID);
            Assert.AreEqual(0, afterDelete);
        }

        [Test]
        public void DeleteGroup_WithStudents_ThrowsForeignKeyException()
        {
            SqlException exception = Assert.Throws<SqlException>(() =>
                ExecuteNonQuery("DELETE FROM Groups WHERE GroupID = @p", testGroupID));

            Assert.AreEqual(547, exception.Number);
        }

        [TestCase(1)]
        [TestCase(6)]
        public void AddGrade_ValueOutOfRange_ThrowsCheckConstraintException(int invalidValue)
        {
            SqlException exception = Assert.Throws<SqlException>(() => InsertTestGrade(invalidValue));
            Assert.AreEqual(547, exception.Number);
        }

        private int InsertTestGrade(int value)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Grades " +
                    "(StudentID, SubjectID, SemesterID, TeacherID, GradeValue, GradeDate) " +
                    "OUTPUT INSERTED.GradeID " +
                    "VALUES (@student, @subject, @semester, @teacher, @value, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@student", testStudentID);
                    cmd.Parameters.AddWithValue("@subject", testSubjectID);
                    cmd.Parameters.AddWithValue("@semester", testSemesterID);
                    cmd.Parameters.AddWithValue("@teacher", testTeacherID);
                    cmd.Parameters.AddWithValue("@value", value);
                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        private int ExecuteScalarInt(string query, params object[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                for (int i = 0; i < parameters.Length; i++)
                    cmd.Parameters.AddWithValue("@p" + (i == 0 ? "" : i.ToString()), parameters[i]);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private string ExecuteScalarString(string query, params object[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                for (int i = 0; i < parameters.Length; i++)
                    cmd.Parameters.AddWithValue("@p" + (i == 0 ? "" : i.ToString()), parameters[i]);
                conn.Open();
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        private void ExecuteNonQuery(string query, params object[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                for (int i = 0; i < parameters.Length; i++)
                    cmd.Parameters.AddWithValue("@p" + (i == 0 ? "" : i.ToString()), parameters[i]);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
