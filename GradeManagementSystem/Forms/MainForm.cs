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
    public class MainForm : Form
    {
        private readonly User currentUser;
        private readonly LookupRepository lookupRepository = new LookupRepository();

        private MenuStrip menuStrip;
        private ToolStripMenuItem menuDirectories;
        private ToolStripMenuItem menuStudents;
        private ToolStripMenuItem menuGroups;
        private ToolStripMenuItem menuGrades;
        private ToolStripMenuItem menuStatistics;
        private ToolStripMenuItem menuCharts;
        private ToolStripMenuItem menuReports;
        private ToolStripMenuItem menuExport;

        private Label lblStudentsValue;
        private Label lblGradesValue;
        private Label lblAverageValue;
        private DataGridView dgvRecent;

        public MainForm(User user)
        {
            currentUser = user;
            UiTheme.ApplyForm(this);
            Text = $"Система учёта успеваемости — {currentUser.FullName} ({GetRoleName(currentUser.Role)})";
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);

            BuildMenu();
            BuildUi();
            SetupMenuByRole();
            RefreshDashboard();
            LoadRecentGrades();
        }

        private string GetRoleName(string role)
        {
            switch (role)
            {
                case "Admin": return "Администратор";
                case "Curator": return "Куратор";
                case "Teacher": return "Преподаватель";
                default: return "Пользователь";
            }
        }

        private void BuildMenu()
        {
            menuStrip = new MenuStrip
            {
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5F),
                Padding = new Padding(10, 3, 0, 3)
            };

            menuDirectories = new ToolStripMenuItem("Справочники");

            menuStudents = new ToolStripMenuItem("Студенты");
            menuStudents.Click += (s, e) => OpenStudents();

            menuGroups = new ToolStripMenuItem("Учебные группы");
            menuGroups.Click += (s, e) => OpenGroups();

            menuDirectories.DropDownItems.Add(menuStudents);
            menuDirectories.DropDownItems.Add(menuGroups);

            menuGrades = new ToolStripMenuItem("Оценки");
            menuGrades.Click += (s, e) => OpenGrades();

            menuStatistics = new ToolStripMenuItem("Статистика");
            ToolStripMenuItem statisticsItem = new ToolStripMenuItem("Расчёт статистики");
            statisticsItem.Click += (s, e) => OpenStatistics();
            menuCharts = new ToolStripMenuItem("Графики");
            menuCharts.Click += (s, e) => OpenCharts();
            menuStatistics.DropDownItems.Add(statisticsItem);
            menuStatistics.DropDownItems.Add(menuCharts);

            menuReports = new ToolStripMenuItem("Отчёты");
            menuReports.Click += (s, e) => OpenReports();

            menuExport = new ToolStripMenuItem("Экспорт");
            ToolStripMenuItem exportReport = new ToolStripMenuItem("Ведомость в CSV");
            exportReport.Click += (s, e) => OpenReports();
            ToolStripMenuItem exportStatistics = new ToolStripMenuItem("Статистика в CSV");
            exportStatistics.Click += (s, e) => OpenStatistics();
            menuExport.DropDownItems.Add(exportReport);
            menuExport.DropDownItems.Add(exportStatistics);

            ToolStripMenuItem menuHelp = new ToolStripMenuItem("Справка");
            menuHelp.Click += (s, e) => MessageBox.Show(
                "Система учёта успеваемости\nУчебный проект, 2026 г.\n\nРоли: администратор, куратор, преподаватель.",
                "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ToolStripMenuItem menuExit = new ToolStripMenuItem("Выход");
            menuExit.Click += (s, e) => Close();

            menuStrip.Items.Add(menuDirectories);
            menuStrip.Items.Add(menuGrades);
            menuStrip.Items.Add(menuStatistics);
            menuStrip.Items.Add(menuReports);
            menuStrip.Items.Add(menuExport);
            menuStrip.Items.Add(new ToolStripSeparator());
            menuStrip.Items.Add(menuHelp);
            menuStrip.Items.Add(menuExit);

            MainMenuStrip = menuStrip;
        }

        private void BuildUi()
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 86,
                BackColor = UiTheme.Accent,
                Padding = new Padding(24, 12, 24, 10)
            };

            Label title = new Label
            {
                Text = "Система учёта успеваемости",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(24, 12)
            };
            header.Controls.Add(title);

            Label subtitle = new Label
            {
                Text = "Учёт оценок, статистический анализ и отчётность по успеваемости студентов",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(216, 236, 244),
                AutoSize = true,
                Location = new Point(26, 50)
            };
            header.Controls.Add(subtitle);

            Label userInfo = new Label
            {
                Text = $"{currentUser.FullName}\n{GetRoleName(currentUser.Role)}  ·  {DateTime.Now:dd.MM.yyyy}",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Right,
                Width = 340,
                Padding = new Padding(0, 0, 6, 0)
            };
            header.Controls.Add(userInfo);

            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Background,
                Padding = new Padding(18, 14, 18, 14)
            };

            TableLayoutPanel cards = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 108,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = UiTheme.Background,
                Margin = new Padding(0)
            };
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

            Panel cardStudents = CreateStatCard("Студентов в системе", "0", "человек", UiTheme.Accent, out lblStudentsValue);
            Panel cardGrades = CreateStatCard("Выставленных оценок", "0", "записей в журнале", Color.FromArgb(22, 128, 74), out lblGradesValue);
            Panel cardAverage = CreateStatCard("Средний балл по заведению", "0,00", "по всем оценкам", Color.FromArgb(176, 106, 0), out lblAverageValue);

            cards.Controls.Add(cardStudents, 0, 0);
            cards.Controls.Add(cardGrades, 1, 0);
            cards.Controls.Add(cardAverage, 2, 0);

            FlowLayoutPanel actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 54,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 0),
                BackColor = UiTheme.Background
            };

            Button btnGrades = UiTheme.PrimaryButton("Журнал оценок");
            btnGrades.Width = 168;
            btnGrades.Click += (s, e) => OpenGrades();

            Button btnStudents = UiTheme.SecondaryButton("Справочник студентов");
            btnStudents.Width = 190;
            btnStudents.Click += (s, e) => OpenStudents();

            Button btnStatistics = UiTheme.SecondaryButton("Статистика");
            btnStatistics.Width = 150;
            btnStatistics.Click += (s, e) => OpenStatistics();

            Button btnReports = UiTheme.SecondaryButton("Ведомость за период");
            btnReports.Width = 180;
            btnReports.Click += (s, e) => OpenReports();

            actions.Controls.Add(btnGrades);
            actions.Controls.Add(btnStudents);
            actions.Controls.Add(btnStatistics);
            actions.Controls.Add(btnReports);

            GroupBox recentBox = new GroupBox
            {
                Text = "Последние выставленные оценки",
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Font = UiTheme.SectionFont,
                ForeColor = UiTheme.TextPrimary,
                Padding = new Padding(12, 10, 12, 12)
            };

            dgvRecent = UiTheme.Grid();
            dgvRecent.Dock = DockStyle.Fill;
            recentBox.Controls.Add(dgvRecent);

            content.Controls.Add(recentBox);
            content.Controls.Add(actions);
            content.Controls.Add(cards);

            StatusStrip status = new StatusStrip { BackColor = Color.White, SizingGrip = false };
            status.Items.Add(new ToolStripStatusLabel($"Подключение: {AppConfig.ConnectionString}"));
            status.Items.Add(new ToolStripStatusLabel("Готово") { Spring = true, TextAlign = ContentAlignment.MiddleRight });

            Controls.Add(content);
            Controls.Add(header);
            Controls.Add(menuStrip);
            Controls.Add(status);
        }

        private Panel CreateStatCard(string caption, string value, string note, Color accent, out Label valueLabel)
        {
            Panel card = new Panel
            {
                BackColor = UiTheme.Surface,
                Margin = new Padding(6, 0, 6, 0),
                Padding = new Padding(18, 12, 12, 10),
                Dock = DockStyle.Fill
            };

            Panel stripe = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accent };
            card.Controls.Add(stripe);

            Label captionLabel = new Label
            {
                Text = caption,
                Font = new Font("Segoe UI", 9F),
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(20, 12)
            };
            card.Controls.Add(captionLabel);

            valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = accent,
                AutoSize = true,
                Location = new Point(18, 32)
            };
            card.Controls.Add(valueLabel);

            Label noteLabel = new Label
            {
                Text = note,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(21, 78)
            };
            card.Controls.Add(noteLabel);

            return card;
        }

        private void SetupMenuByRole()
        {
            bool isAdmin = currentUser.Role == "Admin";
            bool isCurator = currentUser.Role == "Curator";
            bool isTeacher = currentUser.Role == "Teacher";

            menuDirectories.Visible = isAdmin;
            menuStudents.Visible = isAdmin;
            menuGroups.Visible = isAdmin;
            menuGrades.Visible = isAdmin || isTeacher;
            menuStatistics.Visible = isAdmin || isCurator;
            menuCharts.Visible = isAdmin || isCurator;
            menuReports.Visible = true;
            menuExport.Visible = true;

            Logger.Info($"Настроено меню для роли {currentUser.Role}: " +
                        $"справочники={menuDirectories.Visible}, оценки={menuGrades.Visible}, " +
                        $"статистика={menuStatistics.Visible}");
        }

        private void RefreshDashboard()
        {
            try
            {
                lblStudentsValue.Text = lookupRepository.GetStudentsCount().ToString();
                lblGradesValue.Text = lookupRepository.GetGradesCount().ToString();
                lblAverageValue.Text = lookupRepository.GetOverallAverage().ToString("F2");
            }
            catch (Exception ex)
            {
                Logger.Error("Не удалось загрузить показатели главной формы", ex);
            }
        }

        private void LoadRecentGrades()
        {
            string query = "SELECT TOP 12 s.LastName + ' ' + s.FirstName + ' ' + s.MiddleName AS [Студент], " +
                           "sub.SubjectName AS [Предмет], g.GradeValue AS [Оценка], " +
                           "g.GradeDate AS [Дата], gr.GroupName AS [Группа] " +
                           "FROM Grades g " +
                           "INNER JOIN Students s ON g.StudentID = s.StudentID " +
                           "INNER JOIN Groups gr ON s.GroupID = gr.GroupID " +
                           "INNER JOIN Subjects sub ON g.SubjectID = sub.SubjectID " +
                           "ORDER BY g.GradeDate DESC, g.GradeID DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(AppConfig.ConnectionString))
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvRecent.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Не удалось загрузить последние оценки", ex);
            }
        }

        private void OpenStudents()
        {
            using (StudentsForm form = new StudentsForm())
                form.ShowDialog(this);
            RefreshDashboard();
        }

        private void OpenGroups()
        {
            using (GroupsForm form = new GroupsForm())
                form.ShowDialog(this);
            RefreshDashboard();
        }

        private void OpenGrades()
        {
            using (GradesForm form = new GradesForm(currentUser))
                form.ShowDialog(this);
            RefreshDashboard();
            LoadRecentGrades();
        }

        private void OpenStatistics()
        {
            using (StatisticsForm form = new StatisticsForm(currentUser))
                form.ShowDialog(this);
        }

        private void OpenCharts()
        {
            using (VisualizationForm form = new VisualizationForm(currentUser))
                form.ShowDialog(this);
        }

        private void OpenReports()
        {
            using (ReportForm form = new ReportForm(currentUser))
                form.ShowDialog(this);
        }
    }
}
