using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using GradeManagementSystem.Data;
using GradeManagementSystem.Helpers;
using GradeManagementSystem.Models;

namespace GradeManagementSystem.Forms
{
    public class GradesForm : Form
    {
        private readonly User currentUser;
        private readonly string connectionString = AppConfig.ConnectionString;
        private readonly GradeRepository repository = new GradeRepository();
        private readonly LookupRepository lookupRepository = new LookupRepository();

        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private ComboBox cmbGroups;
        private ComboBox cmbStudents;
        private ComboBox cmbSubjects;
        private DataGridView dgvGrades;
        private Label lblPageInfo;
        private Label lblCount;

        private int currentPage = 1;
        private int totalPages = 1;
        private readonly int pageSize = AppConfig.PageSize;
        private string currentSortColumn = "g.GradeDate";
        private string currentSortDirection = "DESC";
        private bool suppressEvents;

        public GradesForm(User user)
        {
            currentUser = user;
            UiTheme.ApplyForm(this);
            Text = "Журнал оценок";
            ClientSize = new Size(1180, 700);
            MinimumSize = new Size(980, 600);

            BuildUi();
            LoadLookups();
            SetDefaultPeriod();
            LoadGrades();
        }

        private void BuildUi()
        {
            Panel header = UiTheme.Header("Журнал оценок",
                                          "Фильтрация, сортировка и постраничный просмотр записей об успеваемости");
            header.Dock = DockStyle.Top;

            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Background,
                Padding = new Padding(18, 12, 18, 12)
            };

            GroupBox filters = new GroupBox
            {
                Text = "Фильтры",
                Dock = DockStyle.Top,
                Height = 86,
                BackColor = UiTheme.Surface,
                Font = UiTheme.SectionFont,
                ForeColor = UiTheme.TextPrimary,
                Padding = new Padding(12, 8, 12, 8)
            };

            FlowLayoutPanel filterFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = UiTheme.Surface
            };

            dtpStartDate = CreateDatePicker();
            dtpEndDate = CreateDatePicker();
            cmbGroups = UiTheme.Combo(150);
            cmbStudents = UiTheme.Combo(240);
            cmbSubjects = UiTheme.Combo(200);

            filterFlow.Controls.Add(CreateFilterCell("Период с", dtpStartDate));
            filterFlow.Controls.Add(CreateFilterCell("по", dtpEndDate));
            filterFlow.Controls.Add(CreateFilterCell("Группа", cmbGroups));
            filterFlow.Controls.Add(CreateFilterCell("Студент", cmbStudents));
            filterFlow.Controls.Add(CreateFilterCell("Предмет", cmbSubjects));

            Button btnReset = UiTheme.SecondaryButton("Сбросить фильтр");
            btnReset.Width = 150;
            btnReset.Margin = new Padding(8, 24, 0, 0);
            btnReset.Click += btnReset_Click;
            filterFlow.Controls.Add(btnReset);

            filters.Controls.Add(filterFlow);

            FlowLayoutPanel toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = UiTheme.Background,
                Padding = new Padding(0, 10, 0, 6)
            };

            Button btnAdd = UiTheme.PrimaryButton("Добавить");
            btnAdd.Width = 128;
            btnAdd.Margin = new Padding(0, 0, 6, 0);
            btnAdd.Click += btnAdd_Click;
            toolbar.Controls.Add(btnAdd);

            Button btnEdit = UiTheme.SecondaryButton("Изменить");
            btnEdit.Width = 128;
            btnEdit.Margin = new Padding(0, 0, 6, 0);
            btnEdit.Click += btnEdit_Click;
            toolbar.Controls.Add(btnEdit);

            Button btnDelete = UiTheme.SecondaryButton("Удалить");
            btnDelete.Width = 128;
            btnDelete.Margin = new Padding(0, 0, 6, 0);
            btnDelete.Click += btnDelete_Click;
            toolbar.Controls.Add(btnDelete);

            Button btnClose = UiTheme.SecondaryButton("Закрыть");
            btnClose.Width = 120;
            btnClose.Click += (s, e) => Close();
            toolbar.Controls.Add(btnClose);

            lblCount = new Label
            {
                Text = "Найдено: 0",
                Font = UiTheme.BaseFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Margin = new Padding(16, 10, 0, 0)
            };
            toolbar.Controls.Add(lblCount);

            GroupBox gridBox = new GroupBox
            {
                Text = "Оценки студентов",
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Font = UiTheme.SectionFont,
                ForeColor = UiTheme.TextPrimary,
                Padding = new Padding(12, 10, 12, 12)
            };

            dgvGrades = UiTheme.Grid();
            dgvGrades.ColumnHeaderMouseClick += dgvGrades_ColumnHeaderMouseClick;
            dgvGrades.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                    EditSelected();
            };
            gridBox.Controls.Add(dgvGrades);

            Panel pagination = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = UiTheme.Background
            };

            Button btnPrevPage = UiTheme.SecondaryButton("← Предыдущая страница");
            btnPrevPage.Width = 190;
            btnPrevPage.Height = 30;
            btnPrevPage.Location = new Point(0, 6);
            btnPrevPage.Click += btnPrevPage_Click;
            pagination.Controls.Add(btnPrevPage);

            lblPageInfo = new Label
            {
                Text = "Страница 1 из 1",
                Font = UiTheme.BaseFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(202, 12)
            };
            pagination.Controls.Add(lblPageInfo);

            Button btnNextPage = UiTheme.SecondaryButton("Следующая страница →");
            btnNextPage.Width = 190;
            btnNextPage.Height = 30;
            btnNextPage.Location = new Point(364, 6);
            btnNextPage.Click += btnNextPage_Click;
            pagination.Controls.Add(btnNextPage);

            content.Controls.Add(gridBox);
            content.Controls.Add(pagination);
            content.Controls.Add(toolbar);
            content.Controls.Add(filters);

            Controls.Add(content);
            Controls.Add(header);
        }

        private DateTimePicker CreateDatePicker()
        {
            DateTimePicker picker = new DateTimePicker
            {
                Font = UiTheme.BaseFont,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy",
                Width = 130
            };
            picker.ValueChanged += (s, e) =>
            {
                if (suppressEvents) return;
                currentPage = 1;
                LoadGrades();
            };
            return picker;
        }

        private Panel CreateFilterCell(string caption, Control control)
        {
            Panel cell = new Panel
            {
                Width = control.Width + 8,
                Height = 58,
                Margin = new Padding(0, 0, 8, 0),
                BackColor = UiTheme.Surface
            };

            Label label = new Label
            {
                Text = caption,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(0, 2)
            };
            cell.Controls.Add(label);

            control.Location = new Point(0, 22);
            cell.Controls.Add(control);
            return cell;
        }

        private void LoadLookups()
        {
            suppressEvents = true;
            try
            {
                var groups = lookupRepository.GetGroups();
                cmbGroups.DataSource = groups;
                cmbGroups.DisplayMember = "GroupName";
                cmbGroups.ValueMember = "GroupID";
                cmbGroups.SelectedIndex = -1;
                cmbGroups.SelectedIndexChanged += cmbGroups_SelectedIndexChanged;

                var subjects = lookupRepository.GetSubjects();
                cmbSubjects.DataSource = subjects;
                cmbSubjects.DisplayMember = "SubjectName";
                cmbSubjects.ValueMember = "SubjectID";
                cmbSubjects.SelectedIndex = -1;
                cmbSubjects.SelectedIndexChanged += (s, e) =>
                {
                    if (suppressEvents) return;
                    currentPage = 1;
                    LoadGrades();
                };

                LoadAllStudents();
            }
            catch (Exception ex)
            {
                Logger.Error("Не удалось загрузить справочники для журнала оценок", ex);
            }
            finally
            {
                suppressEvents = false;
            }
        }

        private void LoadAllStudents()
        {
            suppressEvents = true;
            string query = "SELECT s.StudentID, s.LastName + ' ' + s.FirstName + ' ' + s.MiddleName + " +
                           "' (' + g.GroupName + ')' AS [Подпись] " +
                           "FROM Students s INNER JOIN Groups g ON s.GroupID = g.GroupID " +
                           "ORDER BY s.LastName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                cmbStudents.DataSource = dt;
                cmbStudents.DisplayMember = "Подпись";
                cmbStudents.ValueMember = "StudentID";
                cmbStudents.SelectedIndex = -1;
            }

            cmbStudents.SelectedIndexChanged += (s, e) =>
            {
                if (suppressEvents) return;
                currentPage = 1;
                LoadGrades();
            };
            suppressEvents = false;
        }

        private void cmbGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (suppressEvents)
                return;

            LoadStudentsByGroup();
            currentPage = 1;
            LoadGrades();
        }

        private void LoadStudentsByGroup()
        {
            suppressEvents = true;
            int? groupID = cmbGroups.SelectedIndex != -1 ? (int?)cmbGroups.SelectedValue : null;

            string query = "SELECT s.StudentID, s.LastName + ' ' + s.FirstName + ' ' + s.MiddleName + " +
                           "' (' + g.GroupName + ')' AS [Подпись] " +
                           "FROM Students s INNER JOIN Groups g ON s.GroupID = g.GroupID ";
            if (groupID.HasValue)
                query += "WHERE s.GroupID = @GroupID ";
            query += "ORDER BY s.LastName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (groupID.HasValue)
                    cmd.Parameters.AddWithValue("@GroupID", groupID.Value);

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    cmbStudents.DataSource = dt;
                    cmbStudents.DisplayMember = "Подпись";
                    cmbStudents.ValueMember = "StudentID";
                    cmbStudents.SelectedIndex = -1;
                }
            }

            suppressEvents = false;
        }

        private void SetDefaultPeriod()
        {
            suppressEvents = true;
            DateTime today = DateTime.Today;
            dtpStartDate.Value = new DateTime(today.Year, today.Month, 1);
            dtpEndDate.Value = today;
            suppressEvents = false;
        }

        private void LoadGrades()
        {
            LoadGradesWithFilters();
        }

        private void LoadGradesPaged()
        {
            LoadGradesWithFilters();
        }

        private void LoadGradesWithFilters()
        {
            DateTime startDate = dtpStartDate.Value.Date;
            DateTime endDate = dtpEndDate.Value.Date;

            int? groupID = cmbGroups.SelectedIndex != -1 ? (int?)cmbGroups.SelectedValue : null;
            int? studentID = cmbStudents.SelectedIndex != -1 ? (int?)cmbStudents.SelectedValue : null;
            int? subjectID = cmbSubjects.SelectedIndex != -1 ? (int?)cmbSubjects.SelectedValue : null;

            string filter = " WHERE g.GradeDate BETWEEN @StartDate AND @EndDate";

            if (groupID.HasValue)
                filter += " AND s.GroupID = @GroupID";
            if (studentID.HasValue)
                filter += " AND g.StudentID = @StudentID";
            if (subjectID.HasValue)
                filter += " AND g.SubjectID = @SubjectID";

            string baseFrom = "FROM Grades g " +
                              "INNER JOIN Students s ON g.StudentID = s.StudentID " +
                              "INNER JOIN Groups gr ON s.GroupID = gr.GroupID " +
                              "INNER JOIN Subjects sub ON g.SubjectID = sub.SubjectID ";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand countCmd = new SqlCommand("SELECT COUNT(*) " + baseFrom + filter, conn))
                    {
                        AddFilterParameters(countCmd, startDate, endDate, groupID, studentID, subjectID);
                        int totalRows = Convert.ToInt32(countCmd.ExecuteScalar());
                        totalPages = Math.Max(1, (int)Math.Ceiling(totalRows / (double)pageSize));

                        if (currentPage > totalPages)
                            currentPage = totalPages;
                        if (currentPage < 1)
                            currentPage = 1;

                        lblCount.Text = $"Найдено: {totalRows}";
                    }

                    int offset = (currentPage - 1) * pageSize;

                    string query = "SELECT g.GradeID, " +
                                   "s.LastName + ' ' + s.FirstName + ' ' + s.MiddleName AS Студент, " +
                                   "sub.SubjectName AS Предмет, g.GradeValue AS Оценка, " +
                                   "g.GradeDate AS Дата, g.Comment AS Комментарий " +
                                   baseFrom + filter +
                                   $" ORDER BY {currentSortColumn} {currentSortDirection}, g.GradeID DESC " +
                                   $" OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        AddFilterParameters(cmd, startDate, endDate, groupID, studentID, subjectID);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgvGrades.DataSource = dt;

                            if (dgvGrades.Columns.Contains("GradeID"))
                                dgvGrades.Columns["GradeID"].Visible = false;
                            if (dgvGrades.Columns.Contains("Комментарий"))
                                dgvGrades.Columns["Комментарий"].FillWeight = 160;
                        }
                    }

                    lblPageInfo.Text = $"Страница {currentPage} из {totalPages}";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки журнала оценок", ex);
                MessageBox.Show($"Не удалось загрузить журнал оценок: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void AddFilterParameters(SqlCommand cmd, DateTime startDate, DateTime endDate,
                                                int? groupID, int? studentID, int? subjectID)
        {
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);

            if (groupID.HasValue)
                cmd.Parameters.AddWithValue("@GroupID", groupID.Value);
            if (studentID.HasValue)
                cmd.Parameters.AddWithValue("@StudentID", studentID.Value);
            if (subjectID.HasValue)
                cmd.Parameters.AddWithValue("@SubjectID", subjectID.Value);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            suppressEvents = true;

            DateTime minDate = DateTime.Today;
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT MIN(GradeDate) FROM Grades", conn))
            {
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                    minDate = Convert.ToDateTime(result);
            }

            dtpStartDate.Value = minDate;
            dtpEndDate.Value = DateTime.Today;
            cmbGroups.SelectedIndex = -1;
            cmbSubjects.SelectedIndex = -1;
            LoadAllStudents();
            currentPage = 1;
            suppressEvents = false;

            LoadGrades();
        }

        private void dgvGrades_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string columnName = dgvGrades.Columns[e.ColumnIndex].Name;
            string sortColumn;

            switch (columnName)
            {
                case "Студент": sortColumn = "s.LastName"; break;
                case "Предмет": sortColumn = "sub.SubjectName"; break;
                case "Оценка": sortColumn = "g.GradeValue"; break;
                case "Дата": sortColumn = "g.GradeDate"; break;
                default: sortColumn = "g.GradeDate"; break;
            }

            if (currentSortColumn == sortColumn)
            {
                currentSortDirection = currentSortDirection == "ASC" ? "DESC" : "ASC";
            }
            else
            {
                currentSortColumn = sortColumn;
                currentSortDirection = "ASC";
            }

            currentPage = 1;
            LoadGrades();
        }

        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadGradesPaged();
            }
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadGradesPaged();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (GradeEditForm form = new GradeEditForm(currentUser, null))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                    LoadGrades();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            EditSelected();
        }

        private void EditSelected()
        {
            if (dgvGrades.CurrentRow == null)
            {
                MessageBox.Show("Выберите оценку в таблице.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int gradeID = Convert.ToInt32(dgvGrades.CurrentRow.Cells["GradeID"].Value);
            Grade grade = repository.GetGradeById(gradeID);

            if (grade == null)
                return;

            if (currentUser.IsTeacher && grade.TeacherID != currentUser.UserID)
            {
                MessageBox.Show("Преподаватель может редактировать только выставленные им оценки.",
                    "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (GradeEditForm form = new GradeEditForm(currentUser, gradeID))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                    LoadGrades();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvGrades.CurrentRow == null)
            {
                MessageBox.Show("Выберите оценку в таблице.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int gradeID = Convert.ToInt32(dgvGrades.CurrentRow.Cells["GradeID"].Value);
            Grade grade = repository.GetGradeById(gradeID);

            if (grade == null)
                return;

            if (!currentUser.IsAdmin && (!currentUser.IsTeacher || grade.TeacherID != currentUser.UserID))
            {
                MessageBox.Show("Недостаточно прав для удаления этой оценки.",
                    "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить выбранную оценку?", "Подтверждение удаления",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                repository.DeleteGrade(gradeID);
                Logger.Info($"Удалена оценка GradeID={gradeID}");
                LoadGrades();
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка удаления оценки GradeID={gradeID}", ex);
                MessageBox.Show($"Не удалось удалить оценку: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
