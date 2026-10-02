using System;
using System.IO;

namespace EndForge.Logging
{
    public static class AppEventLogger
    {
        private static readonly string _logPath = Path.Combine(AppContext.BaseDirectory, "app_events.log");

        public static void Log(string message)
        {
            try
            {
                var line = $"{DateTime.UtcNow:O} {message}{Environment.NewLine}";
                File.AppendAllText(_logPath, line);
            }
            catch
            {
                // No fallar la aplicación por errores de logging
            }
        }
    }
}
