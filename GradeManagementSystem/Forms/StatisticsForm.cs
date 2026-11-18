using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GradeManagementSystem.Data;
using GradeManagementSystem.Helpers;
using GradeManagementSystem.Models;
using GradeManagementSystem.Services;

namespace GradeManagementSystem.Forms
{
    public class StatisticsForm : Form
    {
        private readonly User currentUser;
        private readonly GradeRepository repository = new GradeRepository();
        private readonly LookupRepository lookupRepository = new LookupRepository();

        private ComboBox cmbGroups;
        private ComboBox cmbSemesters;
        private ComboBox cmbSubjects;
        private DataGridView dgvStudents;

        private Label lblAverage;
        private Label lblVariance;
        private Label lblStdDev;
        private Label lblMedian;
        private Label lblMin;
        private Label lblMax;
        private Label lblCount;
        private Label lblWorkingDays;
        private Label lblGradesPerDay;
        private Button btnCalculate;
        private Button btnLoadCalendar;

        public StatisticsForm(User user)
        {
            currentUser = user;
            UiTheme.ApplyForm(this);
            Text = "Статистика успеваемости";
            ClientSize = new Size(1180, 720);
            MinimumSize = new Size(1000, 620);

            BuildUi();
            LoadLookups();
            ApplyRoleRestrictions();
        }

        private void BuildUi()
        {
            Panel header = UiTheme.Header("Статистика успеваемости",
                                          "Средний балл, дисперсия, медиана и детализация по студентам");
            header.Dock = DockStyle.Top;

            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Background,
                Padding = new Padding(18, 12, 18, 12)
            };

            GroupBox filters = new GroupBox
            {
                Text = "Параметры расчёта",
                Dock = DockStyle.Top,
                Height = 82,
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

            cmbGroups = UiTheme.Combo(170);
            cmbSemesters = UiTheme.Combo(190);
            cmbSubjects = UiTheme.Combo(200);

            filterFlow.Controls.Add(CreateFilterCell("Учебная группа", cmbGroups));
            filterFlow.Controls.Add(CreateFilterCell("Семестр", cmbSemesters));
            filterFlow.Controls.Add(CreateFilterCell("Дисциплина", cmbSubjects));

            btnCalculate = UiTheme.PrimaryButton("Рассчитать");
            btnCalculate.Width = 140;
            btnCalculate.Margin = new Padding(8, 24, 6, 0);
            btnCalculate.Click += btnCalculate_Click;
            filterFlow.Controls.Add(btnCalculate);

            Button btnExport = UiTheme.SecondaryButton("Экспорт в CSV");
            btnExport.Width = 150;
            btnExport.Margin = new Padding(0, 24, 0, 0);
            btnExport.Click += btnExport_Click;
            filterFlow.Controls.Add(btnExport);

            filters.Controls.Add(filterFlow);

            TableLayoutPanel metrics = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 104,
                ColumnCount = 7,
                RowCount = 1,
                BackColor = UiTheme.Background,
                Padding = new Padding(0, 8, 0, 0)
            };
            for (int i = 0; i < 7; i++)
                metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857F));

            metrics.Controls.Add(CreateMetricCard("Средний балл", "0,00", UiTheme.Accent, out lblAverage), 0, 0);
            metrics.Controls.Add(CreateMetricCard("Дисперсия", "0,00", UiTheme.TextPrimary, out lblVariance), 1, 0);
            metrics.Controls.Add(CreateMetricCard("Ст. отклонение", "0,00", UiTheme.TextPrimary, out lblStdDev), 2, 0);
            metrics.Controls.Add(CreateMetricCard("Медиана", "0,00", UiTheme.TextPrimary, out lblMedian), 3, 0);
            metrics.Controls.Add(CreateMetricCard("Минимум", "—", UiTheme.TextPrimary, out lblMin), 4, 0);
            metrics.Controls.Add(CreateMetricCard("Максимум", "—", UiTheme.TextPrimary, out lblMax), 5, 0);
            metrics.Controls.Add(CreateMetricCard("Оценок", "0", UiTheme.TextPrimary, out lblCount), 6, 0);

            Panel calendarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = UiTheme.Surface,
                Padding = new Padding(14, 8, 14, 6)
            };

            Label calendarCaption = new Label
            {
                Text = "Производственный календарь:",
                Font = UiTheme.BaseFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(14, 14)
            };
            calendarPanel.Controls.Add(calendarCaption);

            lblWorkingDays = new Label
            {
                Text = "учебных дней: —",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(190, 14)
            };
            calendarPanel.Controls.Add(lblWorkingDays);

            lblGradesPerDay = new Label
            {
                Text = "оценок за учебный день: —",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(340, 14)
            };
            calendarPanel.Controls.Add(lblGradesPerDay);

            btnLoadCalendar = UiTheme.SecondaryButton("Загрузить календарь");
            btnLoadCalendar.Width = 180;
            btnLoadCalendar.Dock = DockStyle.Right;
            btnLoadCalendar.Click += btnLoadCalendar_Click;
            calendarPanel.Controls.Add(btnLoadCalendar);

            GroupBox studentsBox = new GroupBox
            {
                Text = "Статистика по студентам",
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Font = UiTheme.SectionFont,
                ForeColor = UiTheme.TextPrimary,
                Padding = new Padding(12, 10, 12, 12)
            };

            dgvStudents = UiTheme.Grid();
            studentsBox.Controls.Add(dgvStudents);

            content.Controls.Add(studentsBox);
            content.Controls.Add(calendarPanel);
            content.Controls.Add(metrics);
            content.Controls.Add(filters);

            Controls.Add(content);
            Controls.Add(header);
        }

        private Panel CreateFilterCell(string caption, Control control)
        {
            Panel cell = new Panel
            {
                Width = control.Width + 8,
                Height = 56,
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

        private Panel CreateMetricCard(string caption, string value, Color color, out Label valueLabel)
        {
            Panel card = new Panel
            {
                BackColor = UiTheme.Surface,
                Margin = new Padding(4, 0, 4, 0),
                Padding = new Padding(12, 10, 8, 8),
                Dock = DockStyle.Fill
            };

            Panel stripe = new Panel { Dock = DockStyle.Top, Height = 3, BackColor = color };
            card.Controls.Add(stripe);

            Label captionLabel = new Label
            {
                Text = caption,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(12, 12)
            };
            card.Controls.Add(captionLabel);

            valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = color,
                AutoSize = true,
                Location = new Point(10, 36)
            };
            card.Controls.Add(valueLabel);

            return card;
        }

        private void LoadLookups()
        {
            var groups = lookupRepository.GetGroups();
            cmbGroups.DataSource = groups;
            cmbGroups.DisplayMember = "GroupName";
            cmbGroups.ValueMember = "GroupID";
            cmbGroups.SelectedIndex = -1;

            var semesters = lookupRepository.GetSemesters();
            cmbSemesters.DataSource = semesters;
            cmbSemesters.DisplayMember = "SemesterName";
            cmbSemesters.ValueMember = "SemesterID";
            cmbSemesters.SelectedIndex = -1;

            var subjects = lookupRepository.GetSubjects();
            cmbSubjects.DataSource = subjects;
            cmbSubjects.DisplayMember = "SubjectName";
            cmbSubjects.ValueMember = "SubjectID";
            cmbSubjects.SelectedIndex = -1;
        }

        private void ApplyRoleRestrictions()
        {
            if (!currentUser.IsCurator)
                return;

            if (currentUser.GroupID.HasValue)
                cmbGroups.SelectedValue = currentUser.GroupID.Value;

            cmbGroups.Enabled = false;
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            int? groupID = cmbGroups.SelectedIndex != -1 ? (int?)cmbGroups.SelectedValue : null;
            int? semesterID = cmbSemesters.SelectedIndex != -1 ? (int?)cmbSemesters.SelectedValue : null;
            int? subjectID = cmbSubjects.SelectedIndex != -1 ? (int?)cmbSubjects.SelectedValue : null;

            if (currentUser.IsCurator)
                groupID = currentUser.GroupID;

            try
            {
                List<int> grades = repository.GetGradesForStatistics(groupID, null, subjectID, semesterID);

                if (grades.Count == 0)
                {
                    MessageBox.Show("Нет данных для расчёта статистики!", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                double average = MathStatistics.CalculateAverage(grades);
                double variance = MathStatistics.CalculateVariance(grades);
                double stdDev = MathStatistics.CalculateStandardDeviation(grades);
                double median = MathStatistics.CalculateMedian(grades);
                int minGrade = grades.Min();
                int maxGrade = grades.Max();

                lblAverage.Text = average.ToString("F2");
                lblVariance.Text = variance.ToString("F2");
                lblStdDev.Text = stdDev.ToString("F2");
                lblMedian.Text = median.ToString("F2");
                lblMin.Text = minGrade.ToString();
                lblMax.Text = maxGrade.ToString();
                lblCount.Text = grades.Count.ToString();

                Color averageColor;
                if (average >= 4.5)
                    averageColor = UiTheme.Success;
                else if (average >= 3.0)
                    averageColor = UiTheme.Warning;
                else
                    averageColor = UiTheme.Danger;

                lblAverage.ForeColor = averageColor;

                dgvStudents.DataSource = repository.GetStudentStatistics(groupID, semesterID);
                if (dgvStudents.Columns.Contains("StudentID"))
                    dgvStudents.Columns["StudentID"].Visible = false;

                Logger.Info($"Рассчитана статистика: оценок={grades.Count}, средний балл={average:F2}, " +
                            $"дисперсия={variance:F2}");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка расчёта статистики", ex);
                MessageBox.Show($"Не удалось рассчитать статистику: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnLoadCalendar_Click(object sender, EventArgs e)
        {
            btnLoadCalendar.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                AcademicCalendarService service = new AcademicCalendarService();
                DateTime month = cmbSemesters.SelectedIndex != -1
                    ? ((Semester)cmbSemesters.SelectedItem).StartDate
                    : DateTime.Today;

                int workingDays = await service.GetWorkingDaysCountAsync(month.Year, month.Month);
                lblWorkingDays.Text = $"учебных дней: {workingDays}";

                int gradesCount;
                int.TryParse(lblCount.Text, out gradesCount);

                double gradesPerDay = workingDays > 0
                    ? Math.Round((double)gradesCount / workingDays, 2)
                    : 0;

                lblGradesPerDay.Text = $"оценок за учебный день: {gradesPerDay:F2}";
                Logger.Info($"Рассчитано среднее число оценок за учебный день: {gradesPerDay}");
            }
            catch (Exception ex)
            {
                lblWorkingDays.Text = "учебных дней: н/д";
                lblGradesPerDay.Text = "оценок за учебный день: н/д";
                MessageBox.Show(ex.Message, "Сервис календаря недоступен",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnLoadCalendar.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv",
                FileName = "Статистика_успеваемости.csv"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using (StreamWriter writer = new StreamWriter(saveDialog.FileName, false, new UTF8Encoding(true)))
                {
                    writer.WriteLine("Показатель;Значение");
                    writer.WriteLine($"Средний балл;{lblAverage.Text}");
                    writer.WriteLine($"Дисперсия;{lblVariance.Text}");
                    writer.WriteLine($"Стандартное отклонение;{lblStdDev.Text}");
                    writer.WriteLine($"Медиана;{lblMedian.Text}");
                    writer.WriteLine($"Минимальная оценка;{lblMin.Text}");
                    writer.WriteLine($"Максимальная оценка;{lblMax.Text}");
                    writer.WriteLine($"Количество оценок;{lblCount.Text}");
                    writer.WriteLine();
                    writer.WriteLine("Студент;Средний балл;Количество оценок");

                    foreach (DataGridViewRow row in dgvStudents.Rows)
                    {
                        writer.WriteLine($"{row.Cells["ФИО"].Value};" +
                                         $"{row.Cells["СреднийБалл"].Value};" +
                                         $"{row.Cells["КоличествоОценок"].Value}");
                    }
                }

                Logger.Info("Статистика экспортирована в CSV");
                MessageBox.Show("Статистика экспортирована в CSV!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка экспорта статистики в CSV", ex);
                MessageBox.Show($"Не удалось сохранить файл: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
