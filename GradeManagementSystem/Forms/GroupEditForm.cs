using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using GradeManagementSystem.Helpers;

namespace GradeManagementSystem.Forms
{
    public class GroupEditForm : Form
    {
        private readonly string connectionString = AppConfig.ConnectionString;
        private readonly int? groupID;
        private readonly bool isNew;
        private readonly ErrorProvider errorProvider = new ErrorProvider();

        private TextBox txtGroupName;
        private NumericUpDown numCourse;
        private TextBox txtSpecialty;
        private Button btnSave;

        public GroupEditForm(int? groupID)
        {
            this.groupID = groupID;
            isNew = !groupID.HasValue;

            UiTheme.ApplyForm(this);
            Text = isNew ? "Новая учебная группа" : "Редактирование учебной группы";
            ClientSize = new Size(540, 330);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            BuildUi();

            if (!isNew)
                LoadGroup(groupID.Value);
        }

        private void BuildUi()
        {
            Panel header = UiTheme.Header(isNew ? "Новая учебная группа" : "Редактирование группы",
                                          "Укажите название, курс и специальность");
            header.Dock = DockStyle.Top;

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
                RowCount = 3,
                BackColor = UiTheme.Surface
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtGroupName = UiTheme.TextBox(300);
            numCourse = new NumericUpDown
            {
                Font = UiTheme.BaseFont,
                Minimum = 1,
                Maximum = 5,
                Value = AppConfig.DefaultCourse,
                Width = 80
            };
            txtSpecialty = UiTheme.TextBox(300);

            AddField(table, 0, "Название группы *", txtGroupName);
            AddField(table, 1, "Курс *", numCourse);
            AddField(table, 2, "Специальность *", txtSpecialty);

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

        private void LoadGroup(int id)
        {
            string query = "SELECT GroupName, Course, Specialty FROM Groups WHERE GroupID = @GroupID";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@GroupID", id);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtGroupName.Text = reader["GroupName"].ToString();
                            numCourse.Value = Convert.ToInt32(reader["Course"]);
                            txtSpecialty.Text = reader["Specialty"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Не удалось загрузить группу GroupID={id}", ex);
            }
        }

        private bool ValidateFields()
        {
            bool isValid = true;
            errorProvider.Clear();

            if (string.IsNullOrWhiteSpace(txtGroupName.Text))
            {
                errorProvider.SetError(txtGroupName, "Введите название группы!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtSpecialty.Text))
            {
                errorProvider.SetError(txtSpecialty, "Введите специальность!");
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
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = isNew
                        ? "INSERT INTO Groups (GroupName, Course, Specialty) " +
                          "VALUES (@GroupName, @Course, @Specialty)"
                        : "UPDATE Groups SET GroupName = @GroupName, Course = @Course, " +
                          "Specialty = @Specialty WHERE GroupID = @GroupID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@GroupName", txtGroupName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Course", (int)numCourse.Value);
                        cmd.Parameters.AddWithValue("@Specialty", txtSpecialty.Text.Trim());

                        if (!isNew)
                            cmd.Parameters.AddWithValue("@GroupID", groupID.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                Logger.Info(isNew
                    ? "Добавлена новая учебная группа"
                    : $"Изменена учебная группа GroupID={groupID}");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                Logger.Warning("Попытка сохранить группу с неуникальным названием");
                errorProvider.SetError(txtGroupName, "Группа с таким названием уже существует!");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения учебной группы", ex);
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
