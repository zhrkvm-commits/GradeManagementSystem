using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using GradeManagementSystem.Data;
using GradeManagementSystem.Helpers;
using GradeManagementSystem.Models;

namespace GradeManagementSystem.Forms
{
    public class StudentEditForm : Form
    {
        private readonly string connectionString = AppConfig.ConnectionString;
        private readonly int? studentID;
        private readonly bool isNew;
        private readonly ErrorProvider errorProvider = new ErrorProvider();

        private TextBox txtLastName;
        private TextBox txtFirstName;
        private TextBox txtMiddleName;
        private DateTimePicker dtpBirthDate;
        private ComboBox cmbGroups;
        private Button btnSave;

        public StudentEditForm(int? studentID)
        {
            this.studentID = studentID;
            isNew = !studentID.HasValue;

            UiTheme.ApplyForm(this);
            Text = isNew ? "Новый студент" : "Редактирование студента";
            ClientSize = new Size(560, 430);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            BuildUi();
            LoadGroups();

            if (!isNew)
                LoadStudent(studentID.Value);
        }

        private void BuildUi()
        {
            Panel header = UiTheme.Header(isNew ? "Новый студент" : "Редактирование студента",
                                          "Заполните обязательные поля и выберите учебную группу");
            header.Dock = DockStyle.Top;
            header.Height = 78;

            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Padding = new Padding(24, 14, 24, 10)
            };

            TableLayoutPanel table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                BackColor = UiTheme.Surface
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtLastName = UiTheme.TextBox(320);
            txtLastName.Name = "txtLastName";
            txtFirstName = UiTheme.TextBox(320);
            txtFirstName.Name = "txtFirstName";
            txtMiddleName = UiTheme.TextBox(320);
            txtMiddleName.Name = "txtMiddleName";

            dtpBirthDate = new DateTimePicker
            {
                Font = UiTheme.BaseFont,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy",
                Width = 180,
                MinDate = new DateTime(1900, 1, 1),
                MaxDate = DateTime.Today
            };
            dtpBirthDate.Name = "dtpBirthDate";
            dtpBirthDate.Value = new DateTime(2004, 1, 1);

            cmbGroups = UiTheme.Combo(320);
            cmbGroups.Name = "cmbGroups";

            AddField(table, 0, "Фамилия *", txtLastName);
            AddField(table, 1, "Имя *", txtFirstName);
            AddField(table, 2, "Отчество *", txtMiddleName);
            AddField(table, 3, "Дата рождения *", dtpBirthDate);
            AddField(table, 4, "Учебная группа *", cmbGroups);

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

        private void LoadGroups()
        {
            try
            {
                LookupRepository repository = new LookupRepository();
                List<Group> groups = repository.GetGroups();

                cmbGroups.DataSource = groups;
                cmbGroups.DisplayMember = "GroupName";
                cmbGroups.ValueMember = "GroupID";
                cmbGroups.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                Logger.Error("Не удалось загрузить список учебных групп", ex);
            }
        }

        private void LoadStudent(int id)
        {
            string query = "SELECT StudentID, LastName, FirstName, MiddleName, BirthDate, GroupID " +
                           "FROM Students WHERE StudentID = @StudentID";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentID", id);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtLastName.Text = reader["LastName"].ToString();
                            txtFirstName.Text = reader["FirstName"].ToString();
                            txtMiddleName.Text = reader["MiddleName"].ToString();

                            DateTime birthDate = Convert.ToDateTime(reader["BirthDate"]);
                            if (birthDate >= dtpBirthDate.MinDate && birthDate <= dtpBirthDate.MaxDate)
                                dtpBirthDate.Value = birthDate;

                            cmbGroups.SelectedValue = Convert.ToInt32(reader["GroupID"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Не удалось загрузить студента StudentID={id}", ex);
                MessageBox.Show($"Не удалось загрузить данные студента: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateFields()
        {
            bool isValid = true;
            errorProvider.Clear();

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                errorProvider.SetError(txtLastName, "Введите фамилию!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                errorProvider.SetError(txtFirstName, "Введите имя!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtMiddleName.Text))
            {
                errorProvider.SetError(txtMiddleName, "Введите отчество!");
                isValid = false;
            }

            if (dtpBirthDate.Value >= DateTime.Now.Date)
            {
                errorProvider.SetError(dtpBirthDate, "Дата рождения должна быть в прошлом!");
                isValid = false;
            }

            if (cmbGroups.SelectedIndex == -1)
            {
                errorProvider.SetError(cmbGroups, "Выберите учебную группу!");
                isValid = false;
            }

            return isValid;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateFields())
            {
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query;

                    if (isNew)
                    {
                        query = "INSERT INTO Students (LastName, FirstName, MiddleName, BirthDate, GroupID) " +
                                "VALUES (@LastName, @FirstName, @MiddleName, @BirthDate, @GroupID)";
                    }
                    else
                    {
                        query = "UPDATE Students SET LastName = @LastName, FirstName = @FirstName, " +
                                "MiddleName = @MiddleName, BirthDate = @BirthDate, GroupID = @GroupID " +
                                "WHERE StudentID = @StudentID";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@MiddleName", txtMiddleName.Text.Trim());
                        cmd.Parameters.AddWithValue("@BirthDate", dtpBirthDate.Value.Date);
                        cmd.Parameters.AddWithValue("@GroupID", cmbGroups.SelectedValue);

                        if (!isNew)
                        {
                            cmd.Parameters.AddWithValue("@StudentID", studentID.Value);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }

                Logger.Info(isNew
                    ? "Добавлен новый студент"
                    : $"Изменены данные студента StudentID={studentID}");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения студента", ex);
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
