using System;
using System.Windows.Forms;
using GradeManagementSystem.Forms;
using GradeManagementSystem.Helpers;
using GradeManagementSystem.Models;

namespace GradeManagementSystem
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Logger.Initialize();
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                using (LoginForm loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() != DialogResult.OK)
                    {
                        Logger.Info("Вход отменён пользователем, приложение закрыто");
                        return;
                    }

                    User user = loginForm.Tag as User;
                    if (user == null)
                    {
                        MessageBox.Show("Не удалось получить данные пользователя.", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Application.Run(new MainForm(user));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Критическая ошибка при запуске приложения", ex);
                MessageBox.Show($"Критическая ошибка при запуске приложения: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Logger.Error("Необработанное исключение в потоке интерфейса", e.Exception);
            MessageBox.Show($"Произошла непредвиденная ошибка: {e.Exception.Message}",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            if (exception != null)
                Logger.Error("Необработанное исключение в домене приложения", exception);
        }
    }
}
