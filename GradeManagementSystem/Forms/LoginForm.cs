using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using GradeManagementSystem.Helpers;
using GradeManagementSystem.Models;

namespace GradeManagementSystem.Forms
{
    public class LoginForm : Form
    {
        private readonly string connectionString = AppConfig.ConnectionString;

        private TextBox txtLogin;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnCancel;
        private Label lblError;

        public LoginForm()
        {
            UiTheme.ApplyForm(this);
            Text = "Авторизация — Система учёта успеваемости";
            ClientSize = new Size(460, 378);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            BuildUi();
            Logger.Info("Открыта форма авторизации");
        }

        private void BuildUi()
        {
            Panel header = UiTheme.Header("Система учёта успеваемости",
                                          "Автоматизация учёта и анализа успеваемости студентов");
            header.Height = 96;
            header.Dock = DockStyle.Top;

            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Padding = new Padding(36, 18, 36, 18)
            };

            Controls.Add(content);
            Controls.Add(header);

            Label lblLogin = new Label
            {
                Text = "Логин",
                Font = UiTheme.BaseFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(36, 22)
            };
            content.Controls.Add(lblLogin);

            txtLogin = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(36, 44),
                Width = 388,
                BorderStyle = BorderStyle.FixedSingle
            };
            content.Controls.Add(txtLogin);

            Label lblPassword = new Label
            {
                Text = "Пароль",
                Font = UiTheme.BaseFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(36, 86)
            };
            content.Controls.Add(lblPassword);

            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(36, 108),
                Width = 388,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };
            content.Controls.Add(txtPassword);

            lblError = new Label
            {
                Text = string.Empty,
                ForeColor = UiTheme.Danger,
                Font = UiTheme.BaseFont,
                Location = new Point(36, 146),
                Width = 388,
                Height = 36
            };
            content.Controls.Add(lblError);

            btnLogin = UiTheme.PrimaryButton("Войти");
            btnLogin.Location = new Point(36, 186);
            btnLogin.Width = 188;
            btnLogin.Height = 38;
            btnLogin.Click += btnLogin_Click;
            content.Controls.Add(btnLogin);

            btnCancel = UiTheme.SecondaryButton("Отмена");
            btnCancel.Location = new Point(236, 186);
            btnCancel.Width = 188;
            btnCancel.Height = 38;
            btnCancel.DialogResult = DialogResult.Cancel;
            content.Controls.Add(btnCancel);

            Label hint = new Label
            {
                Text = "Тестовые учётные записи: ertine / 1234 (администратор), curator / 1234 (куратор), teacher / 1234 (преподаватель)",
                Font = new Font("Segoe UI", 8F),
                ForeColor = UiTheme.TextSecondary,
                Location = new Point(36, 240),
                Width = 388,
                Height = 34
            };
            content.Controls.Add(hint);

            AcceptButton = btnLogin;
            CancelButton = btnCancel;
            ActiveControl = txtLogin;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                Logger.Warning($"Попытка входа с незаполненными полями (логин: '{login}')");
                ShowError("Заполните все поля!");
                return;
            }

            string passwordHash = PasswordHelper.HashPassword(password);
            User user = AuthenticateUser(login, passwordHash);

            if (user != null)
            {
                Logger.Info($"Успешный вход: {user.Login} ({user.Role}), UserID={user.UserID}");
                this.Tag = user;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                Logger.Warning($"Неудачная попытка входа для логина '{login}'");
                ShowError("Неверный логин или пароль!");
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        private User AuthenticateUser(string login, string passwordHash)
        {
            string query = "SELECT UserID, Login, Password, Role, FullName, GroupID, CreatedDate " +
                           "FROM Users WHERE Login = @Login AND Password = @Password";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Login", login);
                cmd.Parameters.AddWithValue("@Password", passwordHash);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
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

            return null;
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
        }
    }
}
