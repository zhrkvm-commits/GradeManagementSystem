using System.Drawing;
using System.Windows.Forms;

namespace GradeManagementSystem.Helpers
{
    public static class UiTheme
    {
        public static readonly Font BaseFont = new Font("Segoe UI", 9F);
        public static readonly Font TitleFont = new Font("Segoe UI", 15F, FontStyle.Bold);
        public static readonly Font SubtitleFont = new Font("Segoe UI", 9F);
        public static readonly Font SectionFont = new Font("Segoe UI", 10F, FontStyle.Bold);

        public static readonly Color Accent = Color.FromArgb(0, 114, 160);
        public static readonly Color AccentDark = Color.FromArgb(0, 88, 124);
        public static readonly Color Background = Color.FromArgb(242, 245, 248);
        public static readonly Color Surface = Color.White;
        public static readonly Color Border = Color.FromArgb(214, 220, 226);
        public static readonly Color TextPrimary = Color.FromArgb(31, 41, 55);
        public static readonly Color TextSecondary = Color.FromArgb(107, 114, 128);
        public static readonly Color Success = Color.FromArgb(22, 128, 74);
        public static readonly Color Warning = Color.FromArgb(194, 120, 0);
        public static readonly Color Danger = Color.FromArgb(180, 40, 40);

        public static void ApplyForm(Form form)
        {
            form.Font = BaseFont;
            form.BackColor = Background;
            form.ForeColor = TextPrimary;
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.StartPosition = FormStartPosition.CenterScreen;
        }

        public static Panel Header(string title, string subtitle)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 74,
                BackColor = Accent,
                Padding = new Padding(20, 10, 20, 8)
            };

            Label titleLabel = new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = TitleFont,
                AutoSize = true,
                Location = new Point(20, 10)
            };

            Label subtitleLabel = new Label
            {
                Text = subtitle,
                ForeColor = Color.FromArgb(220, 238, 246),
                Font = SubtitleFont,
                AutoSize = true,
                Location = new Point(22, 44)
            };

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(subtitleLabel);
            return panel;
        }

        public static Button PrimaryButton(string text)
        {
            return new Button
            {
                Text = text,
                Font = BaseFont,
                BackColor = Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 34,
                Width = 132,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
        }

        public static Button SecondaryButton(string text)
        {
            return new Button
            {
                Text = text,
                Font = BaseFont,
                BackColor = Surface,
                ForeColor = TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Height = 34,
                Width = 132,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
        }

        public static void StyleFlatButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.BorderSize = 1;
        }

        public static Label FieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = BaseFont,
                ForeColor = TextSecondary,
                AutoSize = true,
                Margin = new Padding(0, 8, 8, 4)
            };
        }

        public static Label ValueLabel(string text, float size = 16F)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", size, FontStyle.Bold),
                ForeColor = TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 2)
            };
        }

        public static TextBox TextBox(int width = 220)
        {
            return new TextBox
            {
                Font = BaseFont,
                Width = width,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        public static ComboBox Combo(int width = 220)
        {
            return new ComboBox
            {
                Font = BaseFont,
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
        }

        public static DataGridView Grid()
        {
            DataGridView grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Surface,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                GridColor = Border,
                Font = BaseFont
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(233, 239, 244);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 239, 244);
            grid.ColumnHeadersHeight = 34;
            grid.RowTemplate.Height = 28;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 251, 252);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(205, 231, 242);
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;

            return grid;
        }

        public static GroupBox Card(string title, int height)
        {
            return new GroupBox
            {
                Text = title,
                Font = SectionFont,
                ForeColor = TextPrimary,
                BackColor = Surface,
                Height = height,
                Dock = DockStyle.Top,
                Padding = new Padding(12, 8, 12, 8)
            };
        }
    }
}
