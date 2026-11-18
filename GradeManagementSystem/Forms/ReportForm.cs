using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GradeManagementSystem.Data;
using GradeManagementSystem.Helpers;
using GradeManagementSystem.Models;

namespace GradeManagementSystem.Forms
{
    public class ReportForm : Form
    {
        private readonly User currentUser;
        private readonly GradeRepository repository = new GradeRepository();
        private readonly LookupRepository lookupRepository = new LookupRepository();

        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private ComboBox cmbGroups;
        private ComboBox cmbSubjects;
        private ComboBox cmbSemesters;
        private DataGridView dgvReport;
        private Label lblCount;
        private Label lblAverage;
        private Label lblVariance;
        private DataTable reportTable;

        private readonly PrintDocument printDocument = new PrintDocument();
        private readonly PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
        private int printRowIndex;

        public ReportForm(User user)
        {
            currentUser = user;
            UiTheme.ApplyForm(this);
            Text = "Ведомость успеваемости за период";
            ClientSize = new Size(1180, 720);
            MinimumSize = new Size(1000, 620);

            BuildUi();
            LoadLookups();
            ApplyRoleRestrictions();

            printDocument.PrintPage += printDocument_PrintPage;
            printPreviewDialog.Document = printDocument;
        }

        private void BuildUi()
        {
            Panel header = UiTheme.Header("Ведомость успеваемости за период",
                                          "Формирование отчёта, экспорт в CSV и печать с сохранением в PDF");
            header.Dock = DockStyle.Top;

            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Background,
                Padding = new Padding(18, 12, 18, 12)
            };

            GroupBox filters = new GroupBox
            {
                Text = "Параметры отчёта",
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

            dtpStartDate = new DateTimePicker
            {
                Font = UiTheme.BaseFont,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy",
                Width = 128
            };

            dtpEndDate = new DateTimePicker
            {
                Font = UiTheme.BaseFont,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy",
                Width = 128,
                Value = DateTime.Today
            };

            cmbGroups = UiTheme.Combo(160);
            cmbSubjects = UiTheme.Combo(190);
            cmbSemesters = UiTheme.Combo(180);

            DateTime today = DateTime.Today;
            dtpStartDate.Value = new DateTime(today.AddMonths(-6).Year, today.AddMonths(-6).Month, 1);

            filterFlow.Controls.Add(CreateFilterCell("Период с", dtpStartDate));
            filterFlow.Controls.Add(CreateFilterCell("по", dtpEndDate));
            filterFlow.Controls.Add(CreateFilterCell("Группа", cmbGroups));
            filterFlow.Controls.Add(CreateFilterCell("Предмет", cmbSubjects));
            filterFlow.Controls.Add(CreateFilterCell("Семестр", cmbSemesters));

            Button btnBuild = UiTheme.PrimaryButton("Сформировать");
            btnBuild.Width = 150;
            btnBuild.Margin = new Padding(8, 24, 6, 0);
            btnBuild.Click += btnBuild_Click;
            filterFlow.Controls.Add(btnBuild);

            Button btnExport = UiTheme.SecondaryButton("Экспорт в CSV");
            btnExport.Width = 150;
            btnExport.Margin = new Padding(0, 24, 6, 0);
            btnExport.Click += btnExport_Click;
            filterFlow.Controls.Add(btnExport);

            Button btnPrint = UiTheme.SecondaryButton("Печать / PDF");
            btnPrint.Width = 150;
            btnPrint.Margin = new Padding(0, 24, 0, 0);
            btnPrint.Click += btnPrint_Click;
            filterFlow.Controls.Add(btnPrint);

            filters.Controls.Add(filterFlow);

            Panel totals = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = UiTheme.Surface,
                Padding = new Padding(14, 8, 14, 6)
            };

            Label totalsCaption = new Label
            {
                Text = "Итоги выборки:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(14, 14)
            };
            totals.Controls.Add(totalsCaption);

            lblCount = CreateTotalsLabel("Оценок: 0", 130);
            lblAverage = CreateTotalsLabel("Средний балл: —", 290);
            lblVariance = CreateTotalsLabel("Дисперсия: —", 500);
            totals.Controls.Add(lblCount);
            totals.Controls.Add(lblAverage);
            totals.Controls.Add(lblVariance);

            GroupBox gridBox = new GroupBox
            {
                Text = "Ведомость успеваемости за период",
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Font = UiTheme.SectionFont,
                ForeColor = UiTheme.TextPrimary,
                Padding = new Padding(12, 10, 12, 12)
            };

            dgvReport = UiTheme.Grid();
            gridBox.Controls.Add(dgvReport);

            content.Controls.Add(gridBox);
            content.Controls.Add(totals);
            content.Controls.Add(filters);

            Controls.Add(content);
            Controls.Add(header);
        }

        private Label CreateTotalsLabel(string text, int x)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = UiTheme.Accent,
                AutoSize = true,
                Location = new Point(x, 14)
            };
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

        private void LoadLookups()
        {
            var groups = lookupRepository.GetGroups();
            cmbGroups.DataSource = groups;
            cmbGroups.DisplayMember = "GroupName";
            cmbGroups.ValueMember = "GroupID";
            cmbGroups.SelectedIndex = -1;

            var subjects = lookupRepository.GetSubjects();
            cmbSubjects.DataSource = subjects;
            cmbSubjects.DisplayMember = "SubjectName";
            cmbSubjects.ValueMember = "SubjectID";
            cmbSubjects.SelectedIndex = -1;

            var semesters = lookupRepository.GetSemesters();
            cmbSemesters.DataSource = semesters;
            cmbSemesters.DisplayMember = "SemesterName";
            cmbSemesters.ValueMember = "SemesterID";
            cmbSemesters.SelectedIndex = -1;
        }

        private void ApplyRoleRestrictions()
        {
            if (!currentUser.IsCurator)
                return;

            if (currentUser.GroupID.HasValue)
                cmbGroups.SelectedValue = currentUser.GroupID.Value;

            cmbGroups.Enabled = false;
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            int? groupID = cmbGroups.SelectedIndex != -1 ? (int?)cmbGroups.SelectedValue : null;
            int? subjectID = cmbSubjects.SelectedIndex != -1 ? (int?)cmbSubjects.SelectedValue : null;
            int? semesterID = cmbSemesters.SelectedIndex != -1 ? (int?)cmbSemesters.SelectedValue : null;
            int? teacherID = currentUser.IsTeacher ? currentUser.UserID : (int?)null;

            if (currentUser.IsCurator)
                groupID = currentUser.GroupID;

            try
            {
                reportTable = repository.GetReportData(dtpStartDate.Value.Date, dtpEndDate.Value.Date,
                                                       groupID, subjectID, semesterID, teacherID);

                if (reportTable.Rows.Count == 0)
                {
                    Logger.Warning("Отчёт не сформирован: выборка пуста");
                    MessageBox.Show("За выбранный период данных не найдено.", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgvReport.DataSource = null;
                    lblCount.Text = "Оценок: 0";
                    lblAverage.Text = "Средний балл: —";
                    lblVariance.Text = "Дисперсия: —";
                    return;
                }

                dgvReport.DataSource = reportTable;

                List<int> grades = new List<int>();
                foreach (DataRow row in reportTable.Rows)
                    grades.Add(Convert.ToInt32(row["Оценка"]));

                lblCount.Text = $"Оценок: {grades.Count}";
                lblAverage.Text = $"Средний балл: {MathStatistics.CalculateAverage(grades):F2}";
                lblVariance.Text = $"Дисперсия: {MathStatistics.CalculateVariance(grades):F2}";

                Logger.Info($"Сформирована ведомость: {grades.Count} оценок, период " +
                            $"{dtpStartDate.Value:dd.MM.yyyy}–{dtpEndDate.Value:dd.MM.yyyy}");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка формирования ведомости", ex);
                MessageBox.Show($"Не удалось сформировать отчёт: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            bool needQuotes = value.Contains(";") || value.Contains("\"") ||
                              value.Contains("\n") || value.Contains("\r");

            string result = value.Replace("\"", "\"\"");
            return needQuotes ? "\"" + result + "\"" : result;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (reportTable == null || reportTable.Rows.Count == 0)
            {
                MessageBox.Show("Сначала сформируйте отчёт.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv",
                FileName = $"Ведомость_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using (StreamWriter writer = new StreamWriter(saveDialog.FileName, false, new UTF8Encoding(true)))
                {
                    writer.WriteLine("ВЕДОМОСТЬ УСПЕВАЕМОСТИ ЗА ПЕРИОД");
                    writer.WriteLine($"Период;{dtpStartDate.Value:dd.MM.yyyy};{dtpEndDate.Value:dd.MM.yyyy}");
                    writer.WriteLine($"Группа;{EscapeCsv(cmbGroups.Text)}");
                    writer.WriteLine($"Предмет;{EscapeCsv(cmbSubjects.Text)}");
                    writer.WriteLine($"Сформировал;{EscapeCsv(currentUser.FullName)}");
                    writer.WriteLine($"Дата формирования;{DateTime.Now:dd.MM.yyyy HH:mm}");
                    writer.WriteLine();

                    List<string> headers = new List<string>();
                    foreach (DataColumn column in reportTable.Columns)
                        headers.Add(EscapeCsv(column.ColumnName));
                    writer.WriteLine(string.Join(";", headers));

                    foreach (DataRow row in reportTable.Rows)
                    {
                        List<string> cells = new List<string>();
                        foreach (object value in row.ItemArray)
                            cells.Add(EscapeCsv(value == null ? string.Empty : value.ToString()));
                        writer.WriteLine(string.Join(";", cells));
                    }

                    writer.WriteLine();
                    writer.WriteLine($"{lblCount.Text};{lblAverage.Text};{lblVariance.Text}");
                }

                Logger.Info($"Ведомость экспортирована в CSV: {saveDialog.FileName}");
                MessageBox.Show("Ведомость экспортирована в CSV!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (IOException ex)
            {
                Logger.Warning($"Файл занят другой программой: {ex.Message}");
                MessageBox.Show("Не удалось сохранить файл. Возможно, он открыт в Microsoft Excel.\n" +
                                "Закройте файл и повторите экспорт.",
                    "Файл занят", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка экспорта ведомости", ex);
                MessageBox.Show($"Не удалось сохранить файл: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (reportTable == null || reportTable.Rows.Count == 0)
            {
                MessageBox.Show("Сначала сформируйте отчёт.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            printRowIndex = 0;
            printDocument.DefaultPageSettings.Landscape = true;
            printPreviewDialog.Document = printDocument;

            if (printPreviewDialog.ShowDialog() == DialogResult.OK)
                printDocument.Print();

            Logger.Info("Ведомость отправлена на печать");
        }

        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font headerFont = new Font("Times New Roman", 14, FontStyle.Bold);
            Font bodyFont = new Font("Times New Roman", 9);
            Font headerCellFont = new Font("Times New Roman", 8, FontStyle.Bold);

            float left = e.MarginBounds.Left;
            float top = e.MarginBounds.Top;
            float y = top;
            float lineHeight = bodyFont.GetHeight(e.Graphics) + 5;

            float[] widths = { 0.22f, 0.09f, 0.18f, 0.07f, 0.10f, 0.20f, 0.14f };
            string[] titles = { "ФИО студента", "Группа", "Предмет", "Оценка", "Дата", "Преподаватель", "Комментарий" };
            float tableWidth = e.MarginBounds.Width;

            e.Graphics.DrawString("ВЕДОМОСТЬ УСПЕВАЕМОСТИ ЗА ПЕРИОД", headerFont, Brushes.Black, left, y);
            y += headerFont.GetHeight(e.Graphics) + 8;
            e.Graphics.DrawString($"Период: {dtpStartDate.Value:dd.MM.yyyy} — {dtpEndDate.Value:dd.MM.yyyy}", bodyFont, Brushes.Black, left, y);
            y += lineHeight;
            e.Graphics.DrawString($"Группа: {cmbGroups.Text}    Предмет: {cmbSubjects.Text}", bodyFont, Brushes.Black, left, y);
            y += lineHeight;
            e.Graphics.DrawString($"Сформировал: {currentUser.FullName}    {DateTime.Now:dd.MM.yyyy HH:mm}", bodyFont, Brushes.Black, left, y);
            y += lineHeight + 6;

            float x = left;
            for (int i = 0; i < titles.Length; i++)
            {
                RectangleF cell = new RectangleF(x, y, tableWidth * widths[i], lineHeight);
                e.Graphics.DrawRectangle(Pens.Gray, cell.X, cell.Y, cell.Width, cell.Height);
                e.Graphics.DrawString(titles[i], headerCellFont, Brushes.Black, cell);
                x += tableWidth * widths[i];
            }
            y += lineHeight;

            while (printRowIndex < reportTable.Rows.Count)
            {
                if (y + lineHeight > e.MarginBounds.Bottom)
                {
                    e.HasMorePages = true;
                    return;
                }

                DataRow row = reportTable.Rows[printRowIndex];
                x = left;

                for (int i = 0; i < reportTable.Columns.Count && i < widths.Length; i++)
                {
                    string text = row[i] == null ? string.Empty : row[i].ToString();
                    if (i == 4 && row[i] is DateTime)
                        text = ((DateTime)row[i]).ToString("dd.MM.yyyy");

                    RectangleF cell = new RectangleF(x, y, tableWidth * widths[i], lineHeight);
                    e.Graphics.DrawRectangle(Pens.LightGray, cell.X, cell.Y, cell.Width, cell.Height);
                    e.Graphics.DrawString(text, bodyFont, Brushes.Black, cell);
                    x += tableWidth * widths[i];
                }

                y += lineHeight;
                printRowIndex++;
            }

            y += 10;
            e.Graphics.DrawString($"{lblCount.Text}    {lblAverage.Text}    {lblVariance.Text}",
                headerCellFont, Brushes.Black, left, y);
            e.HasMorePages = false;
        }
    }
}
