using System;
using System.Configuration;

namespace GradeManagementSystem.Helpers
{
    public static class AppConfig
    {
        public static string ConnectionString
        {
            get
            {
                ConnectionStringSettings settings =
                    ConfigurationManager.ConnectionStrings["GradeManagementDB"];

                if (settings == null || string.IsNullOrEmpty(settings.ConnectionString))
                    throw new ConfigurationErrorsException(
                        "В конфигурационном файле не найдена строка подключения GradeManagementDB");

                return settings.ConnectionString;
            }
        }

        public static string ApiBaseUrl
        {
            get { return GetString("ApiBaseUrl", "https://isdayoff.ru"); }
        }

        public static int ApiTimeoutSeconds
        {
            get { return GetInt("ApiTimeoutSeconds", 10); }
        }

        public static int PageSize
        {
            get { return GetInt("PageSize", 50); }
        }

        public static int DefaultCourse
        {
            get { return GetInt("DefaultCourse", 1); }
        }

        private static string GetString(string key, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private static int GetInt(string key, int defaultValue)
        {
            int result;
            string value = ConfigurationManager.AppSettings[key];
            return int.TryParse(value, out result) && result > 0 ? result : defaultValue;
        }
    }
}
