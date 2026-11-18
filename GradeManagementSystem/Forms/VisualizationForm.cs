using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using GradeManagementSystem.Data;
using GradeManagementSystem.Helpers;
using GradeManagementSystem.Models;

namespace GradeManagementSystem.Forms
{
    public class VisualizationForm : Form
    {
        private readonly User currentUser;
        private readonly GradeRepository repository = new GradeRepository();
        private readonly LookupRepository lookupRepository = new LookupRepository();

        private TabControl tabs;
        private Chart chartDynamic;
        private Chart chartDistribution;
        private Chart chartGroups;

        private ComboBox cmbGroups;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;

        private Button btnBuild;
        private Button btnSaveChart;

        public VisualizationForm(User user)
        {
            currentUser = user;
            UiTheme.ApplyForm(this);
            Text = "Визуализация данных";
            ClientSize = new Size(1180, 740);
            MinimumSize = new Size(1000, 640);

            BuildUi();
            LoadLookups();
            ApplyRoleRestrictions();
            BuildCharts();
        }

        private void BuildUi()
        {
            Panel header = UiTheme.Header("Визуализация данных",
                                          "Динамика успеваемости, распределение оценок и сравнение групп");

            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Background,
                Padding = new Padding(18, 12, 18, 12)
            };

            Panel filters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BackColor = UiTheme.Surface,
                Padding = new Padding(14, 6, 14, 6)
            };

            Label groupCaption = new Label
            {
                Text = "Группа",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(14, 8)
            };
            filters.Controls.Add(groupCaption);

            cmbGroups = UiTheme.Combo(180);
            cmbGroups.Location = new Point(14, 28);
            filters.Controls.Add(cmbGroups);

            Label startCaption = new Label
            {
                Text = "Начало периода",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(210, 8)
            };
            filters.Controls.Add(startCaption);

            dtpStartDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = UiTheme.BaseFont,
                Location = new Point(210, 28),
                Width = 130,
                Value = new DateTime(DateTime.Today.Year, 3, 1)
            };
            filters.Controls.Add(dtpStartDate);

            Label endCaption = new Label
            {
                Text = "Конец периода",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(356, 8)
            };
            filters.Controls.Add(endCaption);

            dtpEndDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = UiTheme.BaseFont,
                Location = new Point(356, 28),
                Width = 130,
                Value = DateTime.Today
            };
            filters.Controls.Add(dtpEndDate);

            btnBuild = UiTheme.PrimaryButton("Построить");
            btnBuild.Width = 130;
            btnBuild.Location = new Point(504, 26);
            btnBuild.Click += btnBuild_Click;
            filters.Controls.Add(btnBuild);

            btnSaveChart = UiTheme.SecondaryButton("Сохранить график");
            btnSaveChart.Width = 160;
            btnSaveChart.Location = new Point(644, 26);
            btnSaveChart.Click += btnSaveChart_Click;
            filters.Controls.Add(btnSaveChart);


            tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.BaseFont
            };

            TabPage pageDynamic = new TabPage("Динамика");
            pageDynamic.BackColor = UiTheme.Surface;
            chartDynamic = CreateChart();
            pageDynamic.Controls.Add(chartDynamic);

            TabPage pageDistribution = new TabPage("Распределение");
            pageDistribution.BackColor = UiTheme.Surface;
            chartDistribution = CreateChart();
            pageDistribution.Controls.Add(chartDistribution);

            TabPage pageGroups = new TabPage("Сравнение групп");
            pageGroups.BackColor = UiTheme.Surface;
            chartGroups = CreateChart();
            pageGroups.Controls.Add(chartGroups);

            tabs.TabPages.Add(pageDynamic);
            tabs.TabPages.Add(pageDistribution);
            tabs.TabPages.Add(pageGroups);

            content.Controls.Add(tabs);
            content.Controls.Add(filters);

            Controls.Add(content);
            Controls.Add(header);
        }

        private Chart CreateChart()
        {
            Chart chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Palette = ChartColorPalette.BrightPastel
            };

            ChartArea area = new ChartArea("MainArea");
            area.BackColor = UiTheme.Surface;
            area.AxisX.MajorGrid.LineColor = UiTheme.Border;
            area.AxisY.MajorGrid.LineColor = UiTheme.Border;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 9F);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 9F);
            area.AxisX.TitleFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            area.AxisY.TitleFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            area.AxisX.Title = "";
            area.AxisY.Title = "";
            chart.ChartAreas.Add(area);

            Legend legend = new Legend("MainLegend")
            {
                Docking = Docking.Bottom,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.Transparent
            };
            chart.Legends.Add(legend);

            Title title = new Title(string.Empty)
            {
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = UiTheme.TextPrimary
            };
            chart.Titles.Add(title);

            return chart;
        }

        private void LoadLookups()
        {
            DataTable groups = new DataTable();
            groups.Columns.Add("GroupID", typeof(int));
            groups.Columns.Add("GroupName", typeof(string));
            groups.Rows.Add(0, "Все группы");

            foreach (Group group in lookupRepository.GetGroups())
                groups.Rows.Add(group.GroupID, group.GroupName);

            cmbGroups.DataSource = groups;
            cmbGroups.DisplayMember = "GroupName";
            cmbGroups.ValueMember = "GroupID";
            cmbGroups.SelectedIndex = 0;
        }

        private void ApplyRoleRestrictions()
        {
            if (!currentUser.IsCurator)
                return;

            if (currentUser.GroupID.HasValue)
            {
                cmbGroups.SelectedValue = currentUser.GroupID.Value;
                cmbGroups.Enabled = false;
            }
        }

        private int? SelectedGroupID
        {
            get
            {
                if (cmbGroups.SelectedValue == null)
                    return null;

                int groupID = Convert.ToInt32(cmbGroups.SelectedValue);
                return groupID == 0 ? (int?)null : groupID;
            }
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            if (dtpStartDate.Value.Date > dtpEndDate.Value.Date)
            {
                MessageBox.Show("Дата начала периода не может быть позже даты окончания.",
                    "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                LoadDynamicChart();
                LoadDistributionChart();
                LoadGroupComparisonChart();
                Logger.Info("Построены диаграммы визуализации данных");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка построения диаграмм", ex);
                MessageBox.Show($"Не удалось построить диаграммы: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BuildCharts()
        {
            try
            {
                LoadDynamicChart();
                LoadDistributionChart();
                LoadGroupComparisonChart();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка начального построения диаграмм", ex);
            }
        }

        private void LoadDynamicChart()
        {
            int? groupID = SelectedGroupID;
            DateTime startDate = dtpStartDate.Value.Date;
            DateTime endDate = dtpEndDate.Value.Date;

            DataTable data = repository.GetMonthlyAverage(groupID, startDate, endDate);

            chartDynamic.Series.Clear();
            chartDynamic.Titles[0].Text = "Динамика среднего балла по месяцам";

            ChartArea area = chartDynamic.ChartAreas["MainArea"];
            area.AxisX.Title = "Месяц";
            area.AxisY.Title = "Средний балл";
            area.AxisY.Minimum = 2;
            area.AxisY.Maximum = 5;
            area.AxisY.Interval = 0.5;
            area.AxisX.LabelStyle.Angle = -35;
            area.AxisX.IsMarginVisible = true;

            Series series = new Series("Средний балл")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 3,
                Color = UiTheme.Accent,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 9,
                MarkerColor = UiTheme.AccentDark,
                IsValueShownAsLabel = true,
                LabelFormat = "F2",
                Font = new Font("Segoe UI", 8.5F)
            };

            foreach (DataRow row in data.Rows)
            {
                string label = $"{System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Convert.ToInt32(row["Месяц"]))} {row["Год"]}";
                int pointIndex = series.Points.AddXY(label, Convert.ToDouble(row["СреднийБалл"]));
                series.Points[pointIndex].ToolTip = $"Средний балл: {row["СреднийБалл"]}\nОценок: {row["КоличествоОценок"]}";
            }

            chartDynamic.Series.Add(series);
        }

        private void LoadDistributionChart()
        {
            int? groupID = SelectedGroupID;
            DateTime startDate = dtpStartDate.Value.Date;
            DateTime endDate = dtpEndDate.Value.Date;

            DataTable data = repository.GetReportData(startDate, endDate, groupID, null, null, null);

            chartDistribution.Series.Clear();
            chartDistribution.Titles[0].Text = "Распределение оценок";

            ChartArea area = chartDistribution.ChartAreas["MainArea"];
            area.AxisX.Title = "Оценка";
            area.AxisY.Title = "Количество";
            area.AxisX.LabelStyle.Angle = 0;
            area.AxisY.Minimum = 0;
            area.AxisX.IsMarginVisible = true;

            Series series = new Series("Количество оценок")
            {
                ChartType = SeriesChartType.Column,
                IsValueShownAsLabel = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Color = UiTheme.Accent
            };

            Color[] colors =
            {
                UiTheme.Danger,
                UiTheme.Warning,
                Color.FromArgb(214, 168, 0),
                UiTheme.Accent,
                UiTheme.Success
            };

            int total = data.Rows.Count;
            int maxCount = 0;

            for (int grade = 2; grade <= 5; grade++)
            {
                int count = data.Rows.Cast<DataRow>()
                    .Count(r => Convert.ToInt32(r["Оценка"]) == grade);

                if (count > maxCount)
                    maxCount = count;

                double share = total > 0 ? Math.Round(count * 100.0 / total, 1) : 0;

                int pointIndex = series.Points.AddXY(grade.ToString(), count);
                series.Points[pointIndex].Color = colors[grade - 2];
                series.Points[pointIndex].Label = $"{count}\n({share}%)";
                series.Points[pointIndex].ToolTip = $"Оценка {grade}: {count} ({share}%)";
            }

            area.AxisY.Maximum = NiceMaximum(maxCount * 1.15, out double interval);
            area.AxisY.Interval = interval;

            chartDistribution.Series.Add(series);
        }

        private void LoadGroupComparisonChart()
        {
            DataTable data = repository.GetGroupAverages();

            chartGroups.Series.Clear();
            chartGroups.Titles[0].Text = "Сравнение групп по среднему баллу";

            ChartArea area = chartGroups.ChartAreas["MainArea"];
            area.AxisX.Title = "Группа";
            area.AxisY.Title = "Средний балл";
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 5;
            area.AxisY.Interval = 0.5;
            area.AxisX.IsMarginVisible = true;

            Series averageSeries = new Series("Средний балл")
            {
                ChartType = SeriesChartType.Column,
                IsValueShownAsLabel = true,
                LabelFormat = "F2",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Color = UiTheme.Accent
            };

            Series varianceSeries = new Series("Дисперсия")
            {
                ChartType = SeriesChartType.Line,
                YAxisType = AxisType.Secondary,
                BorderWidth = 2,
                Color = UiTheme.Warning,
                MarkerStyle = MarkerStyle.Diamond,
                MarkerSize = 8,
                IsValueShownAsLabel = false,
                Font = new Font("Segoe UI", 8.5F)
            };

            area.AxisY2.Enabled = AxisEnabled.True;
            area.AxisY2.Title = "Дисперсия";
            area.AxisY2.TitleFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            area.AxisY2.LabelStyle.Font = new Font("Segoe UI", 9F);
            area.AxisY2.Minimum = 0;
            area.AxisY2.Maximum = 1;
            area.AxisY2.Interval = 0.2;

            foreach (DataRow row in data.Rows)
            {
                string groupName = row["Группа"].ToString();
                double average = Convert.ToDouble(row["СреднийБалл"]);
                double variance = Convert.ToDouble(row["Дисперсия"]);

                int pointIndex = averageSeries.Points.AddXY(groupName, average);
                averageSeries.Points[pointIndex].ToolTip =
                    $"Группа: {groupName}\nСредний балл: {average:F2}\nОценок: {row["КоличествоОценок"]}";

                if (average >= 4.5)
                    averageSeries.Points[pointIndex].Color = UiTheme.Success;
                else if (average < 3.5)
                    averageSeries.Points[pointIndex].Color = UiTheme.Warning;

                varianceSeries.Points.AddXY(groupName, variance);
            }

            chartGroups.Series.Add(averageSeries);
            chartGroups.Series.Add(varianceSeries);
        }

        private void btnSaveChart_Click(object sender, EventArgs e)
        {
            Chart chart = GetCurrentChart();
            if (chart == null)
                return;

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "PNG изображение (*.png)|*.png|JPEG изображение (*.jpg)|*.jpg",
                FileName = $"Диаграмма_{tabs.SelectedTab.Text}.png"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                ChartImageFormat format = Path.GetExtension(saveDialog.FileName).ToLowerInvariant() == ".jpg"
                    ? ChartImageFormat.Jpeg
                    : ChartImageFormat.Png;

                chart.SaveImage(saveDialog.FileName, format);
                Logger.Info($"Диаграмма сохранена в файл: {saveDialog.FileName}");
                MessageBox.Show("Диаграмма сохранена в файл.", "Сохранение",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения диаграммы", ex);
                MessageBox.Show($"Не удалось сохранить диаграмму: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Chart GetCurrentChart()
        {
            switch (tabs.SelectedIndex)
            {
                case 0:
                    return chartDynamic;
                case 1:
                    return chartDistribution;
                case 2:
                    return chartGroups;
                default:
                    return null;
            }
        }

        private static double NiceMaximum(double value, out double interval)
        {
            if (value <= 0)
            {
                interval = 1;
                return 10;
            }

            double rawStep = value / 5.0;
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
            double fraction = rawStep / magnitude;

            double niceFraction;
            if (fraction <= 1)
                niceFraction = 1;
            else if (fraction <= 2)
                niceFraction = 2;
            else if (fraction <= 5)
                niceFraction = 5;
            else
                niceFraction = 10;

            interval = niceFraction * magnitude;
            return Math.Ceiling(value / interval) * interval;
        }
    }
}
