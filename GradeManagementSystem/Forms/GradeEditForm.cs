using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using GradeManagementSystem.Data;
using GradeManagementSystem.Helpers;
using GradeManagementSystem.Models;

namespace GradeManagementSystem.Forms
{
    public class GradeEditForm : Form
    {
        private readonly User currentUser;
        private readonly string connectionString = AppConfig.ConnectionString;
        private readonly int? gradeID;
        private readonly bool isNew;
        private readonly GradeRepository repository = new GradeRepository();
        private readonly ErrorProvider errorProvider = new ErrorProvider();

        private ComboBox cmbStudents;
        private ComboBox cmbSubjects;
        private ComboBox cmbSemesters;
        private NumericUpDown numGrade;
        private DateTimePicker dtpGradeDate;
        private TextBox txtComment;
        private Button btnSave;

        public GradeEditForm(User user, int? gradeID)
        {
            currentUser = user;
            this.gradeID = gradeID;
            isNew = !gradeID.HasValue;

            UiTheme.ApplyForm(this);
            Text = isNew ? "Новая оценка" : "Редактирование оценки";
            ClientSize = new Size(620, 590);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            BuildUi();
            LoadStudents();
            LoadSubjects();
            LoadSemesters();

            if (!isNew)
                LoadGrade(gradeID.Value);
        }

        private void BuildUi()
        {
            Panel header = UiTheme.Header(isNew ? "Регистрация оценки" : "Редактирование оценки",
                                          "Выберите студента, дисциплину и семестр, укажите значение оценки");
            header.Dock = DockStyle.Top;

            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Padding = new Padding(24, 12, 24, 8)
            };

            TableLayoutPanel table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                BackColor = UiTheme.Surface
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            cmbStudents = UiTheme.Combo(400);
            cmbStudents.Name = "cmbStudents";
            cmbSubjects = UiTheme.Combo(400);
            cmbSubjects.Name = "cmbSubjects";
            cmbSemesters = UiTheme.Combo(400);
            cmbSemesters.Name = "cmbSemesters";

            numGrade = new NumericUpDown
            {
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Minimum = 2,
                Maximum = 5,
                Value = 5,
                Width = 80,
                TextAlign = HorizontalAlignment.Center
            };
            numGrade.Name = "numGrade";

            dtpGradeDate = new DateTimePicker
            {
                Font = UiTheme.BaseFont,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy",
                Width = 160,
                MaxDate = DateTime.Today
            };
            dtpGradeDate.Name = "dtpGradeDate";

            txtComment = new TextBox
            {
                Font = UiTheme.BaseFont,
                Multiline = true,
                Height = 96,
                Width = 400,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical
            };
            txtComment.Name = "txtComment";

            AddField(table, 0, "Студент *", cmbStudents);
            AddField(table, 1, "Дисциплина *", cmbSubjects);
            AddField(table, 2, "Семестр *", cmbSemesters);
            AddField(table, 3, "Оценка *", numGrade);
            AddField(table, 4, "Дата выставления *", dtpGradeDate);
            AddField(table, 5, "Комментарий", txtComment);

            content.Controls.Add(table);

            Panel buttons = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = UiTheme.Surface };
            FlowLayoutPanel flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 300,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 12, 0, 0)
            };

            btnSave = UiTheme.PrimaryButton("Сохранить");
            btnSave.Width = 140;
            btnSave.Click += btnSave_Click;
            flow.Controls.Add(btnSave);

            Button btnCancel = UiTheme.SecondaryButton("Отмена");
            btnCancel.Width = 120;
            btnCancel.DialogResult = DialogResult.Cancel;
            flow.Controls.Add(btnCancel);

            buttons.Controls.Add(flow);
            content.Controls.Add(buttons);

            Controls.Add(content);
            Controls.Add(header);

            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void AddField(TableLayoutPanel table, int row, string caption, Control control)
        {
            Label label = new Label
            {
                Text = caption,
                Font = UiTheme.BaseFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 12, 8, 6)
            };

            control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            control.Margin = new Padding(0, 8, 0, 6);

            table.Controls.Add(label, 0, row);
            table.Controls.Add(control, 1, row);
        }

        private void LoadStudents()
        {
            string query = "SELECT StudentID, LastName + ' ' + FirstName + ' ' + MiddleName + " +
                           "' (' + g.GroupName + ')' AS FullName " +
                           "FROM Students s INNER JOIN Groups g ON s.GroupID = g.GroupID " +
                           "ORDER BY LastName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                cmbStudents.DataSource = dt;
                cmbStudents.DisplayMember = "FullName";
                cmbStudents.ValueMember = "StudentID";
                cmbStudents.SelectedIndex = -1;
            }
        }

        private void LoadSubjects()
        {
            List<Subject> subjects = new LookupRepository().GetSubjects();
            cmbSubjects.DataSource = subjects;
            cmbSubjects.DisplayMember = "SubjectName";
            cmbSubjects.ValueMember = "SubjectID";
            cmbSubjects.SelectedIndex = -1;
        }

        private void LoadSemesters()
        {
            List<Semester> semesters = new LookupRepository().GetSemesters();
            cmbSemesters.DataSource = semesters;
            cmbSemesters.DisplayMember = "SemesterName";
            cmbSemesters.ValueMember = "SemesterID";
            cmbSemesters.SelectedIndex = -1;
        }

        private void LoadGrade(int id)
        {
            Grade grade = repository.GetGradeById(id);
            if (grade == null)
                return;

            cmbStudents.SelectedValue = grade.StudentID;
            cmbSubjects.SelectedValue = grade.SubjectID;
            cmbSemesters.SelectedValue = grade.SemesterID;
            numGrade.Value = grade.GradeValue;
            if (grade.GradeDate <= dtpGradeDate.MaxDate && grade.GradeDate >= dtpGradeDate.MinDate)
                dtpGradeDate.Value = grade.GradeDate;
            txtComment.Text = grade.Comment;
        }

        private bool ValidateFields()
        {
            bool isValid = true;
            errorProvider.Clear();

            if (cmbStudents.SelectedIndex == -1)
            {
                errorProvider.SetError(cmbStudents, "Выберите студента!");
                isValid = false;
            }

            if (cmbSubjects.SelectedIndex == -1)
            {
                errorProvider.SetError(cmbSubjects, "Выберите дисциплину!");
                isValid = false;
            }

            if (cmbSemesters.SelectedIndex == -1)
            {
                errorProvider.SetError(cmbSemesters, "Выберите семестр!");
                isValid = false;
            }

            if (numGrade.Value < 2 || numGrade.Value > 5)
            {
                errorProvider.SetError(numGrade, "Оценка должна быть от 2 до 5!");
                isValid = false;
            }

            if (dtpGradeDate.Value.Date > DateTime.Today)
            {
                errorProvider.SetError(dtpGradeDate, "Дата выставления не может быть в будущем!");
                isValid = false;
            }

            return isValid;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateFields())
                return;

            try
            {
                Grade grade = isNew ? new Grade() : repository.GetGradeById(gradeID.Value);
                if (grade == null)
                    return;

                grade.StudentID = Convert.ToInt32(cmbStudents.SelectedValue);
                grade.SubjectID = Convert.ToInt32(cmbSubjects.SelectedValue);
                grade.SemesterID = Convert.ToInt32(cmbSemesters.SelectedValue);
                grade.GradeValue = (int)numGrade.Value;
                grade.GradeDate = dtpGradeDate.Value.Date;
                grade.Comment = txtComment.Text.Trim();

                if (isNew)
                {
                    grade.TeacherID = currentUser.UserID;
                    int newID = repository.AddGrade(grade);
                    Logger.Info($"Выставлена оценка GradeID={newID}, студент={grade.StudentID}, " +
                                $"дисциплина={grade.SubjectID}, значение={grade.GradeValue}");
                }
                else
                {
                    repository.UpdateGrade(grade);
                    Logger.Info($"Изменена оценка GradeID={grade.GradeID}, значение={grade.GradeValue}");
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения оценки", ex);
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
