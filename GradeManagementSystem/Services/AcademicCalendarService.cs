using System;
using System.Net.Http;
using System.Threading.Tasks;
using GradeManagementSystem.Helpers;

namespace GradeManagementSystem.Services
{
    public class AcademicCalendarService
    {
        private static readonly HttpClient httpClient;

        static AcademicCalendarService()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(AppConfig.ApiBaseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(AppConfig.ApiTimeoutSeconds);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "GradeManagementSystem/1.0");
        }

        public async Task<int> GetWorkingDaysCountAsync(int year, int month)
        {
            string requestUrl = $"/api/getdata?year={year}&month={month}&cc=ru";
            Logger.Info($"Запрос к API производственного календаря: {requestUrl}");

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                string data = await response.Content.ReadAsStringAsync();
                data = data.Trim();

                if (data.Length < 28)
                {
                    Logger.Warning($"API вернул код ошибки вместо данных: {data}");
                    throw new InvalidOperationException(
                        $"Сервис календаря вернул код ошибки: {data}");
                }

                int workingDays = 0;
                foreach (char symbol in data)
                {
                    if (symbol == '0' || symbol == '2' || symbol == '4')
                        workingDays++;
                }

                Logger.Info($"Получен производственный календарь: {year}-{month:D2}, " +
                            $"учебных дней: {workingDays}");
                return workingDays;
            }
            catch (TaskCanceledException)
            {
                Logger.Warning("Превышен таймаут запроса к API производственного календаря");
                throw new TimeoutException(
                    "Сервис производственного календаря не ответил за отведённое время.");
            }
            catch (HttpRequestException ex)
            {
                Logger.Error("Ошибка подключения к API производственного календаря", ex);
                throw new InvalidOperationException(
                    "Не удалось подключиться к сервису производственного календаря. " +
                    "Проверьте подключение к сети Интернет.");
            }
        }
    }
}
