using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using GradeManagementSystem.Forms;
using GradeManagementSystem.Helpers;
using GradeManagementSystem.Models;

namespace ScreenshotTool
{
    internal static class Program
    {
        private static string outputDirectory;

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            outputDirectory = args.Length > 0
                ? Path.GetFullPath(args[0])
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screenshots");
            Directory.CreateDirectory(outputDirectory);

            Application.ThreadException += (sender, e) =>
            {
                Log("THREAD EXCEPTION: " + e.Exception);
                Environment.Exit(2);
            };

            try
            {
                Log("Start, output=" + outputDirectory);
                User admin = LoadUser("ertine");
                User teacher = LoadUser("teacher");
                int gradeID = GetLastGradeID();

                Capture(() => new LoginForm(), "01_login.png", form =>
                {
                    SetFieldText(form, "txtLogin", "ertine");
                    SetFieldText(form, "txtPassword", "1234");
                });

                Capture(() => new MainForm(admin), "02_main.png");

                Capture(() => new StudentsForm(), "03_students.png");

                Capture(() => new StudentEditForm(null), "04_student_edit_validation.png", form =>
                {
                    form.Activate();
                    Pump(300);
                    ClickButton(form, "Сохранить");
                    Pump(600);

                    FieldInfo providerField = form.GetType().GetField("errorProvider",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    ErrorProvider provider = providerField == null ? null : providerField.GetValue(form) as ErrorProvider;
                    FieldInfo lastNameField = form.GetType().GetField("txtLastName",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    TextBox lastName = lastNameField == null ? null : lastNameField.GetValue(form) as TextBox;

                    if (provider != null && lastName != null)
                    {
                        Log("validation error text: '" + provider.GetError(lastName) + "'");
                        provider.SetIconAlignment(lastName, ErrorIconAlignment.MiddleRight);
                        provider.SetIconPadding(lastName, -20);
                    }

                    foreach (string fieldName in new[] { "txtLastName", "txtFirstName", "txtMiddleName", "cmbGroups" })
                    {
                        FieldInfo field = form.GetType().GetField(fieldName,
                            BindingFlags.Instance | BindingFlags.NonPublic);
                        Control control = field == null ? null : field.GetValue(form) as Control;
                        if (provider == null || control == null)
                            continue;

                        string message = provider.GetError(control);
                        if (!string.IsNullOrEmpty(message))
                        {
                            provider.SetIconAlignment(control, ErrorIconAlignment.MiddleRight);
                            provider.SetIconPadding(control, -20);
                            provider.SetError(control, string.Empty);
                            provider.SetError(control, message);
                        }
                    }
                }, true);

                Capture(() => new GradesForm(admin), "05_grades.png");

                Capture(() => new GradeEditForm(teacher, gradeID), "06_grade_edit.png");

                Capture(() => new StatisticsForm(admin), "07_statistics.png", form =>
                {
                    ClickButton(form, "Рассчитать");
                });

                Capture(() => new ReportForm(admin), "08_report.png", form =>
                {
                    ClickButton(form, "Сформировать");
                });

                Capture(() => new VisualizationForm(admin), "09_chart_dynamic.png");

                Capture(() => new VisualizationForm(admin), "10_chart_distribution.png", form =>
                {
                    SelectTab(form, 1);
                });

                Capture(() => new VisualizationForm(admin), "11_chart_groups.png", form =>
                {
                    SelectTab(form, 2);
                });

                Console.WriteLine("Screenshots saved to " + outputDirectory);
                Log("Done");
            }
            catch (Exception ex)
            {
                Log("ERROR: " + ex);
                Console.WriteLine("ERROR: " + ex);
                Environment.ExitCode = 1;
            }
        }

        private static void Log(string message)
        {
            try
            {
                File.AppendAllText(Path.Combine(outputDirectory, "screenshot.log"),
                    DateTime.Now.ToString("HH:mm:ss") + " " + message + Environment.NewLine);
            }
            catch
            {
            }
        }

        private static void Capture(Func<Form> factory, string fileName, Action<Form> prepare = null, bool useScreen = false)
        {
            Log("capture " + fileName + " begin");
            using (Form form = factory())
            {
                form.StartPosition = FormStartPosition.Manual;
                if (form.WindowState != FormWindowState.Maximized)
                    form.Location = new Point(60, 40);

                form.TopMost = true;
                form.Show();
                Pump(450);

                if (prepare != null)
                {
                    prepare(form);
                    Pump(700);
                }

                Pump(350);

                Rectangle bounds = form.Bounds;
                bounds.Intersect(Screen.FromControl(form).Bounds);

                using (Bitmap bitmap = new Bitmap(form.Width, form.Height))
                {
                    if (useScreen)
                    {
                        RECT rect;
                        DwmGetWindowAttribute(form.Handle, 9, out rect, Marshal.SizeOf(typeof(RECT)));
                        Rectangle visible = Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
                        visible.Intersect(Screen.FromControl(form).Bounds);

                        using (Bitmap screenBitmap = new Bitmap(visible.Width, visible.Height))
                        {
                            using (Graphics screenGraphics = Graphics.FromImage(screenBitmap))
                                screenGraphics.CopyFromScreen(visible.Location, Point.Empty, visible.Size);

                            SaveBitmap(screenBitmap, fileName);
                        }
                    }
                    else
                    {
                        bool printed = false;
                        using (Graphics graphics = Graphics.FromImage(bitmap))
                        {
                            IntPtr hdc = graphics.GetHdc();
                            try
                            {
                                printed = PrintWindow(form.Handle, hdc, 2);
                            }
                            finally
                            {
                                graphics.ReleaseHdc(hdc);
                            }
                        }

                        if (!printed || IsBlank(bitmap))
                        {
                            using (Graphics graphics = Graphics.FromImage(bitmap))
                                graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
                        }

                        SaveBitmap(bitmap, fileName);
                    }
                }

                form.Hide();
                Pump(150);
                Log("capture " + fileName + " end");
            }
        }

        private static void SaveBitmap(Bitmap bitmap, string fileName)
        {
            string path = Path.Combine(outputDirectory, fileName);
            using (Bitmap trimmed = TrimBlackBorder(bitmap))
            {
                trimmed.Save(path, ImageFormat.Png);
                Console.WriteLine("saved " + path + " (" + trimmed.Width + "x" + trimmed.Height + ")");
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, int attribute, out RECT rect, int size);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint flags);

        private static bool IsBlank(Bitmap bitmap)
        {
            for (int y = 0; y < bitmap.Height; y += 10)
            {
                for (int x = 0; x < bitmap.Width; x += 10)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    if (pixel.R != 0 || pixel.G != 0 || pixel.B != 0)
                        return false;
                }
            }
            return true;
        }

        private static Bitmap TrimBlackBorder(Bitmap source)
        {
            int left = 0, top = 0, right = source.Width - 1, bottom = source.Height - 1;

            int maxX = source.Width - 1;
            int maxY = source.Height - 1;

            while (left < right && IsDarkLine(source, left, 0, maxY, true)) left++;
            while (right > left && IsDarkLine(source, right, 0, maxY, true)) right--;
            while (top < bottom && IsDarkLine(source, top, 0, maxX, false)) top++;
            while (bottom > top && IsDarkLine(source, bottom, 0, maxX, false)) bottom--;

            if (left == 0 && top == 0 && right == source.Width - 1 && bottom == source.Height - 1)
                return (Bitmap)source.Clone();

            Rectangle rect = new Rectangle(left, top, right - left + 1, bottom - top + 1);
            return source.Clone(rect, source.PixelFormat);
        }

        private static bool IsDarkLine(Bitmap bitmap, int index, int from, int to, bool vertical)
        {
            int total = 0;
            int dark = 0;

            if (vertical)
            {
                for (int y = from; y <= to; y++)
                {
                    total++;
                    Color pixel = bitmap.GetPixel(index, y);
                    if (pixel.R + pixel.G + pixel.B < 45)
                        dark++;
                }
            }
            else
            {
                for (int x = from; x <= to; x++)
                {
                    total++;
                    Color pixel = bitmap.GetPixel(x, index);
                    if (pixel.R + pixel.G + pixel.B < 45)
                        dark++;
                }
            }

            return total > 0 && dark >= total * 0.90;
        }

        private static void Pump(int milliseconds)
        {
            DateTime deadline = DateTime.Now.AddMilliseconds(milliseconds);
            while (DateTime.Now < deadline)
            {
                Application.DoEvents();
                Thread.Sleep(25);
            }
            Application.DoEvents();
        }

        private static void SetFieldText(Form form, string fieldName, string text)
        {
            FieldInfo field = form.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            TextBox box = field == null ? null : field.GetValue(form) as TextBox;
            if (box != null)
                box.Text = text;
        }

        private static void ClickButton(Control parent, string text)
        {
            Button button = FindButton(parent, text);
            if (button != null)
                button.PerformClick();
        }

        private static Button FindButton(Control parent, string text)
        {
            foreach (Control child in parent.Controls)
            {
                Button button = child as Button;
                if (button != null && button.Text == text)
                    return button;

                Button nested = FindButton(child, text);
                if (nested != null)
                    return nested;
            }
            return null;
        }

        private static void SelectTab(Control parent, int index)
        {
            foreach (Control child in parent.Controls)
            {
                TabControl tabs = child as TabControl;
                if (tabs != null)
                {
                    tabs.SelectedIndex = index;
                    return;
                }

                SelectTab(child, index);
            }
        }

        private static User LoadUser(string login)
        {
            string query = "SELECT UserID, Login, Password, Role, FullName, GroupID, CreatedDate " +
                           "FROM Users WHERE Login = @Login";

            using (SqlConnection connection = new SqlConnection(AppConfig.ConnectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Login", login);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new InvalidOperationException("Пользователь не найден: " + login);

                    int? groupID = reader["GroupID"] == DBNull.Value
                        ? (int?)null
                        : Convert.ToInt32(reader["GroupID"]);

                    return new User(
                        Convert.ToInt32(reader["UserID"]),
                        reader["Login"].ToString(),
                        reader["Password"].ToString(),
                        reader["Role"].ToString(),
                        reader["FullName"].ToString(),
                        groupID,
                        Convert.ToDateTime(reader["CreatedDate"]));
                }
            }
        }

        private static int GetLastGradeID()
        {
            using (SqlConnection connection = new SqlConnection(AppConfig.ConnectionString))
            using (SqlCommand command = new SqlCommand("SELECT MAX(GradeID) FROM Grades", connection))
            {
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }
}
