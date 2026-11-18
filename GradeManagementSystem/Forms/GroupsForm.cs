using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using GradeManagementSystem.Data;
using GradeManagementSystem.Helpers;

namespace GradeManagementSystem.Forms
{
    public class GroupsForm : Form
    {
        private readonly string connectionString = AppConfig.ConnectionString;
        private readonly LookupRepository lookupRepository = new LookupRepository();

        private DataGridView dgvGroups;
        private Label lblCount;

        public GroupsForm()
        {
            UiTheme.ApplyForm(this);
            Text = "Справочник учебных групп";
            ClientSize = new Size(980, 580);
            MinimumSize = new Size(820, 480);

            BuildUi();
            LoadGroups();
        }

        private void BuildUi()
        {
            Panel header = UiTheme.Header("Справочник учебных групп",
                                          "Управление перечнем групп, курсов и специальностей");
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

            Button btnAdd = UiTheme.PrimaryButton("Добавить");
            btnAdd.Margin = new Padding(0, 5, 6, 0);
            btnAdd.Click += (s, e) => OpenEditor(null);
            toolbar.Controls.Add(btnAdd);

            Button btnEdit = UiTheme.SecondaryButton("Редактировать");
            btnEdit.Width = 150;
            btnEdit.Margin = new Padding(0, 5, 6, 0);
            btnEdit.Click += (s, e) => EditSelected();
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
                Text = "Учебные группы",
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Font = UiTheme.SectionFont,
                ForeColor = UiTheme.TextPrimary,
                Padding = new Padding(12, 10, 12, 12)
            };

            dgvGroups = UiTheme.Grid();
            dgvGroups.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                    EditSelected();
            };
            box.Controls.Add(dgvGroups);

            Panel statusPanel = new Panel { Dock = DockStyle.Bottom, Height = 26, BackColor = UiTheme.Background };
            lblCount = new Label
            {
                Text = "Групп: 0",
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

        public void LoadGroups()
        {
            string query = "SELECT g.GroupID, g.GroupName, g.Course, g.Specialty, " +
                           "COUNT(s.StudentID) AS [Студентов] " +
                           "FROM Groups g LEFT JOIN Students s ON s.GroupID = g.GroupID " +
                           "GROUP BY g.GroupID, g.GroupName, g.Course, g.Specialty " +
                           "ORDER BY g.GroupName";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvGroups.DataSource = dt;

                    if (dgvGroups.Columns.Contains("GroupID"))
                        dgvGroups.Columns["GroupID"].Visible = false;
                    if (dgvGroups.Columns.Contains("GroupName"))
                        dgvGroups.Columns["GroupName"].HeaderText = "Группа";
                    if (dgvGroups.Columns.Contains("Course"))
                        dgvGroups.Columns["Course"].HeaderText = "Курс";
                    if (dgvGroups.Columns.Contains("Specialty"))
                        dgvGroups.Columns["Specialty"].HeaderText = "Специальность";

                    lblCount.Text = $"Групп: {dt.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки списка групп", ex);
                MessageBox.Show($"Не удалось загрузить список групп: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenEditor(int? groupID)
        {
            using (GroupEditForm form = new GroupEditForm(groupID))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                    LoadGroups();
            }
        }

        private void EditSelected()
        {
            if (dgvGroups.CurrentRow == null)
            {
                MessageBox.Show("Выберите учебную группу.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int groupID = Convert.ToInt32(dgvGroups.CurrentRow.Cells["GroupID"].Value);
            OpenEditor(groupID);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvGroups.CurrentRow == null)
            {
                MessageBox.Show("Выберите учебную группу.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int groupID = Convert.ToInt32(dgvGroups.CurrentRow.Cells["GroupID"].Value);
            string groupName = dgvGroups.CurrentRow.Cells["GroupName"].Value.ToString();
            int studentsCount = lookupRepository.GetStudentsCountForGroup(groupID);

            if (studentsCount > 0)
            {
                MessageBox.Show(
                    $"Невозможно удалить группу «{groupName}», так как в ней числятся студенты ({studentsCount} чел.).\n" +
                    "Сначала переведите студентов в другую группу.",
                    "Удаление запрещено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить группу «{groupName}»?", "Подтверждение удаления",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Groups WHERE GroupID = @GroupID", conn))
                {
                    cmd.Parameters.AddWithValue("@GroupID", groupID);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                Logger.Info($"Удалена учебная группа GroupID={groupID}");
                LoadGroups();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                Logger.Warning($"Попытка удаления связанной группы GroupID={groupID}");
                MessageBox.Show("Удаление отклонено сервером базы данных: на группу ссылаются другие записи.",
                    "Ограничение целостности", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка удаления группы GroupID={groupID}", ex);
                MessageBox.Show($"Не удалось удалить группу: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
