using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using GradeManagementSystem.Data;
using GradeManagementSystem.Helpers;

namespace GradeManagementSystem.Forms
{
    public class StudentsForm : Form
    {
        private readonly string connectionString = AppConfig.ConnectionString;
        private readonly LookupRepository lookupRepository = new LookupRepository();

        private TextBox txtSearch;
        private DataGridView dgvStudents;
        private Label lblCount;

        public StudentsForm()
        {
            UiTheme.ApplyForm(this);
            Text = "Справочник студентов";
            ClientSize = new Size(1100, 650);
            MinimumSize = new Size(900, 520);

            BuildUi();
            LoadStudents();
        }

        private void BuildUi()
        {
            Panel header = UiTheme.Header("Справочник студентов",
                                          "Просмотр, добавление, изменение и удаление записей о студентах");
            header.Dock = DockStyle.Top;

            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Background,
                Padding = new Padding(18, 14, 18, 14)
            };

            FlowLayoutPanel toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 52,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = UiTheme.Background,
                Padding = new Padding(0, 4, 0, 8)
            };

            Label searchLabel = new Label
            {
                Text = "Поиск по фамилии:",
                Font = UiTheme.BaseFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Margin = new Padding(0, 10, 6, 0)
            };
            toolbar.Controls.Add(searchLabel);

            txtSearch = UiTheme.TextBox(240);
            txtSearch.Margin = new Padding(0, 5, 10, 0);
            txtSearch.TextChanged += txtSearch_TextChanged;
            toolbar.Controls.Add(txtSearch);

            Button btnAdd = UiTheme.PrimaryButton("Добавить");
            btnAdd.Margin = new Padding(6, 5, 6, 0);
            btnAdd.Click += btnAdd_Click;
            toolbar.Controls.Add(btnAdd);

            Button btnEdit = UiTheme.SecondaryButton("Редактировать");
            btnEdit.Width = 150;
            btnEdit.Margin = new Padding(0, 5, 6, 0);
            btnEdit.Click += btnEdit_Click;
            toolbar.Controls.Add(btnEdit);

            Button btnDelete = UiTheme.SecondaryButton("Удалить");
            btnDelete.Width = 120;
            btnDelete.Margin = new Padding(0, 5, 6, 0);
            btnDelete.Click += btnDelete_Click;
            toolbar.Controls.Add(btnDelete);

            Button btnClose = UiTheme.SecondaryButton("Закрыть");
            btnClose.Width = 120;
            btnClose.Margin = new Padding(0, 5, 0, 0);
            btnClose.Click += (s, e) => Close();
            toolbar.Controls.Add(btnClose);

            GroupBox box = new GroupBox
            {
                Text = "Список студентов",
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Font = UiTheme.SectionFont,
                ForeColor = UiTheme.TextPrimary,
                Padding = new Padding(12, 10, 12, 12)
            };

            dgvStudents = UiTheme.Grid();
            dgvStudents.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                    EditSelected();
            };
            box.Controls.Add(dgvStudents);

            Panel statusPanel = new Panel { Dock = DockStyle.Bottom, Height = 26, BackColor = UiTheme.Background };
            lblCount = new Label
            {
                Text = "Записей: 0",
                Font = UiTheme.BaseFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(0, 4)
            };
            statusPanel.Controls.Add(lblCount);

            content.Controls.Add(box);
            content.Controls.Add(toolbar);
            content.Controls.Add(statusPanel);

            Controls.Add(content);
            Controls.Add(header);
        }

        private void LoadStudents()
        {
            txtSearch_TextChanged(txtSearch, EventArgs.Empty);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            string query = "SELECT s.StudentID, s.LastName, s.FirstName, s.MiddleName, " +
                           "s.BirthDate, g.GroupName AS Группа " +
                           "FROM Students s " +
                           "INNER JOIN Groups g ON s.GroupID = g.GroupID ";

            if (!string.IsNullOrEmpty(searchText))
            {
                query += "WHERE s.LastName LIKE @SearchText ";
            }

            query += "ORDER BY s.LastName, s.FirstName";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        cmd.Parameters.AddWithValue("@SearchText", "%" + searchText + "%");
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvStudents.DataSource = dt;

                        if (dgvStudents.Columns.Contains("StudentID"))
                            dgvStudents.Columns["StudentID"].Visible = false;

                        if (dgvStudents.Columns.Contains("LastName"))
                            dgvStudents.Columns["LastName"].HeaderText = "Фамилия";
                        if (dgvStudents.Columns.Contains("FirstName"))
                            dgvStudents.Columns["FirstName"].HeaderText = "Имя";
                        if (dgvStudents.Columns.Contains("MiddleName"))
                            dgvStudents.Columns["MiddleName"].HeaderText = "Отчество";
                        if (dgvStudents.Columns.Contains("BirthDate"))
                        {
                            dgvStudents.Columns["BirthDate"].HeaderText = "Дата рождения";
                            dgvStudents.Columns["BirthDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
                        }

                        lblCount.Text = $"Записей: {dt.Rows.Count}";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки списка студентов", ex);
                MessageBox.Show($"Не удалось загрузить список студентов: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (StudentEditForm form = new StudentEditForm(null))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadStudents();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            EditSelected();
        }

        private void EditSelected()
        {
            if (dgvStudents.CurrentRow == null)
            {
                MessageBox.Show("Выберите студента в таблице.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int studentID = Convert.ToInt32(dgvStudents.CurrentRow.Cells["StudentID"].Value);

            using (StudentEditForm form = new StudentEditForm(studentID))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadStudents();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvStudents.CurrentRow == null)
            {
                MessageBox.Show("Выберите студента в таблице.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int studentID = Convert.ToInt32(dgvStudents.CurrentRow.Cells["StudentID"].Value);
            string fullName = dgvStudents.CurrentRow.Cells["LastName"].Value + " " +
                              dgvStudents.CurrentRow.Cells["FirstName"].Value;

            int gradesCount = lookupRepository.GetGradesCountForStudent(studentID);
            string question = gradesCount > 0
                ? $"У студента {fullName} в журнале {gradesCount} оценок.\n" +
                  "Оценки будут удалены вместе со студентом (каскадное удаление).\n\nПродолжить?"
                : $"Удалить студента {fullName}?";

            if (MessageBox.Show(question, "Подтверждение удаления",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Students WHERE StudentID = @StudentID", conn))
                {
                    cmd.Parameters.AddWithValue("@StudentID", studentID);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                Logger.Info($"Удалён студент StudentID={studentID}, оценок удалено: {gradesCount}");
                LoadStudents();
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка удаления студента StudentID={studentID}", ex);
                MessageBox.Show($"Не удалось удалить студента: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
